using Stream_Linkify_Backend.Enums;
using Stream_Linkify_Backend.Interfaces;
using Stream_Linkify_Backend.Models;

namespace Stream_Linkify_Backend.Services.Fetchers
{
    public class SpotifyDataFetcher(IMusicServiceFactory musicServices) : IPlatformDataFetcher
    {
        public MusicPlatform Platform => MusicPlatform.Spotify;

        public bool CanHandle(string host) => 
            host.Equals("open.spotify.com", StringComparison.OrdinalIgnoreCase);
        public async Task<AlbumModel> FetchAlbumDataAsync(string url)
        {
            var track = await musicServices.SpotifyAlbum.GetByUrlAsync(url)
                ?? throw new InvalidOperationException($"Spotify album not found for {url}.");

            var model = new AlbumModel
            {
                AlbumName = track.Name,
                AritstNames = [.. track.Artists.Select(a => a.Name)],
                UPC = track.ExternalIds?.Upc,
                StreamingServices = [],
            };

            model.StreamingServices.Add(MusicPlatform.Spotify, track.ExternalUrls.Spotify);

            return model;
        }

        public async Task<TrackModel> FetchTrackDataAsync(string url)
        {
            var track = await musicServices.SpotifyTrack.GetByUrlAsync(url)
                ?? throw new InvalidOperationException($"Spotify track not found for {url}.");

            var model =  new TrackModel
            {
                ISRC = track.ExternalIds?.Isrc,
                AritstNames = [.. track.Artists.Select(a => a.Name)],
                SongName = track.Name,
                AlbumName = track.Album?.Name,
                StreamingServices = []
            };

            model.StreamingServices.Add(MusicPlatform.Spotify, track.ExternalUrls.Spotify);

            return model;
        }
    }
}
