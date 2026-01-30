using Stream_Linkify_Backend.Helpers;
using Stream_Linkify_Backend.Interfaces;
using Stream_Linkify_Backend.Interfaces.Apple;
using Stream_Linkify_Backend.Interfaces.Deezer;
using Stream_Linkify_Backend.Interfaces.Soundcloud;
using Stream_Linkify_Backend.Interfaces.Spotify;
using Stream_Linkify_Backend.Interfaces.Tidal;
using Stream_Linkify_Backend.Services;
using Stream_Linkify_Backend.Services.Apple;
using Stream_Linkify_Backend.Services.Deezer;
using Stream_Linkify_Backend.Services.Fetchers;
using Stream_Linkify_Backend.Services.Resolvers;
using Stream_Linkify_Backend.Services.Soundcloud;
using Stream_Linkify_Backend.Services.Spotify;
using Stream_Linkify_Backend.Services.Tidal;
using System.Net;

namespace Stream_Linkify_Backend.Extensions
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddSpotifyServices(this IServiceCollection services)
        {
            services.AddSingleton<ISpotifyApiClient, SpotifyApiClient>();
            services.AddSingleton<ISpotifyTokenService, SpotifyTokenService>();
            services.AddScoped<ISpotifyTrackService, SpotifyTrackService>();
            services.AddScoped<ISpotifyAlbumService, SpotifyAlbumService>();

            return services;
        }

        public static IServiceCollection AddAppleServices(this IServiceCollection services)
        {
            services.AddSingleton<IAppleApiClient, AppleApiClient>();
            services.AddSingleton<IAppleTokenService, AppleTokenService>();
            services.AddScoped<IAppleTrackService, AppleTrackService>();
            services.AddScoped<IAppleAlbumService, AppleAlbumService>();

            return services;
        }

        public static IServiceCollection AddTidalServices(this IServiceCollection services)
        {
            services.AddSingleton<ITidalApiClient, TidalApiClient>();
            services.AddSingleton<ITidalTokenService, TidalTokenService>();
            services.AddScoped<ITidalArtistService, TidalArtistService>();
            services.AddScoped<ITidalTrackService, TidalTrackService>();
            services.AddScoped<ITidalAlbumService, TidalAlbumService>();

            return services;
        }
        public static IServiceCollection AddDeezerServices(this IServiceCollection services)
        {
            services.AddSingleton<IDeezerApiClient, DeezerApiClient>();
            services.AddScoped<IDeezerTrackService, DeezerTrackService>();
            services.AddScoped<IDeezerAlbumService, DeezerAlbumService>();

            return services;
        }

        public static IServiceCollection AddSoundcloudServices(this IServiceCollection services)
        {
            services.AddSingleton<ISoundcloudTokenService, SoundcloudTokenService>();
            // We need a custom HttpClient to prevent auto-redirects and have minimal headers
            services
                .AddHttpClient("SoundCloud")
                .ConfigurePrimaryHttpMessageHandler(
                    () =>
                        new SocketsHttpHandler
                        {
                            AllowAutoRedirect = false,
                            AutomaticDecompression = DecompressionMethods.All
                        }
                );

            services.AddSingleton<ISoundcloudApiClient, SoundcloudApiClient>();
            services.AddScoped<ISoundcloudTrackService, SoundcloudTrackService>();
            services.AddScoped<ISoundcloudAlbumService, SoundcloudAlbumService>();
            return services;
        }

        public static IServiceCollection AddFetcherServices(this IServiceCollection services)
        {
            services.AddScoped<IPlatformDataFetcher, SpotifyDataFetcher>();
            services.AddScoped<IPlatformDataFetcher, AppleDataFetcher>();
            services.AddScoped<IPlatformDataFetcher, TidalDataFetcher>();
            services.AddScoped<IPlatformDataFetcher, DeezerDataFetcher>();
            services.AddScoped<IPlatformDataFetcher, SoundcloudDataFetcher>();
            return services;
        }

        public static IServiceCollection AddInputAndResolver(this IServiceCollection services)
        {
            services.AddScoped<IPlatformUrlResolver, SpotifyUrlResolver>();
            services.AddScoped<IPlatformUrlResolver, AppleUrlResolver>();
            services.AddScoped<IPlatformUrlResolver, TidalUrlResolver>();
            services.AddScoped<IPlatformUrlResolver, DeezerUrlResolver>();
            services.AddScoped<IPlatformUrlResolver, SoundcloudUrlResolver>();
            services.AddScoped<IMusicUrlResolver, MusicUrlResolver>();
            services.AddScoped<IMusicInput, MusicInput>();
            return services;
        }
    }
}
