using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Text;
using System.Xml.Linq;
using Bridgo.Data;

namespace Bridgo.Controllers;

/// <summary>
/// SEO: Sitemap.xml generator
/// </summary>
public class SitemapController : Controller
{
    private readonly ApplicationDbContext _context;
    private readonly IConfiguration _configuration;

    public SitemapController(ApplicationDbContext context, IConfiguration configuration)
    {
        _context = context;
        _configuration = configuration;
    }

    [HttpGet("/sitemap.xml")]
    [ResponseCache(Duration = 3600)] // 1 saat cache
    public async Task<IActionResult> Index()
    {
        var baseUrl = $"{Request.Scheme}://{Request.Host}";

        XNamespace ns = "http://www.sitemaps.org/schemas/sitemap/0.9";

        var urls = new List<XElement>();

        // Ana sayfa
        urls.Add(CreateUrlElement(ns, baseUrl, "/", DateTime.UtcNow.Date, "daily", "1.0"));

        // Public Demands sayfasi
        urls.Add(CreateUrlElement(ns, baseUrl, "/Demands", DateTime.UtcNow.Date, "hourly", "0.9"));

        // Public Suppliers sayfasi
        urls.Add(CreateUrlElement(ns, baseUrl, "/Suppliers", DateTime.UtcNow.Date, "daily", "0.8"));

        // Aktif ve indexlenebilir talepler (Status=2 Active, Visibility=1 Public)
        var demands = await _context.PublicDemands
            .Where(d => d.Status == 2 &&
                       d.Visibility == 1 &&
                       d.IsIndexable &&
                       !d.IsDeleted)
            .Select(d => new { d.Slug, d.UpdatedAt, d.CreatedAt })
            .ToListAsync();

        foreach (var demand in demands)
        {
            var lastmod = demand.UpdatedAt ?? demand.CreatedAt;
            urls.Add(CreateUrlElement(ns, baseUrl, $"/Demands/{demand.Slug}", lastmod, "weekly", "0.7"));
        }

        // Aktif supplier profilleri
        var suppliers = await _context.SupplierProfiles
            .Where(s => s.IsPubliclyVisible && !s.IsDeleted)
            .Select(s => new { s.Slug, s.UpdatedAt, s.CreatedAt })
            .ToListAsync();

        foreach (var supplier in suppliers)
        {
            var lastmod = supplier.UpdatedAt ?? supplier.CreatedAt;
            urls.Add(CreateUrlElement(ns, baseUrl, $"/Suppliers/{supplier.Slug}", lastmod, "weekly", "0.6"));
        }

        var sitemap = new XDocument(
            new XDeclaration("1.0", "UTF-8", null),
            new XElement(ns + "urlset", urls)
        );

        var xml = sitemap.Declaration + Environment.NewLine + sitemap.ToString();
        return Content(xml, "application/xml", Encoding.UTF8);
    }

    private static XElement CreateUrlElement(XNamespace ns, string baseUrl, string path, DateTime lastmod, string changefreq, string priority)
    {
        return new XElement(ns + "url",
            new XElement(ns + "loc", baseUrl + path),
            new XElement(ns + "lastmod", lastmod.ToString("yyyy-MM-dd")),
            new XElement(ns + "changefreq", changefreq),
            new XElement(ns + "priority", priority)
        );
    }

    [HttpGet("/robots.txt")]
    [ResponseCache(Duration = 86400)] // 1 gun cache
    public IActionResult Robots()
    {
        var baseUrl = $"{Request.Scheme}://{Request.Host}";
        var content = $@"User-agent: *
Allow: /
Allow: /Demands
Allow: /Suppliers

Disallow: /Dashboard
Disallow: /Account
Disallow: /Admin
Disallow: /api/

Sitemap: {baseUrl}/sitemap.xml
";
        return Content(content, "text/plain", Encoding.UTF8);
    }
}
