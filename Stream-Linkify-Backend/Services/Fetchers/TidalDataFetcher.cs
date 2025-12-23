using Stream_Linkify_Backend.Enums;
using Stream_Linkify_Backend.Interfaces;
using Stream_Linkify_Backend.Interfaces.Tidal;
using Stream_Linkify_Backend.Models;

namespace Stream_Linkify_Backend.Services.Fetchers
{
    public class TidalDataFetcher(
        IMusicServiceFactory musicServices,
        ITidalArtistService tidalArtistService
    ) : IPlatformDataFetcher
    {
        public MusicPlatform Platform => MusicPlatform.Tidal;

        public bool CanHandle(string host) =>
            host.Equals("listen.tidal.com", StringComparison.OrdinalIgnoreCase) ||
            host.Equals("tidal.com", StringComparison.OrdinalIgnoreCase);

        public async Task<TrackModel> FetchTrackDataAsync(string url)
        {
            var track = await musicServices.TidalTrack.GetTrackByUrlAsync(url)
                ?? throw new InvalidOperationException($"Tidal track not found for {url}");

            var tidalUrl = track.Data.Attributes.ExternalLinks
                .Select(l => l.Href).FirstOrDefault() ?? url;

            var artists = await tidalArtistService.GetArtistNamesAsync(url, "track");

            return new TrackModel
            {
                ISRC = track.Data.Attributes.Isrc,
                SongName = track.Data.Attributes.Title,
                AritstNames = artists ?? [],
                TidalUrl = tidalUrl
            };
        }

        public async Task<AlbumModel> FetchAlbumDataAsync(string url)
        {
            var album = await musicServices.TidalAlbum.GetByUrlAsync(url)
                ?? throw new InvalidOperationException($"Tidal album not found for {url}");

            var tidalUrl = album.Data.Attributes.ExternalLinks
                .Select(l => l.Href).FirstOrDefault() ?? url;

            var artists = await tidalArtistService.GetArtistNamesAsync(url, "album");

            return new AlbumModel
            {
                UPC = album.Data.Attributes.BarcodeId,
                AlbumName = album.Data.Attributes.Title,
                AritstNames = artists ?? [],
                TidalUrl = tidalUrl
            };
        }
    }
}