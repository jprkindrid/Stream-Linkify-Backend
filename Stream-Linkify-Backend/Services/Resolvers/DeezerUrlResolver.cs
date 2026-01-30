using Stream_Linkify_Backend.Enums;
using Stream_Linkify_Backend.Interfaces;
using Stream_Linkify_Backend.Models;

namespace Stream_Linkify_Backend.Services.Resolvers
{
    public class DeezerUrlResolver(
        IMusicServiceFactory musicServices,
        ILogger<DeezerUrlResolver> logger
        ) : IPlatformUrlResolver
    {
        public MusicPlatform Platform => MusicPlatform.Deezer;

        public async Task ResolveTrackAsync(TrackModel track)
        {
            logger.LogInformation("Resolving Deezer Track URL for: {TrackName} by {ArtistNames}", track.SongName, string.Join(", ", track.ArtistNames));
            var trackUrl = await musicServices.DeezerTrack.GetByNameAsync(
                track.SongName,
                track.ArtistNames.FirstOrDefault()!
                );

            track.StreamingServices[MusicPlatform.Deezer] = trackUrl;

            if (trackUrl == null) {
                logger.LogWarning("Could not resolve Deezer URL for track: {TrackName} by {ArtistNames} with ISRC {ISRC}", track.SongName, string.Join(", ", track.ArtistNames), track.ISRC);
            }
        }

        public async Task ResolveAlbumAsync(AlbumModel album)
        {
            var albumUrl = await musicServices.DeezerAlbum.GetByNameAsync(
                album.AlbumName!,
                album.ArtistNames.FirstOrDefault()!
                );

            album.StreamingServices[MusicPlatform.Deezer] = albumUrl;

            if (albumUrl == null) {
                logger.LogWarning("Could not resolve Deezer URL for album: {AlbumName} by {ArtistNames} ", album.AlbumName, string.Join(", ", album.ArtistNames));
            }
        }
    }
}
