namespace Bridgo.Middleware;

/// <summary>
/// LandingOnly modda sadece landing page ve waitlist API'ye izin verir.
/// Diger tum istekleri root'a yonlendirir.
/// </summary>
public class LandingOnlyMiddleware
{
    private readonly RequestDelegate _next;
    private readonly bool _isLandingOnly;

    private static readonly string[] AllowedPaths = new[]
    {
        "/",
        "/api/waitlist",
        "/Home/Error",
        "/Home/Privacy"
    };

    private static readonly string[] AllowedPrefixes = new[]
    {
        "/css/",
        "/js/",
        "/lib/",
        "/images/",
        "/favicon",
        "/_framework/",
        "/_content/"
    };

    public LandingOnlyMiddleware(RequestDelegate next, IConfiguration configuration)
    {
        _next = next;
        _isLandingOnly = configuration.GetValue<bool>("LandingOnly");
    }

    public async Task InvokeAsync(HttpContext context)
    {
        if (!_isLandingOnly)
        {
            await _next(context);
            return;
        }

        var path = context.Request.Path.Value ?? "/";

        // Statik dosyalara izin ver
        if (path.Contains('.') || AllowedPrefixes.Any(p => path.StartsWith(p, StringComparison.OrdinalIgnoreCase)))
        {
            await _next(context);
            return;
        }

        // Izin verilen path'ler
        if (AllowedPaths.Any(p => path.Equals(p, StringComparison.OrdinalIgnoreCase)))
        {
            await _next(context);
            return;
        }

        // Geri kalan her seyi root'a yonlendir
        context.Response.Redirect("/");
    }
}
