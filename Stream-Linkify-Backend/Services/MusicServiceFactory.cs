using Stream_Linkify_Backend.Interfaces;
using Stream_Linkify_Backend.Interfaces.Apple;
using Stream_Linkify_Backend.Interfaces.Deezer;
using Stream_Linkify_Backend.Interfaces.Spotify;
using Stream_Linkify_Backend.Interfaces.Tidal;
using Stream_Linkify_Backend.Interfaces.Soundcloud;

namespace Stream_Linkify_Backend.Services
{
    public class MusicServiceFactory(
        ISpotifyTrackService spotifyTrack,
        ISpotifyAlbumService spotifyAlbum,
        IAppleTrackService appleTrack,
        IAppleAlbumService appleAlbum,
        ITidalTrackService tidalTrack,
        ITidalAlbumService tidalAlbum,
        IDeezerTrackService deezerTrack,
        IDeezerAlbumService deezerAlbum,
        ISoundcloudTrackService soundcloudTrack,
        ISoundcloudAlbumService soundcloudAlbum
            ) : IMusicServiceFactory
    {
        public ISpotifyTrackService SpotifyTrack { get; } = spotifyTrack;
        public ISpotifyAlbumService SpotifyAlbum { get; } = spotifyAlbum;
        public IAppleTrackService AppleTrack { get; } = appleTrack;
        public IAppleAlbumService AppleAlbum { get; } = appleAlbum;
        public ITidalTrackService TidalTrack { get; } = tidalTrack;
        public ITidalAlbumService TidalAlbum { get; } = tidalAlbum;
        public IDeezerTrackService DeezerTrack { get; } = deezerTrack;
        public IDeezerAlbumService DeezerAlbum { get; } = deezerAlbum;
        public ISoundcloudTrackService SoundcloudTrack { get; } = soundcloudTrack;
        public ISoundcloudAlbumService SoundcloudAlbum { get; } = soundcloudAlbum;
    }
}
