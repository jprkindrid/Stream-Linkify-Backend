using Stream_Linkify_Backend.Enums;
using Stream_Linkify_Backend.Models;

namespace Stream_Linkify_Backend.Interfaces
{
    public interface IPlatformUrlResolver
    {
        MusicPlatform Platform { get; }
        Task ResolveTrackAsync(TrackModel track);
        Task ResolveAlbumAsync(AlbumModel album);
    }
}
