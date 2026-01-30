using Stream_Linkify_Backend.Enums;
using Stream_Linkify_Backend.Interfaces;
using Stream_Linkify_Backend.Models;

namespace Stream_Linkify_Backend.Services.Resolvers
{
    public class SpotifyUrlResolver(
        IMusicServiceFactory musicServices,
        ILogger<SpotifyUrlResolver> logger
        ) : IPlatformUrlResolver
    {
        public MusicPlatform Platform => MusicPlatform.Spotify;

        public async Task ResolveTrackAsync(TrackModel track)
        {
            var (url, albumName, artistNames, artworkUrl) = await musicServices.SpotifyTrack.GetByNameAsync(
                track.ISRC!,
                track.SongName,
                track.ArtistNames.FirstOrDefault()
                );

            track.StreamingServices[MusicPlatform.Spotify] = url;

            if ((track.AlbumArtworkUrl == null || track.AlbumArtworkUrl == string.Empty) && artworkUrl != null && artworkUrl != string.Empty) {
                track.AlbumArtworkUrl = artworkUrl;
            }

            if (url == null) {
                logger.LogWarning("Could not resolve Spotify URL for track: {TrackName} by {ArtistNames} with ISRC {ISRC}", track.SongName, string.Join(", ", track.ArtistNames), track.ISRC);
            }

            if (albumName != null && track.AlbumName == null) {
                track.AlbumName = albumName;
            }

            if (artistNames != null && (track.ArtistNames.Count == 0 || artistNames.Count > track.ArtistNames.Count)) {
                track.ArtistNames = artistNames;
            }
        }

        public async Task ResolveAlbumAsync(AlbumModel album)
        {
            var (url, artistNames, artworkUrl) = await musicServices.SpotifyAlbum.GetByNameAsync(
                album.UPC!,
                album.AlbumName!,
                album.ArtistNames.FirstOrDefault()
                );

            album.StreamingServices[MusicPlatform.Spotify] = url;

            if ((album.AlbumArtworkUrl == null || album.AlbumArtworkUrl == string.Empty) && artworkUrl != null && artworkUrl != string.Empty) {
                album.AlbumArtworkUrl = artworkUrl;
            }

            if (url == null) {
                logger.LogWarning("Could not resolve Spotify URL for album: {AlbumName} by {ArtistNames} with UPC {UPC}", album.AlbumName, string.Join(", ", album.ArtistNames), album.UPC);
            }

            if (artistNames != null && (album.ArtistNames.Count == 0 || artistNames.Count > album.ArtistNames.Count)) {
                album.ArtistNames = artistNames;
            }
        }
    }
}
