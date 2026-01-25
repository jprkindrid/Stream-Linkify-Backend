using System.Text.Json.Serialization;

namespace Stream_Linkify_Backend.DTOs.Soundcloud
{
    public record SoundcloudPublisherMetadataDto
    {
        [property: JsonPropertyName("isrc")] public string? Isrc { get; set; }
        [property: JsonPropertyName("upc_or_ean")] public string? Upc { get; set; }
        [property: JsonPropertyName("artist")] public string? Artist { get; set; }
        [property: JsonPropertyName("album_title")] public string? AlbumTitle { get; set; }
        [property: JsonPropertyName("p_line")] public string? PLine { get; set; }
        [property: JsonPropertyName("c_line")] public string? CLine { get; set; }
        [property: JsonPropertyName("release_title")] public string? ReleaseTitle { get; set; }
    }
}
