using Microsoft.Extensions.Caching.Distributed;
using Stream_Linkify_Backend.DTOs.Tidal;
using Stream_Linkify_Backend.Enums;
using Stream_Linkify_Backend.Helpers;
using Stream_Linkify_Backend.Interfaces.Tidal;
using System.Text;

namespace Stream_Linkify_Backend.Services.Tidal
{
    public class TidalTokenService(
        IConfiguration config,
        ILogger<TidalTokenService> logger,
        IHttpClientFactory httpClientFactory,
        IDistributedCache cache
    ) : ITidalTokenService
    {
        private const MusicPlatform ProviderName = MusicPlatform.Tidal;
        private readonly SemaphoreSlim sem = new(1, 1);

        public async Task<TidalAccessTokenDto> GetValidTokenAsync()
        {
            await sem.WaitAsync();
            try
            {
                var cached = await TokenCacheHelper.TryGetCachedTokenAsync<TidalAccessTokenDto>(
                    cache,
                    ProviderName,
                    t => t.ExpiresAt);

                if (cached != null)
                {
                    logger.LogDebug("Using cached TIDAL access token");
                    return cached;
                }

                logger.LogInformation("Getting new TIDAL access token");
                var token = await FetchNewTokenAsync();

                await TokenCacheHelper.SetCachedTokenAsync(
                    cache,
                    ProviderName,
                    token,
                    token.ExpiresAt);

                return token;
            }
            catch (Exception ex)
            {
                logger.LogWarning(ex, "Error occurred while getting TIDAL access token");
                throw new Exception($"Error occurred while getting TIDAL token: {ex.Message}");
            }
            finally
            {
                sem.Release();
            }
        }

        private async Task<TidalAccessTokenDto> FetchNewTokenAsync()
        {
            var client = httpClientFactory.CreateClient();
            var clientId = RequiredConfig.Get(config, "Tidal:ClientId");
            var clientSecret = RequiredConfig.Get(config, "Tidal:ClientSecret");

            var url = "https://auth.tidal.com/v1/oauth2/token";

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

            var token = await resp.Content.ReadFromJsonAsync<TidalAccessTokenDto>()
                ?? throw new Exception("Error deserializing TIDAL token JSON");

            token.ExpiresAt = DateTimeOffset.UtcNow
                .AddSeconds(token.ExpiresIn)
                .ToUnixTimeSeconds();

            return token;
        }
    }
}
