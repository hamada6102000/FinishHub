using System.IdentityModel.Tokens.Jwt;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Text.Json;
using Microsoft.IdentityModel.Tokens;

namespace test.Services;

// Verifies Firebase Authentication ID tokens against Firebase's public signing
// certs, per https://firebase.google.com/docs/auth/admin/verify-id-tokens#verify_id_tokens_using_a_third-party_jwt_library
public class FirebaseAuthValidator : IFirebaseAuthValidator
{
    private const string CertsUrl = "https://www.googleapis.com/service_accounts/v1/metadata/x509/securetoken@system.gserviceaccount.com";

    private static readonly SemaphoreSlim CacheLock = new(1, 1);
    private static Dictionary<string, X509Certificate2> _certCache = new();
    private static DateTime _certCacheExpiresAt = DateTime.MinValue;

    private readonly IConfiguration _config;
    private readonly IHttpClientFactory _httpClientFactory;

    public FirebaseAuthValidator(IConfiguration config, IHttpClientFactory httpClientFactory)
    {
        _config = config;
        _httpClientFactory = httpClientFactory;
    }

    public async Task<ExternalAuthPayload> ValidateAsync(string idToken)
    {
        var projectId = _config["Firebase:ProjectId"];
        if (string.IsNullOrWhiteSpace(projectId))
            throw new InvalidOperationException("Firebase:ProjectId is not configured.");

        var handler = new JwtSecurityTokenHandler { MapInboundClaims = false };
        var unverified = handler.ReadJwtToken(idToken);
        var kid = unverified.Header.Kid;
        if (string.IsNullOrEmpty(kid))
            throw new SecurityTokenException("Token is missing a key id.");

        var cert = await GetCertificateAsync(kid);

        var parameters = new TokenValidationParameters
        {
            ValidateIssuer           = true,
            ValidIssuer               = $"https://securetoken.google.com/{projectId}",
            ValidateAudience         = true,
            ValidAudience             = projectId,
            ValidateLifetime         = true,
            ValidateIssuerSigningKey = true,
            IssuerSigningKey         = new X509SecurityKey(cert),
            ClockSkew                = TimeSpan.FromMinutes(1),
        };

        var principal = handler.ValidateToken(idToken, parameters, out _);

        var authTimeClaim = principal.FindFirst("auth_time")?.Value;
        if (authTimeClaim == null || !long.TryParse(authTimeClaim, out var authTime) ||
            DateTimeOffset.FromUnixTimeSeconds(authTime) > DateTimeOffset.UtcNow.AddMinutes(1))
            throw new SecurityTokenException("Token auth_time is invalid.");

        var subject = principal.FindFirst("sub")?.Value ?? principal.FindFirst("user_id")?.Value;
        var email   = principal.FindFirst("email")?.Value;
        if (string.IsNullOrEmpty(subject) || string.IsNullOrEmpty(email))
            throw new SecurityTokenException("Token is missing required claims.");

        return new ExternalAuthPayload
        {
            Subject = subject,
            Email   = email,
            Name    = principal.FindFirst("name")?.Value,
        };
    }

    private async Task<X509Certificate2> GetCertificateAsync(string kid)
    {
        if (DateTime.UtcNow < _certCacheExpiresAt && _certCache.TryGetValue(kid, out var cached))
            return cached;

        await CacheLock.WaitAsync();
        try
        {
            if (DateTime.UtcNow < _certCacheExpiresAt && _certCache.TryGetValue(kid, out cached))
                return cached;

            var client = _httpClientFactory.CreateClient();
            var response = await client.GetAsync(CertsUrl);
            response.EnsureSuccessStatusCode();

            var json = await response.Content.ReadAsStringAsync();
            var certsByKid = JsonSerializer.Deserialize<Dictionary<string, string>>(json)
                ?? new Dictionary<string, string>();

            var maxAge = response.Headers.CacheControl?.MaxAge ?? TimeSpan.FromHours(1);

            _certCache = certsByKid.ToDictionary(
                kv => kv.Key,
                kv => X509CertificateLoader.LoadCertificate(Encoding.UTF8.GetBytes(kv.Value)));
            _certCacheExpiresAt = DateTime.UtcNow.Add(maxAge);

            if (!_certCache.TryGetValue(kid, out cached))
                throw new SecurityTokenException("Unable to find a matching Firebase signing key.");

            return cached;
        }
        finally
        {
            CacheLock.Release();
        }
    }
}
