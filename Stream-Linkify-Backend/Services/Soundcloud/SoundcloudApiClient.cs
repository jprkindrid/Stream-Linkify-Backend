using Stream_Linkify_Backend.Interfaces.Soundcloud;
using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;

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


        public async Task<T?> SendSoundcloudRequestAsync<T>(string reqUrl)
        {
            await sem.WaitAsync();
            try
            {
                var aToken = await soundcloudTokenService.GetValidTokenAsync();
                if (string.IsNullOrWhiteSpace(aToken.AccessToken))
                {
                    logger.LogError("Soundcloud access token is null or empty");
                    return default;
                }

                var client = httpClientFactory.CreateClient("SoundCloud");

                var url = reqUrl;

                for (var hop = 0; hop < 10; hop++)
                {
                    using var req = new HttpRequestMessage(HttpMethod.Get, url);

                    // Header quirks:
                    // Accept: "*/*" matches curl example request on soundcloud api reference
                    // and avoids any content negotiation quirks.
                    // Authorization must be re-added on each hop because .NET will NOT
                    // forward it automatically across redirects (security behavior)
                    // unless you use a custom HttpClientHandler (which we don't want to do here).
                    req.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("*/*"));
                    req.Headers.Authorization = new AuthenticationHeaderValue(
                        "OAuth",
                        aToken.AccessToken
                    );

                    logger.LogInformation("SoundCloud request hop {Hop}: {Url}", hop, url);

                    using var resp = await client.SendAsync(
                        req,
                        HttpCompletionOption.ResponseHeadersRead
                    );

                    if (IsRedirect(resp.StatusCode) && resp.Headers.Location is not null)
                    {
                        var redirectUri = resp.Headers.Location.IsAbsoluteUri
                            ? resp.Headers.Location
                            : new Uri(new Uri(url), resp.Headers.Location);

                        url = redirectUri.ToString();
                        continue;
                    }

                    resp.EnsureSuccessStatusCode();
                    return await resp.Content.ReadFromJsonAsync<T>();
                }

                logger.LogError("Too many redirects for {Url}", reqUrl);
                return default;
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error making Soundcloud API request at {Url}", reqUrl);
                return default;
            }
            finally
            {
                sem.Release();
            }
        }
        private static bool IsRedirect(HttpStatusCode statusCode)
        {
            return statusCode is HttpStatusCode.MovedPermanently
                or HttpStatusCode.Redirect
                or HttpStatusCode.RedirectMethod
                or HttpStatusCode.RedirectKeepVerb
                or HttpStatusCode.TemporaryRedirect
                or HttpStatusCode.PermanentRedirect;
        }
    }
}
