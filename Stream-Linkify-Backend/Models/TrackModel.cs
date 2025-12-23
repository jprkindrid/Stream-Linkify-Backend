using Stream_Linkify_Backend.DTOs;
using Stream_Linkify_Backend.Enums;
using System.Text.Json;

namespace Stream_Linkify_Backend.Models
{
    public class TrackModel
    {
        public string? ISRC { get; set; }
        public required List<string> ArtistNames { get; set; }
        public required string SongName { get; set; }

        public string? AlbumName { get; set; }

        public required Dictionary<MusicPlatform, string> StreamingServices { get; set; } = [];
        public string? AlbumArtworkUrl { get; set; }


    }
}
