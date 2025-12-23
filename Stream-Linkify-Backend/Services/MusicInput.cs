using Stream_Linkify_Backend.DTOs;
using Stream_Linkify_Backend.Interfaces;
using Stream_Linkify_Backend.Mappers;

namespace Stream_Linkify_Backend.Services
{
    public class MusicInput(IEnumerable<IPlatformDataFetcher> fetchers,
        IMusicUrlResolver resolver) : IMusicInput
    {

        private readonly List<IPlatformDataFetcher> _fetchers = [.. fetchers];
        public async Task<AlbumReturnDto> GetAlbumUrlsAsync(string url)
        {
            var uri = new Uri(url);
            var fetcher = _fetchers.FirstOrDefault(f => f.CanHandle(uri.Host))
                ?? throw new NotSupportedException($"Unsupported platform: {uri.Host}");

            var album = await fetcher.FetchAlbumDataAsync(url);
            await resolver.ResolveAlbumUrlsAsync(album, fetcher.Platform);

            return album.ToAlbumReturnDto();
        }

        public async Task<TrackReturnDto> GetTrackUrlsAsync(string url)
        {
            var uri = new Uri(url);
            var fetcher = _fetchers.FirstOrDefault(f => f.CanHandle(uri.Host))
                ?? throw new NotSupportedException($"Unsupported platform: {uri.Host}");

            var track = await fetcher.FetchTrackDataAsync(url); 
            await resolver.ResolveTrackUrlsAsync(track, fetcher.Platform);

            return track.ToTrackReturnDto();
        }
    }
}
