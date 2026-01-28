using Stream_Linkify_Backend.DTOs.Soundcloud;
using Stream_Linkify_Backend.Interfaces.Soundcloud;

namespace Stream_Linkify_Backend.Services.Soundcloud
{
    public class SoundcloudTrackService(
        ISoundcloudApiClient soundcloudApiClient,
        ILogger<SoundcloudTrackService> logger
        ) : ISoundcloudTrackService
    {
        const string soundcloudApiUrl = "https://api.soundcloud.com";
        private readonly ISoundcloudApiClient soundcloudApiClient = soundcloudApiClient;
        private readonly ILogger<SoundcloudTrackService> logger = logger;

        public async Task<SoundcloudTrackDto?> GetByUrlAsync(string soundcloudUrl)
        {
            var reqUrl = $"{soundcloudApiUrl}/resolve?url={soundcloudUrl}";

            SoundcloudTrackDto? result = await soundcloudApiClient.SendSoundcloudRequestAsync<SoundcloudTrackDto>(reqUrl, SoundcloudHeaderPrefix.OAuth);

            if (result == null)
            {
                logger.LogWarning("Could not get response for Soundcloud request {SoundcloudUrl}", soundcloudUrl);
                return null;
            }

            return result;
        }

        public async Task<string?> GetByNameAsync(string trackName, string artistName, string isrc)
        {
            var query = $"{artistName} {trackName}";
            var reqUrl = $"{soundcloudApiUrl}/tracks?q={Uri.EscapeDataString(query)}&limit=10&offset=0&linked_partitioning=true";

            SoundcloudSearchResponseDto<SoundcloudTrackDto>? result = await soundcloudApiClient.SendSoundcloudRequestAsync<SoundcloudSearchResponseDto<SoundcloudTrackDto>>(reqUrl, SoundcloudHeaderPrefix.Bearer);

            if (result == null || result.Collection == null || result.Collection.Count == 0)
            {
                logger.LogWarning("Could not get response for Soundcloud search request {reqUrl}", reqUrl);
                return null;
            }

            foreach (var track in result.Collection)
            {
                if (track.Isrc != null && track.Isrc == isrc)
                    return track.PermalinkUrl;

                if (string.IsNullOrWhiteSpace(track.Title) || string.IsNullOrWhiteSpace(track.User?.Username))
                    continue;

                var titleMatch = track.Title.Contains(trackName, StringComparison.OrdinalIgnoreCase) ||
                    trackName.Contains(track.Title, StringComparison.OrdinalIgnoreCase);

                var artistMatch = track.User.Username.Contains(artistName, StringComparison.OrdinalIgnoreCase) ||
                    artistName.Contains(track.User.Username, StringComparison.OrdinalIgnoreCase);

                if (titleMatch && artistMatch)
                    return track.PermalinkUrl;

            }

            logger.LogWarning("No result found for Soundcloud track with name {trackName} and artist {artistName}", trackName, artistName);

            return null;
        }


    }
}
