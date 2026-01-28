using Stream_Linkify_Backend.DTOs.Soundcloud;

namespace Stream_Linkify_Backend.Interfaces.Soundcloud
{
    public interface ISoundcloudAlbumService
    {
        Task<SoundcloudAlbumDto?> GetByUrlAsync(string soundcloudUrl);
        Task<string?> GetByNameAsync(string albumName, string artistName);
    }
}
