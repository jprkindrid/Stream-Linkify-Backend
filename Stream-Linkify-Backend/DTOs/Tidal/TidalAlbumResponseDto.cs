using System.Text.Json.Serialization;
using System.Text.Json;

namespace Stream_Linkify_Backend.DTOs.Tidal
{
    public record TidalAlbumResponseDto(
        [property: JsonPropertyName("data")] TidalAlbumData Data,
        [property: JsonPropertyName("links")] TidalLinks Links,
        [property: JsonPropertyName("included")] List<TidalAlbumIncludedItem> Included
    );

    public record TidalAlbumData(
        [property: JsonPropertyName("id")] string Id,
        [property: JsonPropertyName("type")] string Type,
        [property: JsonPropertyName("attributes")] TidalAlbumAttributes Attributes,
        [property: JsonPropertyName("relationships")] TidalAlbumRelationships Relationships
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
        [property: JsonPropertyName("type")] string AlbumType
    );

    public record TidalExternalLink(
        [property: JsonPropertyName("href")] string Href,
        [property: JsonPropertyName("meta")] TidalExternalMeta Meta
    );

    public record TidalExternalMeta(
        [property: JsonPropertyName("type")] string Type
    );

    public record TidalAlbumRelationships(
        [property: JsonPropertyName("similarAlbums")] TidalLinksContainer SimilarAlbums,
        [property: JsonPropertyName("artists")] TidalRelationshipWithData Artists,
        [property: JsonPropertyName("genres")] TidalLinksContainer Genres,
        [property: JsonPropertyName("owners")] TidalLinksContainer Owners,
        [property: JsonPropertyName("coverArt")] TidalLinksContainer CoverArt,
        [property: JsonPropertyName("items")] TidalLinksContainer Items,
        [property: JsonPropertyName("providers")] TidalLinksContainer Providers
    );

    public record TidalRelationshipWithData(
        [property: JsonPropertyName("data")] List<TidalRelationshipData> Data,
        [property: JsonPropertyName("links")] TidalLinks Links
    );

    public record TidalRelationshipData(
        [property: JsonPropertyName("id")] string Id,
        [property: JsonPropertyName("type")] string Type
    );

    public record TidalLinksContainer(
        [property: JsonPropertyName("links")] TidalLinks Links
    );

    public record TidalLinks(
        [property: JsonPropertyName("self")] string Self
    );

    public record TidalAlbumIncludedItem(
        [property: JsonPropertyName("id")] string Id,
        [property: JsonPropertyName("type")] string Type,
        [property: JsonPropertyName("attributes")] TidalArtistAttributes Attributes,
        [property: JsonPropertyName("relationships")] TidalArtistRelationships Relationships
    );

    public record TidalArtistAttributes(
        [property: JsonPropertyName("name")] string Name,
        [property: JsonPropertyName("popularity")] double Popularity,
        [property: JsonPropertyName("externalLinks")] List<TidalExternalLink> ExternalLinks
    );

    public record TidalArtistRelationships(
        [property: JsonPropertyName("similarArtists")] TidalLinksContainer SimilarArtists,
        [property: JsonPropertyName("albums")] TidalLinksContainer Albums,
        [property: JsonPropertyName("roles")] TidalLinksContainer Roles,
        [property: JsonPropertyName("videos")] TidalLinksContainer Videos,
        [property: JsonPropertyName("owners")] TidalLinksContainer Owners,
        [property: JsonPropertyName("biography")] TidalLinksContainer Biography,
        [property: JsonPropertyName("profileArt")] TidalLinksContainer ProfileArt,
        [property: JsonPropertyName("trackProviders")] TidalLinksContainer TrackProviders,
        [property: JsonPropertyName("tracks")] TidalLinksContainer Tracks,
        [property: JsonPropertyName("radio")] TidalLinksContainer Radio
    );
}
