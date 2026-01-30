using Stream_Linkify_Backend.Enums;
using Stream_Linkify_Backend.Interfaces;
using Stream_Linkify_Backend.Models;

namespace Stream_Linkify_Backend.Services
{
    public class MusicUrlResolver(
        ILogger<MusicUrlResolver> logger,
        IMusicServiceFactory musicServices,
        IEnumerable<IPlatformUrlResolver> platformResolvers
        ) : IMusicUrlResolver
    {
        private readonly IReadOnlyDictionary<MusicPlatform, IPlatformUrlResolver> platformResolversByPlatform =
            platformResolvers.ToDictionary(resolver => resolver.Platform);

        public async Task ResolveTrackUrlsAsync(TrackModel track, MusicPlatform sourcePlatform)
        {
            await ResolveTrackUrlsInternalAsync(track, sourcePlatform, allowRefetch: true, onlyMissing: false);
        }

        public async Task ResolveAlbumUrlsAsync(AlbumModel album, MusicPlatform sourcePlatform)
        {
            await ResolveAlbumUrlsInternalAsync(album, sourcePlatform, allowRefetch: true, onlyMissing: false);
        }

        private async Task ResolveTrackUrlsInternalAsync(TrackModel track, MusicPlatform sourcePlatform, bool allowRefetch, bool onlyMissing)
        {
            logger.LogInformation("Resolving URLs for track, source: {Source}", sourcePlatform);
            var tasks = platformResolversByPlatform
                .Where(resolver => resolver.Key != sourcePlatform)
                .Where(resolver => ShouldResolvePlatform(track.StreamingServices, resolver.Key, onlyMissing))
                .Select(resolver => resolver.Value.ResolveTrackAsync(track));

            await Task.WhenAll(tasks);

            if (!allowRefetch) {
                return;
            }

            if (!HasMissingUrls(track.StreamingServices, sourcePlatform)) {
                return;
            }

            if (string.IsNullOrWhiteSpace(track.ISRC)) {
                await TryHydrateTrackIsrcAsync(track);
            }

            await ResolveTrackUrlsInternalAsync(track, sourcePlatform, allowRefetch: false, onlyMissing: true);
        }

        private async Task ResolveAlbumUrlsInternalAsync(AlbumModel album, MusicPlatform sourcePlatform, bool allowRefetch, bool onlyMissing)
        {
            logger.LogInformation("Resolving URLs for album, source: {Source}", sourcePlatform);
            var tasks = platformResolversByPlatform
                .Where(resolver => resolver.Key != sourcePlatform)
                .Where(resolver => ShouldResolvePlatform(album.StreamingServices, resolver.Key, onlyMissing))
                .Select(resolver => resolver.Value.ResolveAlbumAsync(album));

            await Task.WhenAll(tasks);

            if (!allowRefetch) {
                return;
            }

            if (!HasMissingUrls(album.StreamingServices, sourcePlatform)) {
                return;
            }

            if (string.IsNullOrWhiteSpace(album.UPC)) {
                await TryHydrateAlbumUpcAsync(album);
            }

            await ResolveAlbumUrlsInternalAsync(album, sourcePlatform, allowRefetch: false, onlyMissing: true);
        }

        private static bool ShouldResolvePlatform(Dictionary<MusicPlatform, string> services, MusicPlatform platform, bool onlyMissing)
        {
            return !onlyMissing || IsMissingUrl(services, platform);
        }

        private static bool HasMissingUrls(Dictionary<MusicPlatform, string> services, MusicPlatform sourcePlatform)
        {
            foreach (var platform in Enum.GetValues<MusicPlatform>())
            {
                if (platform == sourcePlatform) {
                    continue;
                }

                if (IsMissingUrl(services, platform)) {
                    return true;
                }
            }

            return false;
        }

        private static bool IsMissingUrl(Dictionary<MusicPlatform, string> services, MusicPlatform platform)
        {
            return !services.TryGetValue(platform, out var url) || string.IsNullOrWhiteSpace(url);
        }

        private async Task TryHydrateTrackIsrcAsync(TrackModel track)
        {
            if (!string.IsNullOrWhiteSpace(track.ISRC)) {
                return;
            }

            if (track.StreamingServices.TryGetValue(MusicPlatform.Spotify, out var spotifyUrl) && !string.IsNullOrWhiteSpace(spotifyUrl)) {
                var spotifyTrack = await musicServices.SpotifyTrack.GetByUrlAsync(spotifyUrl);
                var isrc = spotifyTrack?.ExternalIds?.Isrc;
                if (!string.IsNullOrWhiteSpace(isrc)) {
                    track.ISRC = isrc;
                    return;
                }
            }

            if (track.StreamingServices.TryGetValue(MusicPlatform.AppleMusic, out var appleUrl) && !string.IsNullOrWhiteSpace(appleUrl)) {
                var appleTrack = await musicServices.AppleTrack.GetTrackByUrlAsync(appleUrl);
                var isrc = appleTrack?.Attributes?.Isrc;
                if (!string.IsNullOrWhiteSpace(isrc)) {
                    track.ISRC = isrc;
                }
            }
        }

        private async Task TryHydrateAlbumUpcAsync(AlbumModel album)
        {
            if (!string.IsNullOrWhiteSpace(album.UPC)) {
                return;
            }

            if (album.StreamingServices.TryGetValue(MusicPlatform.Spotify, out var spotifyUrl) && !string.IsNullOrWhiteSpace(spotifyUrl)) {
                var spotifyAlbum = await musicServices.SpotifyAlbum.GetByUrlAsync(spotifyUrl);
                var upc = spotifyAlbum?.ExternalIds?.Upc;
                if (!string.IsNullOrWhiteSpace(upc)) {
                    album.UPC = upc;
                    return;
                }
            }

            if (album.StreamingServices.TryGetValue(MusicPlatform.AppleMusic, out var appleUrl) && !string.IsNullOrWhiteSpace(appleUrl)) {
                var appleAlbum = await musicServices.AppleAlbum.GetByUrlAsync(appleUrl);
                var upc = appleAlbum?.Attributes?.Upc;
                if (!string.IsNullOrWhiteSpace(upc)) {
                    album.UPC = upc;
                }
            }
        }
    }
}
