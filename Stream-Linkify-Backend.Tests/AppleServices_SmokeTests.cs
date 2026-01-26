using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Stream_Linkify_Backend.Interfaces.Apple;
using Stream_Linkify_Backend.Services.Apple;
using System.IdentityModel.Tokens.Jwt;

// For some reason, Apple token tests fail when run in parallel
[assembly: CollectionBehavior(DisableTestParallelization = true)]

namespace Stream_Linkify_Backend.Tests
{
    public class AppleServices_SmokeTests
    {
        private readonly IConfiguration _config;
        private readonly ServiceProvider _serviceProvider;

        public AppleServices_SmokeTests()
        {
            var services = new ServiceCollection();

            _config = new ConfigurationBuilder()
                .AddUserSecrets<AppleServices_SmokeTests>() // Make sure secrets contain AppleMusicKit keys
                .Build();

            services.AddSingleton<IConfiguration>(_config);

            // Logging & HTTP
            services.AddLogging(b => b.AddConsole());
            services.AddHttpClient();
            services.AddDistributedMemoryCache();

            // Core Apple services
            services.AddSingleton<IAppleApiClient, AppleApiClient>();
            services.AddSingleton<IAppleTokenService, AppleTokenService>();
            services.AddScoped<IAppleTrackService, AppleTrackService>();
            services.AddScoped<IAppleAlbumService, AppleAlbumService>();

            _serviceProvider = services.BuildServiceProvider();
        }

        [Fact]
        public async Task GetValidTokenAsync_ReturnsToken()
        {
            var svc = _serviceProvider.GetRequiredService<IAppleTokenService>();

            var token = await svc.GetValidTokenAsync();

            Assert.NotNull(token);
            Assert.False(string.IsNullOrWhiteSpace(token));
        }

        [Fact]
        public async Task GetValidToken_ShouldProduceValidJwt()
        {
            var svc = _serviceProvider.GetRequiredService<IAppleTokenService>();

            var token = await svc.GetValidTokenAsync();

            Assert.False(string.IsNullOrWhiteSpace(token));

            var handler = new JwtSecurityTokenHandler();
            var jwt = handler.ReadJwtToken(token);

            // Header checks
            Assert.Equal("ES256", jwt.Header["alg"]);
            Assert.Equal(_config["AppleMusicKit:KeyId"], jwt.Header["kid"]);

            // Payload checks
            Assert.Equal(_config["AppleMusicKit:TeamId"], jwt.Payload["iss"]);

            var iat = Convert.ToInt64(jwt.Payload["iat"]);
            var exp = Convert.ToInt64(jwt.Payload["exp"]);

            var issuedAt = DateTimeOffset.FromUnixTimeSeconds(iat);
            var expiresAt = DateTimeOffset.FromUnixTimeSeconds(exp);

            Assert.True(issuedAt <= DateTimeOffset.UtcNow.AddMinutes(1), "IssuedAt is too far in the future");
            Assert.True(expiresAt > issuedAt, "Expiration must be after IssuedAt");

            var maxLifetimeSeconds = 15777000; // Apple's max allowed
            var actualLifetimeSeconds = exp - iat;

            Assert.True(actualLifetimeSeconds <= maxLifetimeSeconds,
                $"Expiration must be <= {maxLifetimeSeconds} seconds (~6 months)");
        }

        [Fact]
        public async Task GetTrackByUrlAsync_ReturnsTrack()
        {
            var trackService = _serviceProvider.GetRequiredService<IAppleTrackService>();

            var testTrackUrl = "https://music.apple.com/us/album/wounded/1825854595?i=1825854596";

            var track = await trackService.GetTrackByUrlAsync(testTrackUrl);

            Assert.NotNull(track);
            Assert.False(string.IsNullOrWhiteSpace(track!.Attributes.Name));
            Assert.False(string.IsNullOrWhiteSpace(track.Attributes.ArtistName));
            Assert.False(string.IsNullOrWhiteSpace(track.Attributes.Isrc));
        }

        [Fact]
        public async Task GetTrackByIsrcAsync_ReturnsTrack()
        {
            var trackService = _serviceProvider.GetRequiredService<IAppleTrackService>();

            var testIsrc = "GBRKQ2482423";

            var track = await trackService.GetTrackUrlByNameAsync(testIsrc, "", "");

            Assert.NotNull(track);
        }

        [Fact]
        public async Task GetAlbumByUrlAsync_ReturnsAlbum()
        {
            var albumService = _serviceProvider.GetService<IAppleAlbumService>();

            var testAlbumUrl = "https://music.apple.com/us/album/inertia-of-solitude/1808747512";

            var album = await albumService!.GetByUrlAsync(testAlbumUrl);

            Assert.NotNull(album);
        }

        [Fact]
        public async Task GetAlbumUrlByUpc_ReturnsUrl()
        {
            var albumService = _serviceProvider.GetService<IAppleAlbumService>();

            var exampleAlbumUpc = "199257088807"; // Kindrid - Inertia of Solitude
            var exampleAlbumLink = "https://music.apple.com/us/album/inertia-of-solitude/1808747512";

            var responseUrl = await albumService!.GetUrlByNameAsync(exampleAlbumUpc, "Inertia of Solitude", "Kindrid");
            Assert.NotNull(responseUrl);

            Assert.Equal(responseUrl, exampleAlbumLink);
        }
    }
}