using Microsoft.Extensions.Logging;
using Stream_Linkify_Backend.Interfaces.Apple;
using System.Net.Http;

namespace Stream_Linkify_Backend.Services.Apple
{
    public class AppleApiClient : IAppleApiClient
    {
        private readonly IAppleTokenService appleTokenService;
        private readonly ILogger<AppleApiClient> logger;
        private readonly IHttpClientFactory httpClientFactory;
        private readonly SemaphoreSlim sem = new(10, 10);

        public AppleApiClient(
            IAppleTokenService appleTokenService,
            ILogger<AppleApiClient> logger,
            IHttpClientFactory httpClientFactory)
        {
            this.appleTokenService = appleTokenService;
            this.logger = logger;
            this.httpClientFactory = httpClientFactory;
        }
        public async Task<T?> SendAppleRequestAsync<T>(string reqUrl)
        {
            await sem.WaitAsync();
            try
            {
                var aToken = await appleTokenService.GetValidTokenAsync();
                if (string.IsNullOrEmpty(aToken))
                {
                    logger.LogError("Apple Music token is null or empty");
                    return default;
                }

                var req = new HttpRequestMessage(HttpMethod.Get, reqUrl);
                req.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", aToken);

                var client = httpClientFactory.CreateClient();
                logger.LogInformation("Making an Apple Music API request at '{ReqUrl}'", reqUrl);

                var resp = await client.SendAsync(req);

                if (!resp.IsSuccessStatusCode)
                {
                    var content = await resp.Content.ReadAsStringAsync();
                    logger.LogError("Apple Music API error: {StatusCode} - {Content}", resp.StatusCode, content);
                    return default;
                }

                var result = await resp.Content.ReadFromJsonAsync<T>();
                if (result == null)
                {
                    logger.LogError("Failed to deserialize Apple Music response for {ReqUrl}", reqUrl);
                    return default;
                }

                return result;
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error making Apple Music API request at {ReqUrl}", reqUrl);
                return default;
            }
            finally { sem.Release(); }
        }
    }
}
