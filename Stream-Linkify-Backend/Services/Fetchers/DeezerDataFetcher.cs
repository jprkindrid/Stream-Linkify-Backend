using Stream_Linkify_Backend.Enums;
using Stream_Linkify_Backend.Interfaces;
using Stream_Linkify_Backend.Models;

namespace Stream_Linkify_Backend.Services.Fetchers
{
    public class DeezerDataFetcher(IMusicServiceFactory musicServices) : IPlatformDataFetcher
    {
        public MusicPlatform Platform => MusicPlatform.Deezer;

        public bool CanHandle(string host) =>
            host.Equals("www.deezer.com", StringComparison.OrdinalIgnoreCase) ||
            host.Equals("deezer.com", StringComparison.OrdinalIgnoreCase) ||
            host.Equals("link.deezer.com", StringComparison.OrdinalIgnoreCase);

        public async Task<TrackModel> FetchTrackDataAsync(string url)
        {
            var track = await musicServices.DeezerTrack.GetByUrlAsync(url)
                ?? throw new InvalidOperationException($"Deezer track not found for {url}");

            return new TrackModel
            {
                ISRC = track.Isrc,
                SongName = track.Title,
                AlbumName = track.Album.Title,
                AritstNames = [.. track.Contributors!.Select(x => x.Name)],
                DeezerUrl = track.Link
            };
        }

        public async Task<AlbumModel> FetchAlbumDataAsync(string url)
        {
            var album = await musicServices.DeezerAlbum.GetByUrlAsync(url)
                ?? throw new InvalidOperationException($"Deezer album not found for {url}");

            return new AlbumModel
            {
                UPC = album.Upc,
                AlbumName = album.Title,
                AritstNames = [.. album.Contributors!.Select(x => x.Name)],
                DeezerUrl = album.Link
            };
        }
    }
}