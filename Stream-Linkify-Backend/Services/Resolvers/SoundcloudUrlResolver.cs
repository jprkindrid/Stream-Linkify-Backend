using Stream_Linkify_Backend.Enums;
using Stream_Linkify_Backend.Interfaces;
using Stream_Linkify_Backend.Models;

namespace Stream_Linkify_Backend.Services.Resolvers
{
    public class SoundcloudUrlResolver(
        IMusicServiceFactory musicServices,
        ILogger<SoundcloudUrlResolver> logger
        ) : IPlatformUrlResolver
    {
        public MusicPlatform Platform => MusicPlatform.Soundcloud;

        public async Task ResolveTrackAsync(TrackModel track)
        {
            var trackUrl = await musicServices.SoundcloudTrack.GetByNameAsync(
                track.SongName,
                track.ArtistNames.FirstOrDefault()!,
                track.ISRC!
                );

            track.StreamingServices[MusicPlatform.Soundcloud] = trackUrl;
            if (trackUrl == null)
            {
                logger.LogWarning("Could not resolve Soundcloud URL for track: {TrackName} by {ArtistNames}", track.SongName, string.Join(", ", track.ArtistNames));
            }
        }

        public async Task ResolveAlbumAsync(AlbumModel album)
        {
            var albumUrl = await musicServices.SoundcloudAlbum.GetByNameAsync(
                album.AlbumName!,
                album.ArtistNames.FirstOrDefault()!
                );

            album.StreamingServices[MusicPlatform.Soundcloud] = albumUrl;
            if (albumUrl == null)
            {
                logger.LogWarning("Could not resolve Soundcloud URL for album: {AlbumName} by {ArtistNames}", album.AlbumName, string.Join(", ", album.ArtistNames));
            }
        }
    }
}
