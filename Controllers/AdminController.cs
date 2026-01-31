using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Bridgo.Controllers;

/// <summary>
/// Admin Paneli - Sistem Yonetimi
/// Sadece Admin rolune sahip kullanicilar erisebilir
/// </summary>
[Authorize(Roles = "Admin")]
public class AdminController : Controller
{
    /// <summary>
    /// Dashboard - Genel bakis
    /// </summary>
    public IActionResult Index()
    {
        return View();
    }

    /// <summary>
    /// Capability Module yonetimi
    /// Her capability altindaki moduller (hiyerarsik)
    /// </summary>
    public IActionResult Modules()
    {
        return View();
    }

    /// <summary>
    /// Company Role yonetimi
    /// Firma ici roller (global ve firmaya ozel)
    /// </summary>
    public IActionResult Roles()
    {
        return View();
    }

    /// <summary>
    /// Vendor yonetimi
    /// Tum firmalari listele ve yonet
    /// </summary>
    public IActionResult Vendors()
    {
        return View();
    }

    /// <summary>
    /// Kullanici yonetimi
    /// Tum kullanicilari listele ve yonet
    /// </summary>
    public IActionResult Users()
    {
        return View();
    }

    /// <summary>
    /// Cografi veri yonetimi
    /// Ulke, eyalet, sehir verilerini seed et ve yonet
    /// </summary>
    public IActionResult Geography()
    {
        return View();
    }

    /// <summary>
    /// Dil yonetimi
    /// Sistemdeki dilleri yonet
    /// </summary>
    public IActionResult Languages()
    {
        return View();
    }

    /// <summary>
    /// Localization yonetimi
    /// Dil kaynaklarini (ceviri metinleri) yonet
    /// </summary>
    public IActionResult Localization()
    {
        return View();
    }

    /// <summary>
    /// Urun Kategorileri yonetimi
    /// Platform genelinde kullanilan global kategoriler
    /// </summary>
    public IActionResult Categories()
    {
        return View();
    }

    /// <summary>
    /// Kategori Talepleri yonetimi
    /// Kullanicilarin kategori taleplerini onayla/reddet
    /// </summary>
    public IActionResult CategoryRequests()
    {
        return View();
    }

    /// <summary>
    /// Hizmet (Capability) Talepleri yonetimi
    /// Kullanicilarin capability taleplerini onayla/reddet
    /// </summary>
    public IActionResult CapabilityRequests()
    {
        return View();
    }

    /// <summary>
    /// Profil Onay yonetimi
    /// Yayinlanmak istenen profilleri onayla/reddet
    /// </summary>
    public IActionResult ProfileApprovals()
    {
        return View();
    }
}
