namespace Stream_Linkify_Backend.DTOs.Apple
{
    public record AppleDeveloperTokenCacheDto
    {
        public required string Token { get; init; }
        public required long ExpiresAt { get; init; }
    }
}
