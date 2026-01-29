using Microsoft.Extensions.Caching.Distributed;
using Newtonsoft.Json;
using Stream_Linkify_Backend.Enums;

namespace Stream_Linkify_Backend.Helpers
{
    public static class TokenCacheHelper
    {
        private static readonly TimeSpan DefaultBuffer = TimeSpan.FromMinutes(5);
        private static readonly TimeSpan MinimumTtl = TimeSpan.FromSeconds(60);
        // Keep tokens in cache longer to preserve refresh tokens after access token expires
        private static readonly TimeSpan RefreshTokenGracePeriod = TimeSpan.FromDays(7);

        public static string GetCacheKey<TProvider>(TProvider provider) where TProvider : Enum
            => $"Token:{provider}";

        public static async Task<T?> TryGetCachedTokenAsync<T, TProvider>(
            IDistributedCache cache,
            TProvider provider,
            Func<T, long> getExpiresAt,
            TimeSpan? buffer = null)
            where T : class
            where TProvider : Enum
        {
            var key = GetCacheKey(provider);
            var cached = await cache.GetStringAsync(key);

            if (string.IsNullOrEmpty(cached))
                return null;

            var token = JsonConvert.DeserializeObject<T>(cached);
            if (token == null)
                return null;

            var expiresAt = getExpiresAt(token);
            var effectiveBuffer = buffer ?? DefaultBuffer;

            if (IsTokenValid(expiresAt, effectiveBuffer))
                return token;

            return null;
        }

        public static async Task<T?> GetCachedTokenAsync<T, TProvider>(
            IDistributedCache cache,
            TProvider provider)
            where T : class
            where TProvider : Enum
        {
            var key = GetCacheKey(provider);
            var cached = await cache.GetStringAsync(key);

            if (string.IsNullOrEmpty(cached))
                return null;

            return JsonConvert.DeserializeObject<T>(cached);
        }

        public static async Task SetCachedTokenAsync<T, TProvider>(
            IDistributedCache cache,
            TProvider provider,
            T token,
            long expiresAt,
            TimeSpan? buffer = null,
            bool preserveForRefresh = true)
            where T : class
            where TProvider : Enum
        {
            var key = GetCacheKey(provider);
            var effectiveBuffer = buffer ?? DefaultBuffer;
            
            // If preserveForRefresh is true, keep the token in cache longer so refresh token can be used
            // even after the access token expires
            var ttl = preserveForRefresh 
                ? CalculateTtlWithGracePeriod(expiresAt) 
                : CalculateTtl(expiresAt, effectiveBuffer);

            var json = JsonConvert.SerializeObject(token);
            var options = new DistributedCacheEntryOptions
            {
                AbsoluteExpirationRelativeToNow = ttl
            };

            await cache.SetStringAsync(key, json, options);
        }

        public static bool IsTokenValid(long expiresAt, TimeSpan? buffer = null)
        {
            var effectiveBuffer = buffer ?? DefaultBuffer;
            var expirationTime = DateTimeOffset.FromUnixTimeSeconds(expiresAt);
            return expirationTime > DateTimeOffset.UtcNow.Add(effectiveBuffer);
        }

        private static TimeSpan CalculateTtl(long expiresAt, TimeSpan buffer)
        {
            var expirationTime = DateTimeOffset.FromUnixTimeSeconds(expiresAt);
            var ttl = expirationTime - DateTimeOffset.UtcNow - buffer;

            return ttl < MinimumTtl ? MinimumTtl : ttl;
        }

        private static TimeSpan CalculateTtlWithGracePeriod(long expiresAt)
        {
            var expirationTime = DateTimeOffset.FromUnixTimeSeconds(expiresAt);
            // Keep in cache for the token lifetime plus grace period for refresh token usage
            var ttl = expirationTime - DateTimeOffset.UtcNow + RefreshTokenGracePeriod;

            return ttl < MinimumTtl ? MinimumTtl : ttl;
        }
    }
}
