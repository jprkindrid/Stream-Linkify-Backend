using Microsoft.IdentityModel.Tokens;
using Stream_Linkify_Backend.Helpers;
using Stream_Linkify_Backend.Interfaces.Apple;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Cryptography;
using Org.BouncyCastle.OpenSsl;
using Org.BouncyCastle.Crypto.Parameters;

namespace Stream_Linkify_Backend.Services.Apple
{
    public class AppleTokenService(
        IConfiguration config,
        ILogger<AppleTokenService> logger) : IAppleTokenService
    {
        private string? token;
        private long? expiresAt;
        private readonly IConfiguration config = config;
        private readonly ILogger<AppleTokenService> logger = logger;
        private readonly Lock lockObj = new();

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

            /*
             * We use BouncyCastle instead of native .NET methods here because
             * Azure App Service's sandbox restricts CNG (Cryptography Next Generation)
             * key storage operations. Loading the P8 key via ImportPkcs8PrivateKey()
             * fails with: "The system cannot find the file specified"
             * 
             * BouncyCastle parses the key entirely in managed memory, bypassing CNG.
             */

            using var reader = new StringReader(privateKeyPem);
            var pemReader = new PemReader(reader);
            var keyObject = pemReader.ReadObject();

            ECPrivateKeyParameters privateKeyParams;

            if (keyObject is Org.BouncyCastle.Crypto.AsymmetricCipherKeyPair keyPair)
            {
                privateKeyParams = (ECPrivateKeyParameters)keyPair.Private;
            }
            else if (keyObject is ECPrivateKeyParameters privateKey)
            {
                privateKeyParams = privateKey;
            }
            else
            {
                throw new InvalidOperationException(
                    $"Unexpected key type: {keyObject?.GetType().Name ?? "null"}");
            }

            var q = privateKeyParams.Parameters.G.Multiply(privateKeyParams.D).Normalize();
            var ecParameters = new ECParameters
            {
                Curve = ECCurve.NamedCurves.nistP256,
                D = privateKeyParams.D.ToByteArrayUnsigned(),
                Q =
                {
                    X = q.AffineXCoord.GetEncoded(),
                    Y = q.AffineYCoord.GetEncoded()
                }
            };

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

            if (!string.IsNullOrWhiteSpace(privateKeyPem))
            {
                logger.LogInformation("Loaded Apple Music private key from configuration");
                return privateKeyPem;
            }

            privateKeyPem = Environment.GetEnvironmentVariable("APPLE_PRIVATE_KEY");

            if (!string.IsNullOrWhiteSpace(privateKeyPem))
            {
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