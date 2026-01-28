using Stream_Linkify_Backend.Interfaces.Soundcloud;

namespace Stream_Linkify_Backend.Services.Soundcloud
{
    public class SoundcloudApiClient(
        IHttpClientFactory httpClientFactory,
        ILogger<SoundcloudApiClient> logger,
        ISoundcloudTokenService soundcloudTokenService) : ISoundcloudApiClient
    {
        private readonly IHttpClientFactory httpClientFactory = httpClientFactory;
        private readonly ILogger<SoundcloudApiClient> logger = logger;
        private readonly ISoundcloudTokenService soundcloudTokenService = soundcloudTokenService;
        private readonly SemaphoreSlim sem = new(10, 10);


        public async Task<T?> SendSoundcloudRequestAsync<T>(string reqUrl, SoundcloudHeaderPrefix prefix)
        {
            await sem.WaitAsync();
            try
            {
                var aToken = await soundcloudTokenService.GetValidTokenAsync();
                if (string.IsNullOrEmpty(aToken.AccessToken))
                {
                    logger.LogError("Soundcloud access token is null or empty");
                    return default;
                }

                var req = new HttpRequestMessage(HttpMethod.Get, reqUrl);
                req.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue(
                    prefix.ToString(), aToken.AccessToken);

                var client = httpClientFactory.CreateClient();
                string reqMessage = $"Making a Soundcloud api request at the path '{reqUrl}'";
                logger.LogInformation(reqMessage);

                var resp = await client.SendAsync(req);
                resp.EnsureSuccessStatusCode();
                var jsonString = await resp.Content.ReadAsStringAsync();
                var result = await resp.Content.ReadFromJsonAsync<T>();

                return result;
            }
            catch (Exception ex)
            {
                string exMessage = $"error getting making Soundcloud API request at {reqUrl}";
                logger.LogError(ex, exMessage);
                return default;
            }
            finally
            {
                sem.Release();
            }
        }
    }
}
