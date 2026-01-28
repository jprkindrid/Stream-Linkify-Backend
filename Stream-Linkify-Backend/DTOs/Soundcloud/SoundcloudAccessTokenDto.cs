using System.Text.Json.Serialization;

namespace Stream_Linkify_Backend.DTOs.Soundcloud
{
    public record SoundcloudAccessTokenDto
    {
        [property: JsonPropertyName("access_token")] public required string AccessToken { get; init; }
        [property: JsonPropertyName("refresh_token")] public string? RefreshToken { get; init; }
        [property: JsonPropertyName("token_type")] public string? TokenType { get; init; }
        [property: JsonPropertyName("expires_in")] public required long ExpiresIn { get; init; }
        [property: JsonPropertyName("scope")] public string? Scope { get; init; }
        public long ExpiresAt { get; init; }
    }
}
