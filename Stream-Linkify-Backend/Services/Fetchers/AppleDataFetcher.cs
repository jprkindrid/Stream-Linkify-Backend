using Stream_Linkify_Backend.Enums;
using Stream_Linkify_Backend.Interfaces;
using Stream_Linkify_Backend.Models;

namespace Stream_Linkify_Backend.Services.Fetchers
{
    public class AppleDataFetcher(IMusicServiceFactory musicServices) : IPlatformDataFetcher
    {
        public MusicPlatform Platform => MusicPlatform.AppleMusic;

        public bool CanHandle(string host) =>
            host.Equals("music.apple.com", StringComparison.OrdinalIgnoreCase);

        public async Task<TrackModel> FetchTrackDataAsync(string url)
        {
            var track = await musicServices.AppleTrack.GetTrackByUrlAsync(url)
                ?? throw new InvalidOperationException($"Apple track not found for {url}");

            var model = new TrackModel
            {
                ISRC = track.Attributes.Isrc,
                SongName = track.Attributes.Name,
                ArtistNames = [track.Attributes.ArtistName],
                AlbumName = track.Attributes.AlbumName,
                AlbumArtworkUrl = track.Attributes.Artwork?.Url?
                .Replace("{w}", "600")
                .Replace("{h}", "600"),
                StreamingServices = []
            };

            model.StreamingServices.Add(MusicPlatform.AppleMusic, track.Attributes.Url);

            return model;
        }

        public async Task<AlbumModel> FetchAlbumDataAsync(string url)
        {
            var album = await musicServices.AppleAlbum.GetByUrlAsync(url)
                ?? throw new InvalidOperationException($"Apple album not found for {url}");

            var model =  new AlbumModel
            {
                UPC = album.Attributes.Upc,
                AlbumName = album.Attributes.Name,
                ArtistNames = [album.Attributes.ArtistName],
                AlbumArtworkUrl = album.Attributes.Artwork?.Url,
                StreamingServices = []
            };

            model.StreamingServices.Add(MusicPlatform.AppleMusic, album.Attributes.Url);

            return model;
        }
    }
}