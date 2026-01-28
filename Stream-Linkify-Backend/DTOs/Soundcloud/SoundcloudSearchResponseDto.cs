using System.Text.Json.Serialization;

namespace Stream_Linkify_Backend.DTOs.Soundcloud
{
    public record SoundcloudSearchResponseDto<T>
    {
        [property: JsonPropertyName("collection")] public List<T>? Collection { get; set; }
        [property: JsonPropertyName("next_href")] public string? NextHref { get; set; }
        [property: JsonPropertyName("total_results")] public int? TotalResults { get; set; }
    }
    public record SoundcloudTrackSearchResponseDto : SoundcloudSearchResponseDto<SoundcloudTrackDto> { }
    public record SoundcloudAlbumSearchResponseDto : SoundcloudSearchResponseDto<SoundcloudAlbumDto> { }
}
