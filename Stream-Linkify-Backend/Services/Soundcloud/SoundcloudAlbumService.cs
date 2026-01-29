using Stream_Linkify_Backend.DTOs.Soundcloud;
using Stream_Linkify_Backend.Interfaces.Soundcloud;

namespace Stream_Linkify_Backend.Services.Soundcloud
{
    public class SoundcloudAlbumService(
        ISoundcloudApiClient soundcloudApiClient,
        ILogger<SoundcloudAlbumService> logger
        ) : ISoundcloudAlbumService
    {

        const string soundcloudApiUrl = "https://api.soundcloud.com";
        private readonly ISoundcloudApiClient soundcloudApiClient = soundcloudApiClient;
        private readonly ILogger<SoundcloudAlbumService> logger = logger;
        public async Task<SoundcloudAlbumDto?> GetByUrlAsync(string soundcloudUrl)
        {

            var reqUrl = $"{soundcloudApiUrl}/resolve?url={Uri.EscapeDataString(soundcloudUrl)}";

            SoundcloudAlbumDto? result = await soundcloudApiClient.SendSoundcloudRequestAsync<SoundcloudAlbumDto>(reqUrl);

            if (result == null || string.IsNullOrWhiteSpace(result.Title) || string.IsNullOrWhiteSpace(result.User.Username))
            {
                logger.LogWarning("Could not get response for Soundcloud request {url}", reqUrl);
                return null;
            }

            return result;
        }

        public async Task<string?> GetByNameAsync(string albumName, string artistName)
        {
            var query = $"{artistName} {albumName}";
            // Soundcloud doesn't have a dedicated album search endpoint, so we search for playlists
            var reqUrl = $"{soundcloudApiUrl}/playlists?q={Uri.EscapeDataString(query)}&show_tracks=true&limit=10&offset=0&linked_partitioning=true";

            SoundcloudSearchResponseDto<SoundcloudAlbumDto>? result =
                await soundcloudApiClient.SendSoundcloudRequestAsync<SoundcloudSearchResponseDto<SoundcloudAlbumDto>>(reqUrl);

            if (result == null || result.Collection == null || result.Collection.Count == 0)
            {
                logger.LogWarning("Could not get response for Soundcloud search request {url}", reqUrl);
                return null;
            }

            // Soundclouds track endpoint takes isrc but their playlist (basically album) endpoint does not take UPC
            // Why? I have no idea.
            foreach (var album in result.Collection)
            {
                if (string.IsNullOrWhiteSpace(album.Title) || string.IsNullOrWhiteSpace(album.User?.Username))
                    continue;

                var titleMatch = album.Title.Contains(albumName, StringComparison.OrdinalIgnoreCase) ||
                    albumName.Contains(album.Title, StringComparison.OrdinalIgnoreCase);

                var artistMatch = album.User.Username.Contains(artistName, StringComparison.OrdinalIgnoreCase) ||
                    artistName.Contains(album.User.Username, StringComparison.OrdinalIgnoreCase);

                if (titleMatch && artistMatch)
                    return album.Permalink;
            }

            logger.LogWarning("No result for Soundcloud album with name {albumName} and artist {artistName}", albumName, artistName);
        
            return null;
        }
    }
}
