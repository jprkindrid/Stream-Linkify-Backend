using Microsoft.IdentityModel.Tokens;
using Stream_Linkify_Backend.Helpers;
using Stream_Linkify_Backend.Interfaces.Apple;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Cryptography;

namespace Stream_Linkify_Backend.Services.Apple
{
    public class AppleTokenService : IAppleTokenService
    {
        private string? token;
        private long? expiresAt;
        private readonly IConfiguration config;
        private readonly ILogger<AppleTokenService> logger;
        private readonly Lock lockObj = new();

        public AppleTokenService(
            IConfiguration config,
            ILogger<AppleTokenService> logger)
        {
            this.config = config;
            this.logger = logger;
        }

        public string GetValidToken()
        {
            lock (lockObj)
            {
                if (token == null || !IsValidToken())
                {
                    logger.LogInformation("Generating new Apple Music developer token");
                    (token, expiresAt) = GenerateDeveloperToken();
                }

                return token!;
            }
        }

        private (string token, long expiresAt) GenerateDeveloperToken()
        {
            var teamId = RequiredConfig.Get(config, "AppleMusicKit:TeamId");
            var keyId = RequiredConfig.Get(config, "AppleMusicKit:KeyId");

            string privateKeyPem = LoadPrivateKey();

            var lines = privateKeyPem.Split(["\n", "\r"], StringSplitOptions.RemoveEmptyEntries);
            var base64Body = string.Concat(lines.Skip(1).TakeWhile(l => !l.StartsWith("-----")));
            var keyBytes = Convert.FromBase64String(base64Body);

            // Parse key and re-create with explicit parameters (bypasses Azure CNG storage)
            ECParameters ecParameters;
            using (var tempEcdsa = ECDsa.Create())
            {
                tempEcdsa.ImportPkcs8PrivateKey(keyBytes, out _);
                ecParameters = tempEcdsa.ExportParameters(true);
            }

            using var ecdsa = ECDsa.Create(ecParameters);

            var securityKey = new ECDsaSecurityKey(ecdsa) { KeyId = keyId };
            var creds = new SigningCredentials(securityKey, SecurityAlgorithms.EcdsaSha256);

            var now = DateTimeOffset.UtcNow;
            var expires = now.AddDays(179);

            var descriptor = new SecurityTokenDescriptor
            {
                Issuer = teamId,
                IssuedAt = now.UtcDateTime,
                Expires = expires.UtcDateTime,
                SigningCredentials = creds
            };

            var handler = new JwtSecurityTokenHandler();
            var securityToken = handler.CreateToken(descriptor);
            string jwt = handler.WriteToken(securityToken);

            return (jwt, expires.ToUnixTimeSeconds());
        }

        private string LoadPrivateKey()
        {
            string? privateKeyPem = config["ApplePrivateKey"];

            if (!string.IsNullOrWhiteSpace(privateKeyPem)){
                logger.LogInformation("Loaded Apple Music private key from configuration");
                return privateKeyPem;
            }

            privateKeyPem = Environment.GetEnvironmentVariable("APPLE_PRIVATE_KEY");

            if (!string.IsNullOrWhiteSpace(privateKeyPem)){
                logger.LogInformation("Loaded Apple Music private key from environment variable");
                return privateKeyPem;
            }

            logger.LogInformation("Loading Apple Music private key from file");
            var keyId = RequiredConfig.Get(config, "AppleMusicKit:KeyId");
            string privateKeyPath = Path.Combine("Keys", $"AuthKey_{keyId}.p8");

            if (!Path.IsPathRooted(privateKeyPath))
            {
                string projectRoot = Path.GetFullPath(
                    Path.Combine(AppContext.BaseDirectory, @"..\..\.."));
                privateKeyPath = Path.Combine(projectRoot, privateKeyPath);
            }

            if (!File.Exists(privateKeyPath))
                throw new FileNotFoundException(
                    $"Apple Music private key not found at {privateKeyPath}");

            return File.ReadAllText(privateKeyPath);
        }

        private bool IsValidToken()
        {
            if (token == null || expiresAt == null)
                return false;

            return DateTimeOffset.FromUnixTimeSeconds(expiresAt.Value) >
                   DateTimeOffset.UtcNow.AddMinutes(5);
        }
    }
}