using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Stream_Linkify_Backend.DTOs;
using Stream_Linkify_Backend.Interfaces;
using Stream_Linkify_Backend.Interfaces.Apple;
using Stream_Linkify_Backend.Interfaces.Deezer;
using Stream_Linkify_Backend.Interfaces.Spotify;
using Stream_Linkify_Backend.Interfaces.Tidal;
using Stream_Linkify_Backend.Services;
using Stream_Linkify_Backend.Services.Apple;
using Stream_Linkify_Backend.Services.Deezer;
using Stream_Linkify_Backend.Services.Fetchers;
using Stream_Linkify_Backend.Services.Spotify;
using Stream_Linkify_Backend.Services.Tidal;
using Stream_Linkify_Backend.Enums;
using Xunit;

namespace Stream_Linkify_Backend.Tests
{
    public class MusicServices_IntegrationTests
    {
        private readonly ServiceProvider _serviceProvider;

        public MusicServices_IntegrationTests()
        {
            var services = new ServiceCollection();

            var config = new ConfigurationBuilder()
                .AddUserSecrets<MusicServices_IntegrationTests>(optional: true)
                .AddEnvironmentVariables()
                .Build();

            services.AddSingleton<IConfiguration>(config);
            services.AddLogging(b => b.AddConsole());
            services.AddHttpClient();
            services.AddDistributedMemoryCache();

            // Spotify
            services.AddSingleton<ISpotifyApiClient, SpotifyApiClient>();
            services.AddSingleton<ISpotifyTokenService, SpotifyTokenService>();
            services.AddSingleton<ISpotifyTrackService, SpotifyTrackService>();
            services.AddSingleton<ISpotifyAlbumService, SpotifyAlbumService>();

            // Apple
            services.AddSingleton<IAppleApiClient, AppleApiClient>();
            services.AddSingleton<IAppleTokenService, AppleTokenService>();
            services.AddScoped<IAppleTrackService, AppleTrackService>();
            services.AddScoped<IAppleAlbumService, AppleAlbumService>();

            // Tidal
            services.AddSingleton<ITidalTokenService, TidalTokenService>();
            services.AddSingleton<ITidalApiClient, TidalApiClient>();
            services.AddSingleton<ITidalTrackService, TidalTrackService>();
            services.AddSingleton<ITidalAlbumService, TidalAlbumService>();
            services.AddSingleton<ITidalArtistService, TidalArtistService>();

            // Deezer
            services.AddSingleton<IDeezerApiClient, DeezerApiClient>();
            services.AddSingleton<IDeezerTrackService, DeezerTrackService>();
            services.AddSingleton<IDeezerAlbumService, DeezerAlbumService>();

            // MusicServiceFactory
            services.AddScoped<IMusicServiceFactory, MusicServiceFactory>();

            // New refactored services
            services.AddScoped<IMusicUrlResolver, MusicUrlResolver>();
            services.AddScoped<IPlatformDataFetcher, SpotifyDataFetcher>();
            services.AddScoped<IPlatformDataFetcher, AppleDataFetcher>();
            services.AddScoped<IPlatformDataFetcher, TidalDataFetcher>();
            services.AddScoped<IPlatformDataFetcher, DeezerDataFetcher>();
            services.AddScoped<IMusicInput, MusicInput>();

            _serviceProvider = services.BuildServiceProvider();
        }

        private static void WarnMissingPlatforms(
            Dictionary<MusicPlatform, string> services,
            MusicPlatform sourcePlatform)
        {
            foreach (var platform in Enum.GetValues<MusicPlatform>())
            {
                if (platform != sourcePlatform && !services.ContainsKey(platform))
                    Console.WriteLine($"Warning: {platform} URL not resolved");
            }
        }

        #region Track Tests

        [Theory]
        [InlineData("https://open.spotify.com/track/43eLl2gwEr0fgbFgS11uOh", "Telefon Tel Aviv")]
        [InlineData("https://music.apple.com/us/album/wounded/1825854595?i=1825854596", "Mat Zo")]
        [InlineData("https://tidal.com/browse/track/430298612", "Kindrid")]
        [InlineData("https://www.deezer.com/us/track/3135556", "Daft Punk")]
        public async Task GetTrackUrlsAsync_FromAnyPlatform_ReturnsResult(
            string inputUrl,
            string expectedArtist)
        {
            using var scope = _serviceProvider.CreateScope();
            var musicInput = scope.ServiceProvider.GetRequiredService<IMusicInput>();

            var result = await musicInput.GetTrackUrlsAsync(inputUrl);

            Assert.NotNull(result);
            Assert.Contains(expectedArtist, result.ArtistNames);
            Assert.False(string.IsNullOrEmpty(result.SongName));
            Assert.True(result.StreamingServices.Count > 0, "At least one URL should be present");
        }

        [Fact]
        public async Task GetTrackUrlsAsync_FromSpotify_ResolvesOtherPlatforms()
        {
            using var scope = _serviceProvider.CreateScope();
            var musicInput = scope.ServiceProvider.GetRequiredService<IMusicInput>();

            var spotifyUrl = "https://open.spotify.com/track/43eLl2gwEr0fgbFgS11uOh";

            var result = await musicInput.GetTrackUrlsAsync(spotifyUrl);

            Assert.NotNull(result);
            Assert.Equal(spotifyUrl, result.StreamingServices[MusicPlatform.Spotify]);
            Assert.False(string.IsNullOrEmpty(result.SongName));
            Assert.NotEmpty(result.ArtistNames);

            WarnMissingPlatforms(result.StreamingServices, MusicPlatform.Spotify);
        }

        [Fact]
        public async Task GetTrackUrlsAsync_FromApple_ResolvesOtherPlatforms()
        {
            using var scope = _serviceProvider.CreateScope();
            var musicInput = scope.ServiceProvider.GetRequiredService<IMusicInput>();

            var appleUrl = "https://music.apple.com/us/album/wounded/1825854595?i=1825854596";

            var result = await musicInput.GetTrackUrlsAsync(appleUrl);

            Assert.NotNull(result);
            Assert.True(result.StreamingServices.ContainsKey(MusicPlatform.AppleMusic));
            Assert.False(string.IsNullOrEmpty(result.SongName));
            Assert.NotEmpty(result.ArtistNames);

            WarnMissingPlatforms(result.StreamingServices, MusicPlatform.AppleMusic);
        }

        [Fact]
        public async Task GetTrackUrlsAsync_FromTidal_ResolvesOtherPlatforms()
        {
            using var scope = _serviceProvider.CreateScope();
            var musicInput = scope.ServiceProvider.GetRequiredService<IMusicInput>();

            var tidalUrl = "https://tidal.com/browse/track/430298612";

            var result = await musicInput.GetTrackUrlsAsync(tidalUrl);

            Assert.NotNull(result);
            Assert.True(result.StreamingServices.ContainsKey(MusicPlatform.Tidal));
            Assert.False(string.IsNullOrEmpty(result.SongName));
            Assert.NotEmpty(result.ArtistNames);

            WarnMissingPlatforms(result.StreamingServices, MusicPlatform.Tidal);
        }

        [Fact]
        public async Task GetTrackUrlsAsync_FromDeezer_ResolvesOtherPlatforms()
        {
            using var scope = _serviceProvider.CreateScope();
            var musicInput = scope.ServiceProvider.GetRequiredService<IMusicInput>();

            var deezerUrl = "https://www.deezer.com/us/track/3326228661";

            var result = await musicInput.GetTrackUrlsAsync(deezerUrl);

            Assert.NotNull(result);
            Assert.True(result.StreamingServices.ContainsKey(MusicPlatform.Deezer));
            Assert.False(string.IsNullOrEmpty(result.SongName));
            Assert.NotEmpty(result.ArtistNames);

            WarnMissingPlatforms(result.StreamingServices, MusicPlatform.Deezer);
        }

        [Fact]
        public async Task GetTrackUrlsAsync_FromDeezerShareLink_ResolvesCorrectly()
        {
            using var scope = _serviceProvider.CreateScope();
            var musicInput = scope.ServiceProvider.GetRequiredService<IMusicInput>();

            var deezerShareLink = "https://link.deezer.com/s/30OAMLlLdVPqaibF4Rj8B";

            var result = await musicInput.GetTrackUrlsAsync(deezerShareLink);

            Assert.NotNull(result);
            Assert.True(result.StreamingServices.ContainsKey(MusicPlatform.Deezer));
            Assert.Equal("Door City", result.SongName);
            Assert.Contains("Kindrid", result.ArtistNames);
        }

        #endregion

        #region Album Tests

        [Theory]
        [InlineData("https://open.spotify.com/album/0lEU44IEBoEONvqDU8hEHc", "Inertia of Solitude")]
        [InlineData("https://music.apple.com/us/album/inertia-of-solitude/1808747512", "Inertia of Solitude")]
        [InlineData("https://tidal.com/album/430298609", "Inertia of Solitude")]
        [InlineData("https://www.deezer.com/us/album/742962591", "Inertia of Solitude")]
        public async Task GetAlbumUrlsAsync_FromAnyPlatform_ReturnsResult(
            string inputUrl,
            string expectedAlbumName)
        {
            using var scope = _serviceProvider.CreateScope();
            var musicInput = scope.ServiceProvider.GetRequiredService<IMusicInput>();

            var result = await musicInput.GetAlbumUrlsAsync(inputUrl);

            Assert.NotNull(result);
            Assert.Equal(expectedAlbumName, result.AlbumName);
            Assert.NotEmpty(result.ArtistNames);
            Assert.True(result.StreamingServices.Count > 0, "At least one URL should be present");
        }

        [Fact]
        public async Task GetAlbumUrlsAsync_FromSpotify_ResolvesOtherPlatforms()
        {
            using var scope = _serviceProvider.CreateScope();
            var musicInput = scope.ServiceProvider.GetRequiredService<IMusicInput>();

            var spotifyUrl = "https://open.spotify.com/album/0lEU44IEBoEONvqDU8hEHc";

            var result = await musicInput.GetAlbumUrlsAsync(spotifyUrl);

            Assert.NotNull(result);
            Assert.Equal(spotifyUrl, result.StreamingServices[MusicPlatform.Spotify]);
            Assert.Equal("Inertia of Solitude", result.AlbumName);
            Assert.Contains("Kindrid", result.ArtistNames);

            WarnMissingPlatforms(result.StreamingServices, MusicPlatform.Spotify);
        }

        [Fact]
        public async Task GetAlbumUrlsAsync_FromApple_ResolvesOtherPlatforms()
        {
            using var scope = _serviceProvider.CreateScope();
            var musicInput = scope.ServiceProvider.GetRequiredService<IMusicInput>();

            var appleUrl = "https://music.apple.com/us/album/inertia-of-solitude/1808747512";

            var result = await musicInput.GetAlbumUrlsAsync(appleUrl);

            Assert.NotNull(result);
            Assert.True(result.StreamingServices.ContainsKey(MusicPlatform.AppleMusic));
            Assert.Equal("Inertia of Solitude", result.AlbumName);
            Assert.Contains("Kindrid", result.ArtistNames);

            WarnMissingPlatforms(result.StreamingServices, MusicPlatform.AppleMusic);
        }

        [Fact]
        public async Task GetAlbumUrlsAsync_FromTidal_ResolvesOtherPlatforms()
        {
            using var scope = _serviceProvider.CreateScope();
            var musicInput = scope.ServiceProvider.GetRequiredService<IMusicInput>();

            var tidalUrl = "https://tidal.com/album/430298609";

            var result = await musicInput.GetAlbumUrlsAsync(tidalUrl);

            Assert.NotNull(result);
            Assert.True(result.StreamingServices.ContainsKey(MusicPlatform.Tidal));
            Assert.Equal("Inertia of Solitude", result.AlbumName);

            WarnMissingPlatforms(result.StreamingServices, MusicPlatform.Tidal);
        }

        [Fact]
        public async Task GetAlbumUrlsAsync_FromDeezer_ResolvesOtherPlatforms()
        {
            using var scope = _serviceProvider.CreateScope();
            var musicInput = scope.ServiceProvider.GetRequiredService<IMusicInput>();

            var deezerUrl = "https://www.deezer.com/us/album/742962591";

            var result = await musicInput.GetAlbumUrlsAsync(deezerUrl);

            Assert.NotNull(result);
            Assert.True(result.StreamingServices.ContainsKey(MusicPlatform.Deezer));
            Assert.Equal("Inertia of Solitude", result.AlbumName);
            Assert.Contains("Kindrid", result.ArtistNames);

            WarnMissingPlatforms(result.StreamingServices, MusicPlatform.Deezer);
        }

        #endregion
    }
}