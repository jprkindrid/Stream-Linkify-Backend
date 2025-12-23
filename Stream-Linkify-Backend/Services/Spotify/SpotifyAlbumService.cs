using Stream_Linkify_Backend.DTOs.Spotify;
using Stream_Linkify_Backend.Helpers.URLs;
using Stream_Linkify_Backend.Interfaces.Spotify;

namespace Stream_Linkify_Backend.Services.Spotify
{
    public class SpotifyAlbumService : ISpotifyAlbumService
    {
        private const string spotifyApiUrl = "https://api.spotify.com/v1";
        private readonly ISpotifyApiClient spotifyApiClient;
        private readonly ILogger<SpotifyAlbumService> logger;

        public SpotifyAlbumService(
           ISpotifyApiClient spotifyApiClient,
            ILogger<SpotifyAlbumService> logger
            )
        {
            this.spotifyApiClient = spotifyApiClient;
            this.logger = logger;
        }
        public async Task<SpotifyAlbumFullDto?> GetByUrlAsync(string spotifyUrl)
        {
            var albumID = SpotifyUrlHelper.ExtractSpotifyId(spotifyUrl, "album");
            var reqUrl = $"{spotifyApiUrl}/albums/{albumID}";

            SpotifyAlbumFullDto? result = await spotifyApiClient.SendSpotifyRequestAsync<SpotifyAlbumFullDto>(reqUrl);
            if (result == null)
            {
                logger.LogWarning("Could not get Spotify album by url");
                return null;
            }

            return result;
        }

        public async Task<(string? url, List<string>? artistNames, string artworkUrl)> GetByNameAsync(string upc, string albumName, string artistName)
        {
            var query = $"upc:{upc}";
            var reqUrl = $"{spotifyApiUrl}/search/?q={Uri.EscapeDataString(query)}&type=album";

            SpotifySearchResponseDto? result = await spotifyApiClient.SendSpotifyRequestAsync<SpotifySearchResponseDto>(reqUrl);

            if (result != null && result.Albums?.Items.Count != 0)
            {
                var url = result?.Albums?.Items?.FirstOrDefault()?.ExternalUrls.Spotify;
                var artistNames = result!.Albums?.Items?.FirstOrDefault()?.Artists.Select(a => a.Name).ToList();
                var artworkUrl = result!.Albums?.Items?.FirstOrDefault()?.Images?.FirstOrDefault()?.Url ?? string.Empty;
                if (artistNames == null)
                    return (url, [], artworkUrl);

                return (url, artistNames, artworkUrl);
            }

            logger.LogWarning("Could not find Spotify Album with UPC: {upc}", upc);

            query = $"album:{albumName} artist:{artistName}";
            reqUrl = $"{spotifyApiUrl}/search?q={Uri.EscapeDataString(query)}&type=album&market=US";

            result = await spotifyApiClient.SendSpotifyRequestAsync<SpotifySearchResponseDto>(reqUrl);

            if (result == null || result.Albums == null || result.Albums?.Items.Count == 0)
            {
                logger.LogWarning("No Spotify result for search with album '{albumName}' and artist '{artistName}'", albumName, artistName);
                return (null, [], null);
            }

            foreach (var album in result.Albums!.Items)
            {
                var artworkUrl = album.Images?.FirstOrDefault()?.Url ?? string.Empty;
                if (album.Name.Contains(albumName, StringComparison.OrdinalIgnoreCase) &&
                    album.Artists.Any(a => string.Equals(a.Name, artistName, StringComparison.OrdinalIgnoreCase)))
                {
                    return (album.ExternalUrls.Spotify, album.Artists.Select(a => a.Name).ToList(), artworkUrl);
                }
            }

            logger.LogWarning("No Spotify result for search with album '{albumName}' and artist '{artistName}'", albumName, artistName);

            return (null, [], null);

        }

    }
}
