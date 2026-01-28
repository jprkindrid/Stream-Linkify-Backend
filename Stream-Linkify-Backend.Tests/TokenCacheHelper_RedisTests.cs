using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using StackExchange.Redis;
using Stream_Linkify_Backend.DTOs.Soundcloud;
using Stream_Linkify_Backend.Helpers;
using Xunit;

namespace Stream_Linkify_Backend.Tests
{
    public enum MusicMockPlatform
    {
        SpotifyMock,
        AppleMusicMock,
        TidalMock,
        DeezerMock,
        SoundcloudMock
    }
    public class TokenCacheHelper_RedisTests
    {
        private static IConfiguration BuildConfig()
        {
            return new ConfigurationBuilder()
                .AddUserSecrets<TokenCacheHelper_RedisTests>(optional: true)
                .AddEnvironmentVariables()
                .Build();
        }

        private static async Task<IDistributedCache?> TryCreateRedisCacheAsync(IConfiguration config)
        {
            var connectionString = config["Redis:ConnectionString"];
            if (string.IsNullOrWhiteSpace(connectionString))
                return null;

            try
            {
                using var mux = await ConnectionMultiplexer.ConnectAsync(connectionString);
                await mux.GetDatabase().PingAsync();

                var services = new ServiceCollection();
                services.AddStackExchangeRedisCache(o => { o.Configuration = connectionString; });

                return services.BuildServiceProvider().GetRequiredService<IDistributedCache>();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Skipping Redis cache tests: {ex.Message}");
                return null;
            }
        }

        [Fact]
        public async Task SetAndGetCachedTokenAsync_RoundTripsToken()
        {
            var config = BuildConfig();
            var cache = await TryCreateRedisCacheAsync(config);
            if (cache == null)
                return;

            var token = CreateMockToken(DateTimeOffset.UtcNow.AddMinutes(30));

            await TokenCacheHelper.SetCachedTokenAsync(
                cache,
                MusicMockPlatform.SoundcloudMock,
                token,
                token.ExpiresAt);

            var cached = await TokenCacheHelper.GetCachedTokenAsync<SoundcloudAccessTokenDto, MusicMockPlatform>(
                cache,
                MusicMockPlatform.SoundcloudMock);

            Assert.NotNull(cached);
            Assert.Equal(token.AccessToken, cached!.AccessToken);
            Assert.Equal(token.RefreshToken, cached.RefreshToken);
        }

        [Fact]
        public async Task TryGetCachedTokenAsync_ReturnsTokenWhenValid()
        {
            var config = BuildConfig();
            var cache = await TryCreateRedisCacheAsync(config);
            if (cache == null)
                return;

            var token = CreateMockToken(DateTimeOffset.UtcNow.AddMinutes(30));

            await TokenCacheHelper.SetCachedTokenAsync(
                cache,
                MusicMockPlatform.SoundcloudMock,
                token,
                token.ExpiresAt);

            var cached = await TokenCacheHelper.TryGetCachedTokenAsync<SoundcloudAccessTokenDto, MusicMockPlatform>(
                cache,
                MusicMockPlatform.SoundcloudMock,
                t => t.ExpiresAt);

            Assert.NotNull(cached);
            Assert.Equal(token.AccessToken, cached!.AccessToken);
        }

        [Fact]
        public async Task TryGetCachedTokenAsync_ReturnsNullWhenExpired()
        {
            var config = BuildConfig();
            var cache = await TryCreateRedisCacheAsync(config);
            if (cache == null)
                return;

            var token = CreateMockToken(DateTimeOffset.UtcNow.AddMinutes(-10));

            await TokenCacheHelper.SetCachedTokenAsync(
                cache,
                MusicMockPlatform.SoundcloudMock,
                token,
                token.ExpiresAt);

            var cached = await TokenCacheHelper.TryGetCachedTokenAsync<SoundcloudAccessTokenDto, MusicMockPlatform>(
                cache,
                MusicMockPlatform.SoundcloudMock,
                t => t.ExpiresAt);

            Assert.Null(cached);
        }

        private static SoundcloudAccessTokenDto CreateMockToken(DateTimeOffset expiresAt)
        {
            return new SoundcloudAccessTokenDto
            {
                AccessToken = "access-token",
                RefreshToken = "refresh-token",
                TokenType = "Bearer",
                ExpiresIn = 3600,
                Scope = "",
                ExpiresAt = expiresAt.ToUnixTimeSeconds()
            };
        }
    }
}
