using Stream_Linkify_Backend.DTOs;
using Stream_Linkify_Backend.Models;

namespace Stream_Linkify_Backend.Mappers
{
    public static class TrackMapper
    {
        public static TrackReturnDto ToTrackReturnDto(this TrackModel track)
        {
            return new TrackReturnDto
            {
                ArtistNames = track.AritstNames,
                SongName = track.SongName,
                StreamingServices = track.StreamingServices,
                ArtworkUrl = track.AlbumArtworUrl
            };
        }
    }
}
