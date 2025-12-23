using Stream_Linkify_Backend.Enums;
using Stream_Linkify_Backend.Models;

namespace Stream_Linkify_Backend.Interfaces
{
    public interface IMusicUrlResolver
    {
        Task ResolveTrackUrlsAsync(TrackModel track, MusicPlatform sourcePlatform);
        Task ResolveAlbumUrlsAsync(AlbumModel track, MusicPlatform sourcePlatform);
    }
}
