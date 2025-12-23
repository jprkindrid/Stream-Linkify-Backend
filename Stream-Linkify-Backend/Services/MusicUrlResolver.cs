using Stream_Linkify_Backend.Enums;
using Stream_Linkify_Backend.Interfaces;
using Stream_Linkify_Backend.Models;

namespace Stream_Linkify_Backend.Services
{
    public class MusicUrlResolver(
        ILogger<MusicUrlResolver> logger,
        IMusicServiceFactory musicServices
        ) : IMusicUrlResolver
    {
        public async Task ResolveAlbumUrlsAsync(AlbumModel track, MusicPlatform sourcePlatform)
        {
            var tasks = new List<Task>();

            if (sourcePlatform != MusicPlatform.Spotify)
                tasks.Add(ResolveSpotifyAlbumAsync(track));

            if (sourcePlatform != MusicPlatform.AppleMusic)
                tasks.Add(ResolveAppleAlbumAsync(track));

            if (sourcePlatform != MusicPlatform.Tidal)
                tasks.Add(ResolveTidalAlbumAsync(track));

            if (sourcePlatform != MusicPlatform.Deezer)
                tasks.Add(ResolveDeezerAlbumAsync(track));

            await Task.WhenAll(tasks);
        }

        public async Task ResolveTrackUrlsAsync(TrackModel track, MusicPlatform sourcePlatform)
        {
            var tasks = new List<Task>();

            if (sourcePlatform != MusicPlatform.Spotify)
                tasks.Add(ResolveSpotifyTrackAsync(track));

            if (sourcePlatform != MusicPlatform.AppleMusic)
                tasks.Add(ResolveAppleTrackAsync(track));

            if (sourcePlatform != MusicPlatform.Tidal)
                tasks.Add(ResolveTidalTrackAsync(track));

            if (sourcePlatform != MusicPlatform.Deezer)
                tasks.Add(ResolveDeezerTrackAsync(track));
            
            await Task.WhenAll(tasks);
        }


        // Track Resolvers
        private async Task ResolveSpotifyTrackAsync(TrackModel track)
        {
            var (url, albumName, artistNames) = await musicServices.SpotifyTrack.GetByNameAsync(
                track.ISRC!,
                track.SongName,
                track.AritstNames.FirstOrDefault()
                );

            track.SpotifyUrl = url;

            if (url == null) {
                logger.LogWarning("Could not resolve Spotify URL for track: {TrackName} by {ArtistNames} with ISRC {ISRC}", track.SongName, string.Join(", ", track.AritstNames), track.ISRC);
            }

            if (albumName != null && track.AlbumName == null) {
                track.AlbumName = albumName;
            }

            if (artistNames != null && (track.AritstNames.Count == 0 || artistNames.Count > track.AritstNames.Count)) {
                track.AritstNames = artistNames;
            }
        }

        private async Task ResolveAppleTrackAsync(TrackModel track)
        {
            track.AppleMusicUrl = await musicServices.AppleTrack.GetTrackUrlByNameAsync(
                track.ISRC!,
                track.SongName,
                track.AritstNames.FirstOrDefault()
                );

            if (track.AppleMusicUrl == null) {
                logger.LogWarning("Could not resolve Apple Music URL for track: {TrackName} by {ArtistNames} with ISRC {ISRC}", track.SongName, string.Join(", ", track.AritstNames), track.ISRC);
            }
        }

        private async Task ResolveTidalTrackAsync(TrackModel track)
        {
            track.TidalUrl = await musicServices.TidalTrack.GetTrackUrlByNameAsync(
                track.SongName,
                track.AritstNames.FirstOrDefault()!,
                track.ISRC!
                );
            if (track.TidalUrl == null) {
                logger.LogWarning("Could not resolve TIDAL URL for track: {TrackName} by {ArtistNames} with ISRC {ISRC}", track.SongName, string.Join(", ", track.AritstNames), track.ISRC);
            }
        }

        private async Task ResolveDeezerTrackAsync(TrackModel track)
        {
            track.DeezerUrl = await musicServices.DeezerTrack.GetByNameAsync(
                track.SongName,
                track.AritstNames.FirstOrDefault()!
                );
            if (track.DeezerUrl == null) {
                logger.LogWarning("Could not resolve Deezer URL for track: {TrackName} by {ArtistNames} with ISRC {ISRC}", track.SongName, string.Join(", ", track.AritstNames), track.ISRC);
            }
        }

        // Album Resolvers
        private async Task ResolveSpotifyAlbumAsync(AlbumModel album) 
        {
            var (url, artistNames) = await musicServices.SpotifyAlbum.GetByNameAsync(
                album.UPC!,
                album.AlbumName!,
                album.AritstNames.FirstOrDefault()
                );  

            album.SpotifyUrl = url;

            if (url == null) { 
                logger.LogWarning("Could not resolve Spotify URL for album: {AlbumName} by {ArtistNames} with UPC {UPC}", album.AlbumName, string.Join(", ", album.AritstNames));
            }

            if (artistNames != null && (album.AritstNames.Count == 0 || artistNames.Count > album.AritstNames.Count)) {
                album.AritstNames = artistNames;
            }
        }

        private async Task ResolveAppleAlbumAsync(AlbumModel album)
        {
            album.AppleMusicUrl = await musicServices.AppleAlbum.GetUrlByNameAsync(
                album.UPC!,
                album.AlbumName!,
                album.AritstNames.FirstOrDefault()
                );
            if (album.AppleMusicUrl == null) {
                logger.LogWarning("Could not resolve Apple Music URL for album: {AlbumName} by {ArtistNames} with UPC {UPC}", album.AlbumName, string.Join(", ", album.AritstNames), album.UPC);
            }
        }   

        private async Task ResolveTidalAlbumAsync(AlbumModel album)
        {
            album.TidalUrl = await musicServices.TidalAlbum.GetUrlByNameAsync(
                album.AlbumName!,
                album.AritstNames.FirstOrDefault()!,
                album.UPC!
                );
            if (album.TidalUrl == null) {
                logger.LogWarning("Could not resolve TIDAL URL for album: {AlbumName} by {ArtistNames} with UPC {UPC}", album.AlbumName, string.Join(", ", album.AritstNames), album.UPC);
            }
        }

        private async Task ResolveDeezerAlbumAsync(AlbumModel album)
        {
            album.DeezerUrl = await musicServices.DeezerAlbum.GetByNameAsync(
                album.AlbumName!,
                album.AritstNames.FirstOrDefault()!
                );
            if (album.DeezerUrl == null) {
                logger.LogWarning("Could not resolve Deezer URL for album: {AlbumName} by {ArtistNames} ", album.AlbumName, string.Join(", ", album.AritstNames));
            }
        }

    }
}