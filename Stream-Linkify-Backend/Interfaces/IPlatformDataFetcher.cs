using Stream_Linkify_Backend.Enums;
using Stream_Linkify_Backend.Models;

namespace Stream_Linkify_Backend.Interfaces
{
    public interface IPlatformDataFetcher
    {

        MusicPlatform Platform { get; }
        bool CanHandle(string hostName);
        Task<TrackModel> FetchTrackDataAsync(string url);
        Task<AlbumModel> FetchAlbumDataAsync(string url);
    }
}
