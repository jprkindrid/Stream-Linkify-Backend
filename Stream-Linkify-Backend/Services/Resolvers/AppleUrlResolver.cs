using Stream_Linkify_Backend.Enums;
using Stream_Linkify_Backend.Interfaces;
using Stream_Linkify_Backend.Models;

namespace Stream_Linkify_Backend.Services.Resolvers
{
    public class AppleUrlResolver(
        IMusicServiceFactory musicServices,
        ILogger<AppleUrlResolver> logger
        ) : IPlatformUrlResolver
    {
        public MusicPlatform Platform => MusicPlatform.AppleMusic;

        public async Task ResolveTrackAsync(TrackModel track)
        {
            var trackUrl = await musicServices.AppleTrack.GetTrackUrlByNameAsync(
                track.ISRC!,
                track.SongName,
                track.ArtistNames.FirstOrDefault()
                );

            track.StreamingServices[MusicPlatform.AppleMusic] = trackUrl;

            if (trackUrl == null) {
                logger.LogWarning("Could not resolve Apple Music URL for track: {TrackName} by {ArtistNames} with ISRC {ISRC}", track.SongName, string.Join(", ", track.ArtistNames), track.ISRC);
            }
        }

        public async Task ResolveAlbumAsync(AlbumModel album)
        {
            var albumUrl = await musicServices.AppleAlbum.GetUrlByNameAsync(
                album.UPC!,
                album.AlbumName!,
                album.ArtistNames.FirstOrDefault()
                );

            album.StreamingServices[MusicPlatform.AppleMusic] = albumUrl;

            if (albumUrl == null) {
                logger.LogWarning("Could not resolve Apple Music URL for album: {AlbumName} by {ArtistNames} with UPC {UPC}", album.AlbumName, string.Join(", ", album.ArtistNames), album.UPC);
            }
        }
    }
}
