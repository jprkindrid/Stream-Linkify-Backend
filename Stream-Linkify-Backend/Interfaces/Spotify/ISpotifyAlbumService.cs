using Stream_Linkify_Backend.DTOs.Spotify;

namespace Stream_Linkify_Backend.Interfaces.Spotify
{
    public interface ISpotifyAlbumService
    {
        Task<SpotifyAlbumFullDto?> GetByUrlAsync(string url);
        Task<(string? url, List<string>? artistNames, string artworkUrl)> GetByNameAsync(string upc, string albumName, string artistName);
    }
}
