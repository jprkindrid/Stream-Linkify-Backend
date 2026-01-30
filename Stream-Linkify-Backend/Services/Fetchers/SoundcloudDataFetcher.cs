using Stream_Linkify_Backend.Enums;
using Stream_Linkify_Backend.Interfaces;
using Stream_Linkify_Backend.Models;

namespace Stream_Linkify_Backend.Services.Fetchers
{
    public class SoundcloudDataFetcher(IMusicServiceFactory musicServices) : IPlatformDataFetcher
    {
        public MusicPlatform Platform =>MusicPlatform.Soundcloud;

        public bool CanHandle(string host) =>
            host.Equals("soundcloud.com", StringComparison.OrdinalIgnoreCase) || 
            host.Equals("www.soundcloud.com", StringComparison.OrdinalIgnoreCase) ||
            host.Equals("on.soundcloud.com", StringComparison.OrdinalIgnoreCase);

        public async Task<AlbumModel> FetchAlbumDataAsync(string url)
        {
            var album = await musicServices.SoundcloudAlbum.GetByUrlAsync(url)
                ?? throw new InvalidOperationException($"Soundcloud Album not found for URL: {url}");

            var model = new AlbumModel
            {
                AlbumName = album.Title,
                ArtistNames = [album.User!.Username!],
                UPC = null,  // There is no way to get UPC from Soundcloud effectively
                AlbumArtworkUrl = album.ArtworkUrl,
                StreamingServices = [],
            };

            model.StreamingServices.Add(MusicPlatform.Soundcloud, album.PermalinkUrl!);

            return model;
        }

        public async Task<TrackModel> FetchTrackDataAsync(string url)
        {
            var track = await musicServices.SoundcloudTrack.GetByUrlAsync(url)
                ?? throw new InvalidOperationException($"Soundcloud Track not found for URL: {url}");

            var model = new TrackModel
            {
                SongName = track.Title!,
                ArtistNames = [track.User!.Username!],
                AlbumName = null, // Soundcloud does not have albums in the traditional sense
                ISRC = track.EffectiveIsrc,
                AlbumArtworkUrl = track.ArtworkUrl,
                StreamingServices = [],
            };

            model.StreamingServices.Add(MusicPlatform.Soundcloud, track.PermalinkUrl!);

            return model;
        }
    }
}
