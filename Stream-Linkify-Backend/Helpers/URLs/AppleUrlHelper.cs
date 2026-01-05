using System;
using System.Web;

namespace Stream_Linkify_Backend.Helpers.URLs
{
    public static class AppleUrlHelper
    {
        public static (string Region, string AlbumId, string? TrackId) ExtractAppleAlbumIdAndRegion(string appleUrl)
        {
            appleUrl = appleUrl.Trim();
            if (!Uri.TryCreate(appleUrl, UriKind.Absolute, out var uri))
                throw new ArgumentException("Invalid URL format");

            var pathParts = uri.AbsolutePath.Split('/', StringSplitOptions.RemoveEmptyEntries);

            if (pathParts.Length < 4 || pathParts[0].Length != 2)
                throw new ArgumentException("Not a valid Apple Music URL");

            var region = pathParts[0]; 
            var albumId = pathParts[^1]; 

            var queryParams = HttpUtility.ParseQueryString(uri.Query);
            var trackId = queryParams["i"]; // null if not present

            return (region, albumId, trackId);
        }

        //TODO: FIX PARSING SO IT CAN HANDLE THE NEW SONG URL VARIANT FOR EXAMPLE https://music.apple.com/us/song/kinetic/1649566880

        // Overload: track-only version
        public static (string Region, string AlbumId, string TrackId) ExtractAppleTrackId(string appleUrl)
        {
            var (region, albumId, trackId) = ExtractAppleAlbumIdAndRegion(appleUrl);

            if (string.IsNullOrEmpty(trackId))
                throw new ArgumentException("Provided URL is not a track link (missing ?i=trackId)");

            return (region, albumId, trackId);
        }
    }
}