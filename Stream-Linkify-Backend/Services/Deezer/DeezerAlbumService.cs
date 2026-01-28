using Stream_Linkify_Backend.DTOs.Deezer.Stream_Linkify_Backend.DTOs.Deezer;
using Stream_Linkify_Backend.Helpers.URLs;
using Stream_Linkify_Backend.Interfaces.Deezer;

namespace Stream_Linkify_Backend.Services.Deezer
{
    public class DeezerAlbumService(
        IDeezerApiClient deezerApiClient,
        ILogger<DeezerAlbumService> logger
        ) : IDeezerAlbumService
    {
        const string deezerApiUrl = "https://api.deezer.com";
        private readonly IDeezerApiClient deezerApiClient = deezerApiClient;
        private readonly ILogger<DeezerAlbumService> logger = logger;
        public async Task<DeezerAlbumFullDto?> GetByUrlAsync(string deezerUrl)
        {
            var albumId = await DeezerUrlHelper.ExtractDeezerId(deezerUrl);
            var reqUrl = $"{deezerApiUrl}/album/{albumId}";

            DeezerAlbumFullDto? result = await deezerApiClient.SendDeezerRequestAsync<DeezerAlbumFullDto>(reqUrl);

            if (result == null || string.IsNullOrWhiteSpace(result.Title) || string.IsNullOrWhiteSpace(result.Artist.Name))
            {
                logger.LogWarning("Could not get response for Deezer request {url}", reqUrl);
                return null;
            }

            return result;

        }
        public async Task<string?> GetByNameAsync(string albumName, string artistName)
        {
            var query = $"{artistName} {albumName}";
            var reqUrl = $"{deezerApiUrl}/search/album?q={Uri.EscapeDataString(query)}";

            DeezerAlbumSearchResponse? result = await deezerApiClient.SendDeezerRequestAsync<DeezerAlbumSearchResponse>(reqUrl);

            if (result == null || result.Data.Count == 0)
            {
                logger.LogWarning("Could not get response for Deezer request {url}", reqUrl);
                return null;
            }

            // Deezer does not include UPC in results to the 'search' endpoint of their api
            foreach (var album in result.Data)
            {
                var titleMatch = album.Title.Contains(albumName, StringComparison.OrdinalIgnoreCase) ||
                                 albumName.Contains(album.Title, StringComparison.OrdinalIgnoreCase);
                var artistMatch = artistName.Contains(album.Artist!.Name, StringComparison.OrdinalIgnoreCase) ||
                                  album.Artist.Name.Contains(artistName, StringComparison.OrdinalIgnoreCase);

                if (titleMatch && artistMatch)
                    return album.Link;
            }

            logger.LogWarning("No result for Deezer album with name {albumName} and artist {artistname}", albumName, artistName);

            return null;
        }
    }
}
