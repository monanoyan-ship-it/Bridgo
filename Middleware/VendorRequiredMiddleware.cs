using System.Security.Claims;
using Microsoft.AspNetCore.Identity;
using Bridgo.Models.Identity;

namespace Bridgo.Middleware;

/// <summary>
/// Kullanicinin VendorId'si yoksa VendorSetup sayfasina yonlendirir
/// Admin kullanicilar ve belirli sayfalar haric tutulur
/// </summary>
public class VendorRequiredMiddleware
{
    private readonly RequestDelegate _next;

    // Bu path'ler VendorId kontrolunden muaf
    private static readonly string[] ExemptPaths = new[]
    {
        "/vendorsetup",
        "/account",
        "/api/vendorsetupapi",
        "/api/auth",
        "/error",
        "/home/error",
        "/lib",
        "/js",
        "/css",
        "/images"
    };

    public VendorRequiredMiddleware(RequestDelegate next)
    {
        _next = next;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        // Kullanici giris yapmamissa devam et (Authentication middleware halleder)
        if (context.User.Identity?.IsAuthenticated != true)
        {
            await _next(context);
            return;
        }

        // Path kontrolu - muaf path'ler
        var path = context.Request.Path.Value?.ToLower() ?? "";
        if (ExemptPaths.Any(p => path.StartsWith(p)))
        {
            await _next(context);
            return;
        }

        // Static dosyalar icin kontrol yapma
        if (path.Contains('.') && (
            path.EndsWith(".js") ||
            path.EndsWith(".css") ||
            path.EndsWith(".png") ||
            path.EndsWith(".jpg") ||
            path.EndsWith(".ico") ||
            path.EndsWith(".woff") ||
            path.EndsWith(".woff2")))
        {
            await _next(context);
            return;
        }

        // Admin kontrolu - Admin rolu olanlar VendorId olmadan girebilir
        // Ama Dashboard'a erisemezler - Admin Panel'e yonlendirilirler
        if (context.User.IsInRole("Admin"))
        {
            // Admin kullanici Dashboard'a girmeye calisiyorsa Admin Panel'e yonlendir
            if (path.StartsWith("/dashboard") || path.StartsWith("/company") ||
                path.StartsWith("/team") || path.StartsWith("/branches"))
            {
                context.Response.Redirect("/Admin");
                return;
            }
            await _next(context);
            return;
        }

        // VendorId kontrolu
        var userManager = context.RequestServices.GetRequiredService<UserManager<ApplicationUser>>();
        var userId = context.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

        if (!string.IsNullOrEmpty(userId) && int.TryParse(userId, out var userIdInt))
        {
            var user = await userManager.FindByIdAsync(userId);

            // VendorId yoksa VendorSetup'a yonlendir
            if (user != null && !user.VendorId.HasValue)
            {
                // API isteklerinde JSON donelim
                if (path.StartsWith("/api/"))
                {
                    context.Response.StatusCode = 403;
                    context.Response.ContentType = "application/json";
                    await context.Response.WriteAsync("{\"error\":\"VendorId required\",\"redirectUrl\":\"/VendorSetup\"}");
                    return;
                }

                // Normal sayfalarda yonlendir
                context.Response.Redirect("/VendorSetup");
                return;
            }
        }

        await _next(context);
    }
}

/// <summary>
/// Extension method for adding VendorRequired middleware
/// </summary>
public static class VendorRequiredMiddlewareExtensions
{
    public static IApplicationBuilder UseVendorRequired(this IApplicationBuilder builder)
    {
        return builder.UseMiddleware<VendorRequiredMiddleware>();
    }
}
