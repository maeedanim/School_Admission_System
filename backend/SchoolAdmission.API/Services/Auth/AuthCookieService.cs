
namespace SchoolAdmission.API.Services.Auth;

public class AuthCookieService
{
    private const string RefreshCookieName = "school_admission_refresh";

    private readonly IConfiguration _configuration;

    public AuthCookieService(IConfiguration configuration)
    {
        _configuration = configuration;
    }

    public string? GetRefreshToken(HttpRequest request)
    {
        return request.Cookies[RefreshCookieName];
    }

    public void SetRefreshToken(
        HttpRequest request,
        HttpResponse response,
        string refreshToken,
        DateTime expiresAtUtc)
    {
        var cookieOptions = CreateCookieOptions(request);

        cookieOptions.Expires = new DateTimeOffset(
            DateTime.SpecifyKind(
                expiresAtUtc,
                DateTimeKind.Utc));

        response.Cookies.Append(
            RefreshCookieName,
            refreshToken,
            cookieOptions);
    }

    public void ClearRefreshToken(
        HttpRequest request,
        HttpResponse response)
    {
        response.Cookies.Delete(
            RefreshCookieName,
            CreateCookieOptions(request));
    }

    public bool IsAllowedFrontendOrigin(HttpRequest request)
    {
        var requestOrigin = request.Headers.Origin.ToString();

        var configuredOrigin =
            _configuration["FRONTEND_ORIGIN"]
            ?? "http://localhost:5173";

        if (string.IsNullOrWhiteSpace(requestOrigin))
            return false;

        if (!Uri.TryCreate(
                requestOrigin,
                UriKind.Absolute,
                out var requestUri))
        {
            return false;
        }

        if (!Uri.TryCreate(
                configuredOrigin,
                UriKind.Absolute,
                out var configuredUri))
        {
            return false;
        }

        var normalizedRequestOrigin =
            requestUri.GetLeftPart(UriPartial.Authority);

        var normalizedConfiguredOrigin =
            configuredUri.GetLeftPart(UriPartial.Authority);

        return string.Equals(
            normalizedRequestOrigin,
            normalizedConfiguredOrigin,
            StringComparison.OrdinalIgnoreCase);
    }

    private static CookieOptions CreateCookieOptions(
        HttpRequest request)
    {
        var isLocalhost = request.Host.Host.Equals(
            "localhost",
            StringComparison.OrdinalIgnoreCase);

        return new CookieOptions
        {
            HttpOnly = true,
            Secure = request.IsHttps || isLocalhost,
            SameSite = SameSiteMode.None,
            Path = "/api/auth",
            IsEssential = true
        };
    }
}
