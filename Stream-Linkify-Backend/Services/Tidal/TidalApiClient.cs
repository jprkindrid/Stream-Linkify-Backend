using Stream_Linkify_Backend.Interfaces.Tidal;

namespace Stream_Linkify_Backend.Services.Tidal
{
    public class TidalApiClient : ITidalApiClient
    {
        private readonly IHttpClientFactory httpClientFactory;
        private readonly ILogger<TidalApiClient> logger;
        private readonly ITidalTokenService tidalTokenService;
        private readonly SemaphoreSlim sem = new(10, 10);

        public TidalApiClient(
            IHttpClientFactory httpClientFactory,
            ILogger<TidalApiClient> logger,
            ITidalTokenService tidalTokenService
            )
        {
            this.httpClientFactory = httpClientFactory;
            this.logger = logger;
            this.tidalTokenService = tidalTokenService;
        }
        public async Task<T?> SendTidalRequestAsync<T>(string reqUrl)
        {
            await sem.WaitAsync();
            try
            {
                var aToken = await tidalTokenService.GetValidTokenAsync()
                            ?? throw new InvalidOperationException("Error getting Tidal access token");

                var req = new HttpRequestMessage(HttpMethod.Get, reqUrl);
                req.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue(
                    "Bearer", aToken.AccessToken);

                var client = httpClientFactory.CreateClient();
                string reqMessage = $"Making a Tidal api request at the path '{reqUrl}'";
                logger.LogInformation(reqMessage);

                var resp = await client.SendAsync(req);
                resp.EnsureSuccessStatusCode();

                var result = await resp.Content.ReadFromJsonAsync<T>();

                return result;
            }
            catch (Exception ex)
            {
                string exMessage = $"error getting making Tidal API request at {reqUrl}";
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
