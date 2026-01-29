using System.Text.Json.Serialization;

namespace Stream_Linkify_Backend.DTOs.Soundcloud
{
    public record SoundcloudTrackDto
    {
        [property: JsonPropertyName("id")] public long Id { get; set; }
        [property: JsonPropertyName("title")] public string? Title { get; set; }
        [property: JsonPropertyName("permalink_url")] public string? PermalinkUrl { get; set; }
        [property: JsonPropertyName("permalink")] public string? Permalink { get; set; }
        [property: JsonPropertyName("description")] public string? Description { get; set; }
        [property: JsonPropertyName("duration")] public long Duration { get; set; }
        [property: JsonPropertyName("genre")] public string? Genre { get; set; }
        [property: JsonPropertyName("artwork_url")] public string? ArtworkUrl { get; set; }
        [property: JsonPropertyName("stream_url")] public string? StreamUrl { get; set; }
        [property: JsonPropertyName("access")] public string? Access { get; set; }
        [property: JsonPropertyName("user")] public SoundcloudUserDto? User { get; set; }
        [property: JsonPropertyName("publisher_metadata")] public SoundcloudPublisherMetadataDto? PublisherMetadata { get; set; }
        [property: JsonPropertyName("created_at")] public string? CreatedAt { get; set; }
        [property: JsonPropertyName("release_year")] public int? ReleaseYear { get; set; }
        [property: JsonPropertyName("release_month")] public int? ReleaseMonth { get; set; }
        [property: JsonPropertyName("release_day")] public int? ReleaseDay { get; set; }
        [property: JsonPropertyName("release_date")] public string? ReleaseDate { get; set; }
        [property: JsonPropertyName("playback_count")] public long? PlaybackCount { get; set; }
        [property: JsonPropertyName("favoritings_count")] public long? LikesCount { get; set; }
        [property: JsonPropertyName("isrc")] public string? Isrc { get; set; }

        /// <summary>
        /// Gets ISRC from either root level or publisher_metadata (where SoundCloud typically returns it)
        /// </summary>
        [JsonIgnore]
        public string? EffectiveIsrc => Isrc ?? PublisherMetadata?.Isrc;
    }

}