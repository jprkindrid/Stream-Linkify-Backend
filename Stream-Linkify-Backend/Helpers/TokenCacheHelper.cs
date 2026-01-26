using Microsoft.Extensions.Caching.Distributed;
using Newtonsoft.Json;
using Stream_Linkify_Backend.Enums;

namespace Stream_Linkify_Backend.Helpers
{
    public static class TokenCacheHelper
    {
        private static readonly TimeSpan DefaultBuffer = TimeSpan.FromMinutes(5);
        private static readonly TimeSpan MinimumTtl = TimeSpan.FromSeconds(60);

        public static string GetCacheKey(MusicPlatform provider) => $"Token:{provider}";

        public static async Task<T?> TryGetCachedTokenAsync<T>(
            IDistributedCache cache,
            MusicPlatform provider,
            Func<T, long> getExpiresAt,
            TimeSpan? buffer = null) where T : class
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

        public static async Task<T?> GetCachedTokenAsync<T>(
            IDistributedCache cache,
            MusicPlatform provider) where T : class
        {
            var key = GetCacheKey(provider);
            var cached = await cache.GetStringAsync(key);

            if (string.IsNullOrEmpty(cached))
                return null;

            return JsonConvert.DeserializeObject<T>(cached);
        }

        public static async Task SetCachedTokenAsync<T>(
            IDistributedCache cache,
            MusicPlatform provider,
            T token,
            long expiresAt,
            TimeSpan? buffer = null) where T : class
        {
            var key = GetCacheKey(provider);
            var effectiveBuffer = buffer ?? DefaultBuffer;
            var ttl = CalculateTtl(expiresAt, effectiveBuffer);

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
    }
}
