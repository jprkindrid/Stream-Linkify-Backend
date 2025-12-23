using Stream_Linkify_Backend.DTOs;
using Stream_Linkify_Backend.Models;

namespace Stream_Linkify_Backend.Mappers
{
    public static class AlbumMapper
    {
        public static AlbumReturnDto ToAlbumReturnDto(this AlbumModel album)
        {
            return new AlbumReturnDto
            {
                ArtistNames = album.ArtistNames,
                AlbumName = album.AlbumName,
                StreamingServices = album.StreamingServices,
                ArtworkUrl = album.AlbumArtworkUrl
            };
        }
    }
}
