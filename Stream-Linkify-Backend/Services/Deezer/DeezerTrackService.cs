using Stream_Linkify_Backend.DTOs.Deezer.Stream_Linkify_Backend.DTOs.Deezer;
using Stream_Linkify_Backend.Helpers.URLs;
using Stream_Linkify_Backend.Interfaces.Deezer;

namespace Stream_Linkify_Backend.Services.Deezer
{
    public class DeezerTrackService(
        IDeezerApiClient deezerApiClient,
        ILogger<DeezerTrackService> logger
        ) : IDeezerTrackService
    {
        const string deezerApiUrl = "https://api.deezer.com";
        private readonly IDeezerApiClient deezerApiClient = deezerApiClient;
        private readonly ILogger<DeezerTrackService> logger = logger;

        public async Task<DeezerTrackFullDto?> GetByUrlAsync(string deezerUrl)
        {
            var trackId = await DeezerUrlHelper.ExtractDeezerId(deezerUrl);
            var reqUrl = $"{deezerApiUrl}/track/{trackId}";

            DeezerTrackFullDto? result = await deezerApiClient.SendDeezerRequestAsync<DeezerTrackFullDto>(reqUrl);

            if (result == null || string.IsNullOrWhiteSpace(result.Title) || string.IsNullOrWhiteSpace(result.Artist.Name))
            {
                logger.LogWarning("Could not get response for Deezer request {url}", reqUrl);
                return null;
            }

            return result;
        }
        public async Task<string?> GetByNameAsync(string trackName, string artistName)
        {
            var query = $"{artistName} {trackName}";
            var reqUrl = $"{deezerApiUrl}/search/track?q={Uri.EscapeDataString(query)}";

            DeezerTrackSearchResponse? result = await deezerApiClient.SendDeezerRequestAsync<DeezerTrackSearchResponse>(reqUrl);

            if (result == null || result.Data.Count == 0)
            {
                logger.LogWarning("Could not get response for Deezer request {url}", reqUrl);
                return null;
            }

            // Deezer does not include ISRC in results to the 'search' endpoint of their api
            foreach (var track in result.Data)
            {
                var titleMatch = string.Equals(track.Title, trackName, StringComparison.OrdinalIgnoreCase);
                var artistMatch = artistName.Contains(track.Artist.Name, StringComparison.OrdinalIgnoreCase) ||
                                  track.Artist.Name.Contains(artistName, StringComparison.OrdinalIgnoreCase);

                if (titleMatch && artistMatch) 
                {
                    logger.LogWarning("Found Deezer track match for {trackName} by {artistName} at {url}", trackName, artistName, track.Link);
                    return track.Link;
                }
                   
            }

            logger.LogWarning("No result for Deezer track with name {trackName} and artist {artistname}", trackName, artistName);

            return null;
        }
    }
}
