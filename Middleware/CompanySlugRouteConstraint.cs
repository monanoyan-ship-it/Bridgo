namespace Bridgo.Middleware;

/// <summary>
/// Root-level /{slug} route'u icin constraint.
/// Reserved word'leri (controller adlari, statik klasorler) reddeder.
/// Sadece gecerli slug formatini kabul eder.
/// </summary>
public class CompanySlugRouteConstraint : IRouteConstraint
{
    /// <summary>
    /// Controller adlari, statik klasorler, ozel route'lar - bunlar slug OLAMAZ
    /// </summary>
    private static readonly HashSet<string> ReservedWords = new(StringComparer.OrdinalIgnoreCase)
    {
        // MVC Controller'lar
        "account", "admin", "branches", "cart", "catalog", "checkout",
        "company", "dashboard", "demands", "home", "invitations",
        "products", "provider", "public", "sitemap", "suppliers",
        "team", "vendorsetup", "warehouses",

        // Module Controller'lar (Dashboard alt sayfalari)
        "demandsdashboard", "financing", "investment", "messages",
        "orders", "productreviews", "proposals", "reports",
        "services", "settings", "stock", "suppliermanagement",

        // API prefix
        "api",

        // Statik dosya/klasorler
        "css", "js", "lib", "uploads", "images", "fonts",
        "favicon.ico", "robots.txt", "sitemap.xml",
        ".well-known",

        // Eski route'lar (geriye uyumluluk)
        "p", "c",

        // Ozel sayfalar
        "error", "privacy", "terms", "about", "contact",
        "login", "register", "logout", "profile",

        // SignalR
        "hubs"
    };

    public bool Match(
        HttpContext? httpContext,
        IRouter? route,
        string routeKey,
        RouteValueDictionary values,
        RouteDirection routeDirection)
    {
        if (!values.TryGetValue(routeKey, out var routeValue) || routeValue == null)
            return false;

        var slug = routeValue.ToString()!;

        // Bos veya cok kisa slug'lari reddet
        if (string.IsNullOrWhiteSpace(slug) || slug.Length < 3)
            return false;

        // Sadece kucuk harf, rakam ve tire (a-z, 0-9, -)
        if (!IsValidSlugFormat(slug))
            return false;

        // Reserved word kontrolu
        if (ReservedWords.Contains(slug))
            return false;

        // Nokta iceren path'leri reddet (dosya uzantilari: .css, .js, .ico vb.)
        if (slug.Contains('.'))
            return false;

        return true;
    }

    /// <summary>
    /// Slug sadece kucuk harf, rakam ve tire icerebilir.
    /// Tire ile baslayamaz veya bitemez.
    /// </summary>
    private static bool IsValidSlugFormat(string slug)
    {
        if (slug[0] == '-' || slug[^1] == '-')
            return false;

        for (int i = 0; i < slug.Length; i++)
        {
            var c = slug[i];
            if (c != '-' && !(c >= 'a' && c <= 'z') && !(c >= '0' && c <= '9'))
                return false;
        }

        return true;
    }

    /// <summary>
    /// Slug uretirken reserved word kontrolu icin static metod.
    /// Service katmanindan cagirilir.
    /// </summary>
    public static bool IsReservedWord(string slug)
    {
        return ReservedWords.Contains(slug);
    }
}
