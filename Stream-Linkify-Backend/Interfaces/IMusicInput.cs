using Stream_Linkify_Backend.DTOs;

namespace Stream_Linkify_Backend.Interfaces
{
    public interface IMusicInput
    {
        Task<TrackReturnDto> GetTrackUrlsAsync(string musicUrl);
        Task<AlbumReturnDto> GetAlbumUrlsAsync(string musicUrl);
    }
}
