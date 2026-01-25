using System.Text.Json.Serialization;

namespace Stream_Linkify_Backend.DTOs.Soundcloud
{
    public record SoundcloudAccessTokenDto
    {
        [property: JsonPropertyName("access_token")] public required string AccessToken { get; set; }
        [property: JsonPropertyName("refresh_token")] public string? RefreshToken { get; set; }
        [property: JsonPropertyName("token_type")] public string? TokenType { get; set; }
        [property: JsonPropertyName("expires_in")] public required long ExpiresIn { get; set; }
        [property: JsonPropertyName("scope")] public string? Scope { get; set; }
        public long ExpiresAt { get; set; }
    }
}
