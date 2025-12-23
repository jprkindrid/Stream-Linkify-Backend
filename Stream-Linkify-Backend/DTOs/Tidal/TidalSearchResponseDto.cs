using System.Text.Json;
using System.Text.Json.Serialization;

public record TidalSearchResponseDto(
    [property: JsonPropertyName("data")] TidalSearchData Data,
    [property: JsonPropertyName("links")] TidalLinks Links,
    [property: JsonPropertyName("included")] List<TidalIncludedItem> Included
);

public record TidalSearchData(
    [property: JsonPropertyName("id")] string Id,
    [property: JsonPropertyName("type")] string Type,
    [property: JsonPropertyName("attributes")] TidalSearchAttributes? SearchAttributes,
    [property: JsonPropertyName("relationships")] TidalSearchRelationships Relationships
);

public record TidalSearchAttributes(
    [property: JsonPropertyName("trackingId")] string TrackingId
);

public record TidalSearchRelationships(
    [property: JsonPropertyName("tracks")] TidalTrackRelationship Tracks
);

public record TidalTrackRelationship(
    [property: JsonPropertyName("data")] List<TidalTrackRef> Data,
    [property: JsonPropertyName("links")] TidalLinks Links
);

public record TidalTrackRef(
    [property: JsonPropertyName("id")] string Id,
    [property: JsonPropertyName("type")] string Type
);

public record TidalIncludedItem(
    [property: JsonPropertyName("id")] string Id,
    [property: JsonPropertyName("type")] string Type,
    [property: JsonPropertyName("attributes")] JsonElement Attributes
)
{
    public T? DeserializeAttributes<T>()
    {
        return Attributes.Deserialize<T>();
    }
};

public record TidalExternalLink(
    [property: JsonPropertyName("href")] string Href,
    [property: JsonPropertyName("meta")] TidalExternalMeta Meta
);

public record TidalExternalMeta(
    [property: JsonPropertyName("type")] string Type
);

public record TidalLinks(
    [property: JsonPropertyName("self")] string Self
);

// Attributes Switch
public record TidalTrackAttributes(
    [property: JsonPropertyName("title")] string Title,
    [property: JsonPropertyName("isrc")] string Isrc,
    [property: JsonPropertyName("duration")] string Duration,
    [property: JsonPropertyName("copyright")] JsonElement Copyright,
    [property: JsonPropertyName("explicit")] bool Explicit,
    [property: JsonPropertyName("popularity")] double Popularity,
    [property: JsonPropertyName("accessType")] string AccessType,
    [property: JsonPropertyName("availability")] List<string> Availability,
    [property: JsonPropertyName("mediaTags")] List<string> MediaTags,
    [property: JsonPropertyName("externalLinks")] List<TidalExternalLink> ExternalLinks
);

public record TidalAlbumAttributes(
    [property: JsonPropertyName("title")] string Title,
    [property: JsonPropertyName("barcodeId")] string BarcodeId,
    [property: JsonPropertyName("numberOfVolumes")] int NumberOfVolumes,
    [property: JsonPropertyName("numberOfItems")] int NumberOfItems,
    [property: JsonPropertyName("duration")] string Duration,
    [property: JsonPropertyName("explicit")] bool Explicit,
    [property: JsonPropertyName("releaseDate")] string ReleaseDate,
    [property: JsonPropertyName("copyright")] JsonElement Copyright,
    [property: JsonPropertyName("popularity")] double Popularity,
    [property: JsonPropertyName("availability")] List<string> Availability,
    [property: JsonPropertyName("mediaTags")] List<string> MediaTags,
    [property: JsonPropertyName("externalLinks")] List<TidalExternalLink> ExternalLinks,
    [property: JsonPropertyName("type")] string Type
);