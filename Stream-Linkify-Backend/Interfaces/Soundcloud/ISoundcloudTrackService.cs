using Stream_Linkify_Backend.DTOs.Soundcloud;

namespace Stream_Linkify_Backend.Interfaces.Soundcloud
{
    public interface ISoundcloudTrackService
    {
        Task<SoundcloudTrackDto?> GetByUrlAsync(string soundcloudUrl);
        Task<string?> GetByNameAsync(string trackName, string artistName, string isrc);
    }
}
