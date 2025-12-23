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

            return new TrackModel
            {
                ISRC = track.Attributes.Isrc,
                AppleMusicUrl = track.Attributes.Url,
                SongName = track.Attributes.Name,
                AritstNames = [track.Attributes.ArtistName],
                AlbumName = track.Attributes.AlbumName
            };
        }

        public async Task<AlbumModel> FetchAlbumDataAsync(string url)
        {
            var album = await musicServices.AppleAlbum.GetByUrlAsync(url)
                ?? throw new InvalidOperationException($"Apple album not found for {url}");

            return new AlbumModel
            {
                UPC = album.Attributes.Upc,
                AppleMusicUrl = album.Attributes.Url,
                AlbumName = album.Attributes.Name,
                AritstNames = [album.Attributes.ArtistName]
            };
        }
    }
}