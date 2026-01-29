using Stream_Linkify_Backend.Services.Soundcloud;

namespace Stream_Linkify_Backend.Interfaces.Soundcloud
{
    public interface ISoundcloudApiClient
    {
        Task<T?> SendSoundcloudRequestAsync<T>(string reqUrl);
    }

}
