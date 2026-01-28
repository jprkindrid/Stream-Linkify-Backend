using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Stream_Linkify_Backend.Interfaces.Soundcloud;
using Stream_Linkify_Backend.Services.Soundcloud;
using Xunit;

namespace Stream_Linkify_Backend.Tests
{
    public class SoundcloudServices_SmokeTests
    {
        private readonly ServiceProvider _serviceProvider;

        public SoundcloudServices_SmokeTests()
        {
            var services = new ServiceCollection();

            var config = new ConfigurationBuilder()
                .AddUserSecrets<SoundcloudServices_SmokeTests>(optional: true)
                .AddEnvironmentVariables()
                .Build();

            services.AddSingleton<IConfiguration>(config);

            // Logging & HTTP
            services.AddLogging(b => b.AddConsole());
            services.AddHttpClient();
            services.AddDistributedMemoryCache();

            // Soundcloud services
            services.AddSingleton<ISoundcloudTokenService, SoundcloudTokenService>();
            services.AddSingleton<ISoundcloudApiClient, SoundcloudApiClient>();
            services.AddSingleton<ISoundcloudTrackService, SoundcloudTrackService>();
            services.AddSingleton<ISoundcloudAlbumService, SoundcloudAlbumService>();

            _serviceProvider = services.BuildServiceProvider();
        }

        [Fact]
        public async Task GetTrackByNameAsync_ReturnsUrl()
        {
            var trackService = _serviceProvider.GetRequiredService<ISoundcloudTrackService>();

            var trackName = "Wounded";
            var artistName = "Mat Zo";
            var isrc = "GBRKQ2482423";

            var trackUrl = await trackService.GetByNameAsync(trackName, artistName, isrc);

            Assert.False(string.IsNullOrWhiteSpace(trackUrl));
        }

        [Fact]
        public async Task GetTrackByUrlAsync_ReturnsTrack()
        {
            var trackService = _serviceProvider.GetRequiredService<ISoundcloudTrackService>();

            var trackUrl = "https://soundcloud.com/kindridmusic/infrared-dreams";

            var track = await trackService.GetByUrlAsync(trackUrl);

            Assert.NotNull(track);
            Assert.False(string.IsNullOrWhiteSpace(track!.Title));
            Assert.False(string.IsNullOrWhiteSpace(track.User?.Username));
            Assert.False(string.IsNullOrWhiteSpace(track.Isrc));
        }

        [Fact]
        public async Task GetAlbumByNameAsync_ReturnsUrl()
        {
            var albumService = _serviceProvider.GetRequiredService<ISoundcloudAlbumService>();

            var albumName = "Inertia of Solitude";
            var artistName = "Kindrid";

            var albumUrl = await albumService.GetByNameAsync(albumName, artistName);

            Assert.False(string.IsNullOrWhiteSpace(albumUrl));
        }

        [Fact]
        public async Task GetAlbumByUrlAsync_ReturnsAlbum()
        {
            var albumService = _serviceProvider.GetRequiredService<ISoundcloudAlbumService>();

            var albumUrl = "https://soundcloud.com/kindridmusic/sets/intertia-of-solitude";

            var album = await albumService.GetByUrlAsync(albumUrl);

            Assert.NotNull(album);
            Assert.False(string.IsNullOrWhiteSpace(album!.Title));
            Assert.False(string.IsNullOrWhiteSpace(album.User?.Username));
        }
    }
}
