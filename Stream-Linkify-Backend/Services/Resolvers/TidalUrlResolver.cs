using Stream_Linkify_Backend.Enums;
using Stream_Linkify_Backend.Interfaces;
using Stream_Linkify_Backend.Models;

namespace Stream_Linkify_Backend.Services.Resolvers
{
    public class TidalUrlResolver(
        IMusicServiceFactory musicServices,
        ILogger<TidalUrlResolver> logger
        ) : IPlatformUrlResolver
    {
        public MusicPlatform Platform => MusicPlatform.Tidal;

        public async Task ResolveTrackAsync(TrackModel track)
        {
            var trackUrl = await musicServices.TidalTrack.GetTrackUrlByNameAsync(
                track.SongName,
                track.ArtistNames.FirstOrDefault()!,
                track.ISRC!
                );

            track.StreamingServices[MusicPlatform.Tidal] = trackUrl;

            if (trackUrl == null) {
                logger.LogWarning("Could not resolve TIDAL URL for track: {TrackName} by {ArtistNames} with ISRC {ISRC}", track.SongName, string.Join(", ", track.ArtistNames), track.ISRC);
            }
        }

        public async Task ResolveAlbumAsync(AlbumModel album)
        {
            var albumUrl = await musicServices.TidalAlbum.GetUrlByNameAsync(
                album.AlbumName!,
                album.ArtistNames.FirstOrDefault()!,
                album.UPC!
                );

            album.StreamingServices[MusicPlatform.Tidal] = albumUrl;

            if (albumUrl == null) {
                logger.LogWarning("Could not resolve TIDAL URL for album: {AlbumName} by {ArtistNames} with UPC {UPC}", album.AlbumName, string.Join(", ", album.ArtistNames), album.UPC);
            }
        }
    }
}
