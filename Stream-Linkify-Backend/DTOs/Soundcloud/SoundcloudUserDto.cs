using System.Text.Json.Serialization;

namespace Stream_Linkify_Backend.DTOs.Soundcloud
{
    public record SoundcloudUserDto
    {
        [property: JsonPropertyName("id")] public long Id { get; set; }
        [property: JsonPropertyName("username")] public string? Username { get; set; }
        [property: JsonPropertyName("permalink_url")] public string? PermalinkUrl { get; set; }
        [property: JsonPropertyName("avatar_url")] public string? AvatarUrl { get; set; }
    }
}
