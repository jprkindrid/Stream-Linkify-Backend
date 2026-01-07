using System;
using System.Web;

namespace Stream_Linkify_Backend.Helpers.URLs
{
    public static class AppleUrlHelper
    {
        public static (string Region, string? AlbumId, string? TrackId) ExtractAppleAlbumIdAndRegion(string appleUrl)
        {
            appleUrl = appleUrl.Trim();
            if (!Uri.TryCreate(appleUrl, UriKind.Absolute, out var uri))
                throw new ArgumentException("Invalid URL format");

            var pathParts = uri.AbsolutePath.Split('/', StringSplitOptions.RemoveEmptyEntries);

            if (pathParts.Length < 3 || pathParts[0].Length != 2)
                throw new ArgumentException("Not a valid Apple Music URL");

            var region = pathParts[0];
            var type = pathParts[1]; // "album" or "song"

            string? albumId;
            string? trackId;

            if (type == "song")
            {
                // Format: /us/song/song-name/trackId
                trackId = pathParts[^1];
                albumId = null;
            }
            else if (type == "album")
            {
                // Format: /us/album/album-name/albumId?i=trackId
                albumId = pathParts[^1];
                var queryParams = HttpUtility.ParseQueryString(uri.Query);
                trackId = queryParams["i"];
            }
            else
            {
                throw new ArgumentException($"Unsupported Apple Music URL type: {type}");
            }

            return (region, albumId, trackId);
        }

        public static (string Region, string TrackId) ExtractAppleTrackId(string appleUrl)
        {
            var (region, _, trackId) = ExtractAppleAlbumIdAndRegion(appleUrl);

            if (string.IsNullOrEmpty(trackId))
                throw new ArgumentException("Provided URL is not a track link");

            return (region, trackId);
        }
    }
}