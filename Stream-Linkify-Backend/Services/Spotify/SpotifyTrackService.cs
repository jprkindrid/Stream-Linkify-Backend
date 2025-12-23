using Microsoft.Extensions.Logging;
using Newtonsoft.Json.Linq;
using Stream_Linkify_Backend.DTOs.Spotify;
using Stream_Linkify_Backend.Helpers.URLs;
using Stream_Linkify_Backend.Interfaces.Spotify;
using System;

namespace Stream_Linkify_Backend.Services.Spotify
{
    public class SpotifyTrackService : ISpotifyTrackService
    {
        private const string spotifyApiUrl = "https://api.Spotify.com/v1";
        private readonly ISpotifyApiClient spotifyApiClient;
        private readonly ILogger<SpotifyTrackService> logger;

        public SpotifyTrackService(
            ISpotifyApiClient spotifyApiClient,
            ILogger<SpotifyTrackService> logger
            )
        {
            this.spotifyApiClient = spotifyApiClient;
            this.logger = logger;
        }
        public async Task<SpotifyTrackFullDto?> GetByUrlAsync(string spotifyUrl)
        {

            var trackID = SpotifyUrlHelper.ExtractSpotifyId(spotifyUrl, "track");
            var reqUrl = $"{spotifyApiUrl}/tracks/{trackID}";

            var result = await spotifyApiClient.SendSpotifyRequestAsync<SpotifyTrackFullDto>(reqUrl);

            return result;
        }

        public async Task<(string? url, string? albumName, List<string> artistNames, string artworkUrl)> GetByNameAsync(string isrc, string trackName, string artistName)
        {
            var query = $"isrc:{isrc}";
            var reqUrl = $"{spotifyApiUrl}/search?q={Uri.EscapeDataString(query)}&type=track%2Calbum";

            SpotifySearchResponseDto? result = await spotifyApiClient.SendSpotifyRequestAsync<SpotifySearchResponseDto>(reqUrl);

            if (result?.Tracks != null && result.Tracks.Items.Count != 0)
            {
                var trackId = result.Tracks?.Items.FirstOrDefault()?.Id;
                var url = $"https://open.spotify.com/track/{trackId}";

                var albumName = result.Tracks?.Items?.FirstOrDefault()?.Album?.Name;

                var artistNames = result.Tracks?.Items?.FirstOrDefault()?.Artists.Select(a => a.Name).ToList();
                var artworkUrl = result.Tracks?.Items?.FirstOrDefault()?.Album?.Images.OrderByDescending(i => i.Width).FirstOrDefault()?.Url;
                if (artistNames == null)
                    return (url, albumName, [], artworkUrl);

                return (url, albumName, artistNames, artworkUrl);
            }

            logger.LogWarning("Could not get Spotify track for isrc '{isrc}' with title '{trackName}'", isrc, trackName);

            query = $"track:{trackName} artist:{artistName}";
            reqUrl =$"{spotifyApiUrl}/search?q={Uri.EscapeDataString(query)}&type=track%2Calbum&market=US";

            result = await spotifyApiClient.SendSpotifyRequestAsync<SpotifySearchResponseDto>(reqUrl);

            if (result == null || result.Tracks == null)
            {
                logger.LogWarning("Coult not get Spotify track with name {trackName} and first artist name {artistName}", trackName, artistName);
                return (null, null, [], null);
            }

            foreach (var track in result.Tracks.Items)
            {
                if (track.Name == trackName && track.Artists.Select(a => a.Name).Contains(artistName))
                {
                    var trackId = track.Id;
                    var url = $"https://open.spotify.com/track/{track.Id}";

                    var albumName = track.Album.Name;

                    var artistNames = track.Artists.Select(a => a.Name).ToList();
                    var artworkUrl = track.Album.Images.OrderByDescending(i => i.Width).FirstOrDefault()?.Url;

                    if (artistNames == null)
                        return (url, albumName, [], artworkUrl);

                    return (url, albumName, artistNames, artworkUrl);
                }
            }

            return (null, null, [], null);
        }
    }
}
