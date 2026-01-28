using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Stream_Linkify_Backend.Interfaces.Soundcloud;
using Stream_Linkify_Backend.Services.Soundcloud;
using Xunit;

namespace Stream_Linkify_Backend.Tests
{
    public class SoundcloudTokenService_SmokeTests
    {
        private readonly ServiceProvider _serviceProvider;

        public SoundcloudTokenService_SmokeTests()
        {
            var services = new ServiceCollection();

            // Logging & HTTP
            services.AddLogging(b => b.AddConsole());
            services.AddHttpClient();
            services.AddDistributedMemoryCache();

            // Config from user-secrets + env vars
            var config = new ConfigurationBuilder()
                .AddUserSecrets<SoundcloudTokenService_SmokeTests>(optional: true)
                .AddEnvironmentVariables()
                .Build();

            services.AddSingleton<IConfiguration>(config);

            // Soundcloud token service
            services.AddSingleton<ISoundcloudTokenService, SoundcloudTokenService>();

            _serviceProvider = services.BuildServiceProvider();
        }

        [Fact]
        public async Task GetValidTokenAsync_ReturnsToken()
        {
            var svc = _serviceProvider.GetRequiredService<ISoundcloudTokenService>();

            var token = await svc.GetValidTokenAsync();

            Assert.NotNull(token);
            Assert.False(string.IsNullOrWhiteSpace(token.AccessToken));
            Assert.True(token.ExpiresIn > 0);
            Assert.True(token.ExpiresAt > 0);
        }

        //[Fact]
        //public async Task GetValidTokenAsync_ReturnsCachedTokenOnSecondCall()
        //{
        //    var svc = _serviceProvider.GetRequiredService<ISoundcloudTokenService>();

        //    var token1 = await svc.GetValidTokenAsync();
        //    var token2 = await svc.GetValidTokenAsync();

        //    Assert.NotNull(token1);
        //    Assert.NotNull(token2);
        //    // Same cached token should be returned
        //    Assert.Equal(token1.AccessToken, token2.AccessToken);
        //    Assert.Equal(token1.ExpiresAt, token2.ExpiresAt);
        //}
    }
}
