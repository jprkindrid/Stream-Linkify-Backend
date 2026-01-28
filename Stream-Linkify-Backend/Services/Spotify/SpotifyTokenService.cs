using Microsoft.Extensions.Caching.Distributed;
using Stream_Linkify_Backend.DTOs.Spotify;
using Stream_Linkify_Backend.Enums;
using Stream_Linkify_Backend.Helpers;
using Stream_Linkify_Backend.Interfaces.Spotify;
using System.Text;

namespace Stream_Linkify_Backend.Services.Spotify
{
    public class SpotifyTokenService(
        IHttpClientFactory httpClientFactory,
        IConfiguration config,
        ILogger<SpotifyTokenService> logger,
        IDistributedCache cache
    ) : ISpotifyTokenService
    {
        private const MusicPlatform ProviderName = MusicPlatform.Spotify;
        private readonly SemaphoreSlim sem = new(1, 1);

        public async Task<SpotifyAccessTokenDto?> GetValidTokenAsync()
        {
            await sem.WaitAsync();
            try
            {
                var cached = await TokenCacheHelper.TryGetCachedTokenAsync<SpotifyAccessTokenDto, MusicPlatform>(
                    cache,
                    ProviderName,
                    t => t.ExpiresAt ?? 0);

                if (cached != null)
                {
                    logger.LogDebug("Using cached Spotify access token");
                    return cached;
                }

                logger.LogInformation("Getting new Spotify access token");
                var token = await FetchNewTokenAsync();

                await TokenCacheHelper.SetCachedTokenAsync(
                    cache,
                    ProviderName,
                    token,
                    token.ExpiresAt ?? 0);

                return token;
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Exception occurred while getting Spotify access token");
                throw new Exception($"Error occurred while getting Spotify token: {ex.Message}");
            }
            finally
            {
                sem.Release();
            }
        }

        private async Task<SpotifyAccessTokenDto> FetchNewTokenAsync()
        {
            var client = httpClientFactory.CreateClient();
            var clientId = RequiredConfig.Get(config, "Spotify:ClientId");
            var clientSecret = RequiredConfig.Get(config, "Spotify:ClientSecret");

            var url = "https://accounts.spotify.com/api/token";
            var req = new HttpRequestMessage(HttpMethod.Post, url);
            req.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue(
                "Basic",
                Convert.ToBase64String(Encoding.UTF8.GetBytes($"{clientId}:{clientSecret}"))
            );

            req.Content = new FormUrlEncodedContent(
            [
                new KeyValuePair<string, string>("grant_type", "client_credentials")
            ]);

            var resp = await client.SendAsync(req);
            resp.EnsureSuccessStatusCode();

            var token = await resp.Content.ReadFromJsonAsync<SpotifyAccessTokenDto>()
                ?? throw new Exception("Error deserializing Spotify token JSON");

            var expiresAt = DateTimeOffset.UtcNow.AddSeconds(token.ExpiresIn).ToUnixTimeSeconds();

            logger.LogDebug("Retrieved new Spotify token expires at {ExpiresAt}",
                DateTimeOffset.FromUnixTimeSeconds(expiresAt));

            return token with { ExpiresAt = expiresAt };
        }
    }
}
