using Microsoft.AspNetCore.Mvc;
using Bridgo.Services;

namespace Bridgo.Controllers;

/// <summary>
/// Tum controller'lar bu base class'tan turetilmeli
/// T() metodu ile ceviri yapilabilir
/// </summary>
public abstract class BaseController : Controller
{
    private ILocalizationService? _localizationService;

    protected ILocalizationService LocalizationService =>
        _localizationService ??= HttpContext.RequestServices.GetRequiredService<ILocalizationService>();

    /// <summary>
    /// Controller icinde ceviri yapmak icin
    /// Ornek: T("Common.SaveSuccess") -> "Kayit basarili"
    /// </summary>
    protected string T(string resourceKey)
    {
        return LocalizationService.T(resourceKey);
    }

    /// <summary>
    /// Format parametreli ceviri
    /// </summary>
    protected string T(string resourceKey, params object[] args)
    {
        return LocalizationService.T(resourceKey, args);
    }
}
