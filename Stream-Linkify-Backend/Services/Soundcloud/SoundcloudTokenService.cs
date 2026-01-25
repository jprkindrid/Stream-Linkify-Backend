using Stream_Linkify_Backend.DTOs.Soundcloud;
using Stream_Linkify_Backend.Helpers;
using Stream_Linkify_Backend.Interfaces.Soundcloud;
using System.Text;

namespace Stream_Linkify_Backend.Services.Soundcloud
{
    public class SoundcloudTokenService(
        IConfiguration config,
        ILogger<SoundcloudTokenService> logger,
        IHttpClientFactory httpClientFactory
        ) : ISoundcloudTokenService
    {
        private SoundcloudAccessTokenDto? token;
        private readonly IConfiguration config = config;
        private readonly ILogger<SoundcloudTokenService> logger = logger;
        private readonly IHttpClientFactory httpClientFactory = httpClientFactory;
        private readonly SemaphoreSlim sem = new(1, 1);

        private const string TokenUrl = "https://secure.soundcloud.com/oauth/token";

        public async Task<SoundcloudAccessTokenDto> GetValidTokenAsync()
        {
            await sem.WaitAsync();
            try
            {
                if (token == null || !IsValidToken())
                {
                    logger.LogInformation("Soundcloud token expired or missing, refreshing...");
                    token = await RefreshOrFetchTokenAsync();
                }

                return token!;
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

        private async Task<SoundcloudAccessTokenDto> RefreshOrFetchTokenAsync()
        {
            if (!string.IsNullOrEmpty(token?.RefreshToken))
            {
                try
                {
                    logger.LogInformation("Attempting to refresh Soundcloud token");
                    return await RefreshTokenAsync(token.RefreshToken);
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

        private bool IsValidToken()
        {
            if (token?.ExpiresAt == null)
                return false;

            return DateTimeOffset.FromUnixTimeSeconds(token.ExpiresAt) >
                   DateTimeOffset.UtcNow.AddMinutes(5);
        }
    }
}
