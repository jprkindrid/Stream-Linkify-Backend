using Newtonsoft.Json.Linq;
using Stream_Linkify_Backend.DTOs.Apple;
using Stream_Linkify_Backend.Helpers.URLs;
using Stream_Linkify_Backend.Interfaces.Apple;
using Stream_Linkify_Backend.Interfaces.Tidal;

namespace Stream_Linkify_Backend.Services.Apple
{
    public class AppleTrackService(IAppleApiClient appleApiClient,
        ILogger<AppleTrackService> logger) : IAppleTrackService
    {
        private const string appleMusicApiUrl = "https://api.music.apple.com/v1";
        private readonly IAppleApiClient appleApiClient = appleApiClient;
        private readonly ILogger<AppleTrackService> logger = logger;

        public async Task<AppleSongDataDto?> GetTrackByUrlAsync(string url)
        {
            var (region, trackId) = AppleUrlHelper.ExtractAppleTrackId(url);
            var reqUrl = $"https://api.music.apple.com/v1/catalog/{region}/songs/{trackId}";

            var result = await appleApiClient.SendAppleRequestAsync<AppleSongResponse>(reqUrl);

            if (result == null || result.Data == null || result.Data.Count == 0)
            {
                logger.LogWarning("Apple API returned no data for {Url}", reqUrl);
                logger.LogDebug("Apple Music Response: {@Result}", result);
                return null;
            }

            return result.Data.FirstOrDefault();

        }
        public async Task<string?> GetTrackUrlByNameAsync(string isrc, string trackName, string artistName)
        {
            var reqUrl = $"{appleMusicApiUrl}/catalog/us/songs?filter[isrc]={isrc}";

            var result = await appleApiClient.SendAppleRequestAsync<AppleSongResponse>(reqUrl);

            if (result != null && result.Data != null && result.Data.Count != 0)
            {
                
                return result?.Data?.FirstOrDefault()?.Attributes.Url;
            }

            logger.LogWarning("Could not get Apple Music track for isrc '{isrc}' with name '{trackName}'", isrc, trackName);

            var query = $"{trackName} {artistName}";

            reqUrl = $"{appleMusicApiUrl}/catalog/us/search?types=songs&term={Uri.EscapeDataString(query)}";
            var searchResult = await appleApiClient.SendAppleRequestAsync<AppleSearchResponseDto>(reqUrl);

            if (searchResult == null || searchResult.Results.Songs == null || searchResult.Results.Songs.Data.Count == 0)
            {
                logger.LogWarning("No Apple Music search result for track {trackName} and artist {artistName}", trackName, artistName);
                return null;
            }

            foreach (var song in searchResult.Results.Songs.Data)
            {
                if (song.Attributes.Name == trackName && song.Attributes.ArtistName == artistName)
                    return song.Attributes.Url;
            }

            logger.LogWarning("No Apple Music search result for track {trackName} and artist {artistName}", trackName, artistName);
            return null;
        }
    }
}
