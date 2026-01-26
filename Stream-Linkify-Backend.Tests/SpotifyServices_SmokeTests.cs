using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Stream_Linkify_Backend.DTOs.Spotify;
using Stream_Linkify_Backend.Helpers.URLs;
using Stream_Linkify_Backend.Interfaces.Spotify;
using Stream_Linkify_Backend.Services.Spotify;
using System.Threading.Tasks;
using Xunit;

namespace Stream_Linkify_Backend.Tests
{
    public class SpotifyServices_SmokeTests
    {
        private readonly ServiceProvider _serviceProvider;

        public SpotifyServices_SmokeTests()
        {
            var services = new ServiceCollection();

            // Logging & HTTP
            services.AddLogging(b => b.AddConsole());
            services.AddHttpClient();
            services.AddDistributedMemoryCache();

            // Config from user-secrets + env vars
            var config = new ConfigurationBuilder()
                .AddUserSecrets<SpotifyServices_SmokeTests>(optional: true)
                .AddEnvironmentVariables()
                .Build();

            services.AddSingleton<IConfiguration>(config);

            // Core Spotify services
            services.AddSingleton<ISpotifyApiClient, SpotifyApiClient>();
            services.AddSingleton<ISpotifyTokenService, SpotifyTokenService>();
            services.AddSingleton<ISpotifyTrackService, SpotifyTrackService>();
            services.AddSingleton<ISpotifyAlbumService, SpotifyAlbumService>();

            _serviceProvider = services.BuildServiceProvider();
        }

        [Fact]
        public async Task GetValidTokenAsync_ReturnsToken()
        {
            var svc = _serviceProvider.GetRequiredService<ISpotifyTokenService>();

            var token = await svc.GetValidTokenAsync();

            Assert.NotNull(token);
            Assert.False(string.IsNullOrWhiteSpace(token!.AccessToken));
        }

        [Fact]
        public async Task GetValidTrackAsync_ReturnsTrack()
        {
            var trackService = _serviceProvider.GetRequiredService<ISpotifyTrackService>();

            var exampleTrack = "https://open.spotify.com/track/43eLl2gwEr0fgbFgS11uOh?si=220d34b2d78b4ca7";

            var track = await trackService.GetByUrlAsync(exampleTrack);

            Assert.NotNull(track);
            Assert.False(string.IsNullOrWhiteSpace(track!.Name));
        }

        [Fact]
        public async Task GetValidAlbumAsync_ReturnsAlbum()
        {
            var albumService = _serviceProvider.GetRequiredService<ISpotifyAlbumService>();

            var exampleAlbum = "https://open.spotify.com/album/27teXombBxDGNa9f5jtOr2?si=R6dKhp2MSc-jKFirPkZzLA";

            var spotifyAlbum = await albumService.GetByUrlAsync(exampleAlbum);

            Assert.NotNull(spotifyAlbum);

            var upc = spotifyAlbum.ExternalIds.Upc;
            Assert.NotNull(upc);

            var artistNames = spotifyAlbum.Artists.Select(x => x.Name).ToList();
            Assert.NotNull(artistNames);
        }

        [Fact]
        public async Task GetAlbumByUpcAsync_ReturnsCorrectAlbum()
        {
            var albumService = _serviceProvider.GetRequiredService<ISpotifyAlbumService>();

            var exampleAlbumUpc = "199257088807"; // Kindrid - Inertia of Solitude
            var exampleAlbumLink = "https://open.spotify.com/album/0lEU44IEBoEONvqDU8hEHc";

            var(albumResponseLink, _, _) = await albumService.GetByNameAsync(exampleAlbumUpc, "Inertia of Solitude", "Kindrid");

            Assert.Equal(exampleAlbumLink, albumResponseLink);
        }
    }
}