using Stream_Linkify_Backend.DTOs.Soundcloud;

namespace Stream_Linkify_Backend.Interfaces.Soundcloud
{
    public interface ISoundcloudTokenService
    {
        Task<SoundcloudAccessTokenDto> GetValidTokenAsync();
    }
}
