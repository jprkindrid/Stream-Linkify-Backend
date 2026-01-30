using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Stream_Linkify_Backend.Interfaces.Soundcloud;
using Stream_Linkify_Backend.Services.Soundcloud;
using System.Net;
using Xunit;



namespace Stream_Linkify_Backend.Tests
{


    public class SoundcloudServices_SmokeTests
    {


        private readonly ServiceProvider _serviceProvider;
        private readonly ITestOutputHelper _output;


        public SoundcloudServices_SmokeTests(ITestOutputHelper output)
        {
            _output = output;

            var services = new ServiceCollection();

            var config = new ConfigurationBuilder()
                .AddUserSecrets<SoundcloudServices_SmokeTests>(optional: true)
                .AddEnvironmentVariables()
                .Build();

            services.AddSingleton<IConfiguration>(config);

            // Logging & HTTP - use Debug level for more visibility in tests
            services.AddLogging();
            services.AddHttpClient();
            services.AddDistributedMemoryCache();
            services.AddSingleton<ISoundcloudTokenService, SoundcloudTokenService>();
            // Soundcloud headers work challenge
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

            _output.WriteLine($"Searching for track: '{trackName}' by '{artistName}' (ISRC: {isrc})");

            var trackUrl = await trackService.GetByNameAsync(trackName, artistName, isrc);

            _output.WriteLine($"Result URL: {trackUrl ?? "(null)"}");

            Assert.False(string.IsNullOrWhiteSpace(trackUrl), "Track URL should not be null or empty");
        }

        [Fact]
        public async Task GetTrackByUrlAsync_ReturnsTrack()
        {
            var trackService = _serviceProvider.GetRequiredService<ISoundcloudTrackService>();

            var trackUrl = "https://soundcloud.com/kindridmusic/infrared-dreams";

            var track = await trackService.GetByUrlAsync(trackUrl);

            // Detailed diagnostics for debugging
            if (track == null)
            {
                _output.WriteLine("Track returned null - check logs for API response details");
                Assert.Fail("Track was null. Check test output and logs for SoundCloud API response.");
            }

            _output.WriteLine($"Track ID: {track.Id}");
            _output.WriteLine($"Track Title: {track.Title ?? "(null)"}");
            _output.WriteLine($"Track User: {track.User?.Username ?? "(null)"}");
            _output.WriteLine($"Track Isrc (root): {track.Isrc ?? "(null)"}");
            _output.WriteLine($"Track Isrc (publisher_metadata): {track.PublisherMetadata?.Isrc ?? "(null)"}");
            _output.WriteLine($"Track EffectiveIsrc: {track.EffectiveIsrc ?? "(null)"}");

            Assert.False(string.IsNullOrWhiteSpace(track.Title), "Track.Title should not be null or empty");
            Assert.False(string.IsNullOrWhiteSpace(track.User?.Username), "Track.User.Username should not be null or empty");
        }

        [Fact]
        public async Task GetAlbumByNameAsync_ReturnsUrl()
        {
            var albumService = _serviceProvider.GetRequiredService<ISoundcloudAlbumService>();

            var albumName = "Inertia of Solitude";
            var artistName = "Kindrid";

            _output.WriteLine($"Searching for album: '{albumName}' by '{artistName}'");

            var albumUrl = await albumService.GetByNameAsync(albumName, artistName);

            _output.WriteLine($"Result URL: {albumUrl ?? "(null)"}");

            Assert.False(string.IsNullOrWhiteSpace(albumUrl), "Album URL should not be null or empty");
        }

        [Fact]
        public async Task GetAlbumByUrlAsync_ReturnsAlbum()
        {
            var albumService = _serviceProvider.GetRequiredService<ISoundcloudAlbumService>();

            var albumUrl = "https://soundcloud.com/kindridmusic/sets/intertia-of-solitude";

            var album = await albumService.GetByUrlAsync(albumUrl);

            // Detailed diagnostics for debugging
            if (album == null)
            {
                _output.WriteLine("Album returned null - check logs for API response details");
                Assert.Fail("Album was null. Check test output and logs for SoundCloud API response.");
            }

            _output.WriteLine($"Album ID: {album.Id}");
            _output.WriteLine($"Album Title: {album.Title ?? "(null)"}");
            _output.WriteLine($"Album User: {album.User?.Username ?? "(null)"}");
            _output.WriteLine($"Album TrackCount: {album.TrackCount}");

            Assert.False(string.IsNullOrWhiteSpace(album.Title), "Album.Title should not be null or empty");
            Assert.False(string.IsNullOrWhiteSpace(album.User?.Username), "Album.User.Username should not be null or empty");
        }
    }
}
