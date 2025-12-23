using Stream_Linkify_Backend.Enums;

namespace Stream_Linkify_Backend.DTOs
{
    public class TrackReturnDto
    {
        public required List<string> ArtistNames { get; set; }
        public required string SongName { get; set; }
        public required Dictionary<MusicPlatform, string> StreamingServices { get; set; }
        public string? ArtworkUrl { get; set; }
    }
}
