using Microsoft.Extensions.Caching.Distributed;
using Stream_Linkify_Backend.DTOs.Soundcloud;
using Stream_Linkify_Backend.Enums;
using Stream_Linkify_Backend.Helpers;
using Stream_Linkify_Backend.Interfaces.Soundcloud;
using System.Text;

namespace Stream_Linkify_Backend.Services.Soundcloud
{
    public class SoundcloudTokenService(
        IConfiguration config,
        ILogger<SoundcloudTokenService> logger,
        IHttpClientFactory httpClientFactory,
        IDistributedCache cache
    ) : ISoundcloudTokenService
    {
        private const MusicPlatform ProviderName = MusicPlatform.Soundcloud;
        private const string TokenUrl = "https://secure.soundcloud.com/oauth/token";
        private readonly SemaphoreSlim sem = new(1, 1);

        public async Task<SoundcloudAccessTokenDto> GetValidTokenAsync()
        {
            await sem.WaitAsync();
            try
            {
                var cached = await TokenCacheHelper.TryGetCachedTokenAsync<SoundcloudAccessTokenDto>(
                    cache,
                    ProviderName,
                    t => t.ExpiresAt);

                if (cached != null)
                {
                    logger.LogDebug("Using cached Soundcloud access token");
                    return cached;
                }

                logger.LogInformation("Soundcloud token expired or missing, refreshing...");
                var staleToken = await TokenCacheHelper.GetCachedTokenAsync<SoundcloudAccessTokenDto>(
                    cache,
                    ProviderName);

                var token = await RefreshOrFetchTokenAsync(staleToken);

                await TokenCacheHelper.SetCachedTokenAsync(
                    cache,
                    ProviderName,
                    token,
                    token.ExpiresAt);

                return token;
            }
            catch (Exception ex)
            {
                logger.LogWarning(ex, "Error occurred while getting Soundcloud access token");
                throw new Exception($"Error occurred while getting Soundcloud token: {ex.Message}");
            }
            finally
            {
                sem.Release();
            }
        }

        private async Task<SoundcloudAccessTokenDto> RefreshOrFetchTokenAsync(SoundcloudAccessTokenDto? cachedToken)
        {
            if (!string.IsNullOrEmpty(cachedToken?.RefreshToken))
            {
                try
                {
                    logger.LogInformation("Attempting to refresh Soundcloud token");
                    return await RefreshTokenAsync(cachedToken.RefreshToken);
                }
                catch (Exception ex)
                {
                    logger.LogWarning(ex, "Refresh token failed, falling back to client credentials");
                }
            }

            logger.LogInformation("Fetching new Soundcloud token via client credentials");
            return await FetchClientCredentialsTokenAsync();
        }

        private async Task<SoundcloudAccessTokenDto> RefreshTokenAsync(string refreshToken)
        {
            var client = httpClientFactory.CreateClient();
            var clientId = RequiredConfig.Get(config, "Soundcloud:ClientId");
            var clientSecret = RequiredConfig.Get(config, "Soundcloud:ClientSecret");

            var req = new HttpRequestMessage(HttpMethod.Post, TokenUrl)
            {
                Content = new FormUrlEncodedContent(
                [
                    new KeyValuePair<string, string>("grant_type", "refresh_token"),
                    new KeyValuePair<string, string>("client_id", clientId),
                    new KeyValuePair<string, string>("client_secret", clientSecret),
                    new KeyValuePair<string, string>("refresh_token", refreshToken)
                ])
            };

            var resp = await client.SendAsync(req);
            resp.EnsureSuccessStatusCode();

            var newToken = await resp.Content.ReadFromJsonAsync<SoundcloudAccessTokenDto>()
                ?? throw new Exception("Error deserializing Soundcloud refresh token response");

            newToken.ExpiresAt = DateTimeOffset.UtcNow
                .AddSeconds(newToken.ExpiresIn)
                .ToUnixTimeSeconds();

            return newToken;
        }

        private async Task<SoundcloudAccessTokenDto> FetchClientCredentialsTokenAsync()
        {
            var client = httpClientFactory.CreateClient();
            var clientId = RequiredConfig.Get(config, "Soundcloud:ClientId");
            var clientSecret = RequiredConfig.Get(config, "Soundcloud:ClientSecret");

            var req = new HttpRequestMessage(HttpMethod.Post, TokenUrl);
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

            var newToken = await resp.Content.ReadFromJsonAsync<SoundcloudAccessTokenDto>()
                ?? throw new Exception("Error deserializing Soundcloud token JSON");

            newToken.ExpiresAt = DateTimeOffset.UtcNow
                .AddSeconds(newToken.ExpiresIn)
                .ToUnixTimeSeconds();

            return newToken;
        }
    }
}
