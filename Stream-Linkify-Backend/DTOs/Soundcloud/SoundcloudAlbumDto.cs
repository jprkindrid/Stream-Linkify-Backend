using System.Text.Json.Serialization;

namespace Stream_Linkify_Backend.DTOs.Soundcloud
{
    /// <summary>
    /// SoundCloud uses playlists for albums (playlist_type = "album")
    /// </summary>
    public record SoundcloudAlbumDto
    {
        [property: JsonPropertyName("id")] public long Id { get; set; }
        [property: JsonPropertyName("title")] public string? Title { get; set; }
        [property: JsonPropertyName("permalink_url")] public string? PermalinkUrl { get; set; }
        [property: JsonPropertyName("permalink")] public string? Permalink { get; set; }
        [property: JsonPropertyName("description")] public string? Description { get; set; }
        [property: JsonPropertyName("artwork_url")] public string? ArtworkUrl { get; set; }
        [property: JsonPropertyName("genre")] public string? Genre { get; set; }
        [property: JsonPropertyName("playlist_type")] public string? PlaylistType { get; set; }
        [property: JsonPropertyName("track_count")] public int TrackCount { get; set; }
        [property: JsonPropertyName("duration")] public long Duration { get; set; }
        [property: JsonPropertyName("user")] public SoundcloudUserDto? User { get; set; }
        [property: JsonPropertyName("tracks_uri")] public string? TracksUri { get; set; }
        [property: JsonPropertyName("created_at")] public string? CreatedAt { get; set; }
        [property: JsonPropertyName("likes_count")] public long? LikesCount { get; set; }
        [property: JsonPropertyName("release_year")] public int? ReleaseYear { get; set; }
        [property: JsonPropertyName("release_month")] public int? ReleaseMonth { get; set; }
        [property: JsonPropertyName("release_day")] public int? ReleaseDay { get; set; }
    }
}
