using Microsoft.AspNetCore.Html;
using Microsoft.AspNetCore.Mvc.Rendering;
using Bridgo.Services;

namespace Bridgo.Extensions;

public static class LocalizationExtensions
{
    /// <summary>
    /// View'larda T("ResourceKey") veya T("ResourceKey", "Default") seklinde kullanilir
    /// Ornek: @Html.T("Common.Save") -> "Kaydet"
    /// Ornek: @Html.T("Common.Save", "Kaydet") -> key yoksa "Kaydet"
    /// </summary>
    public static IHtmlContent T(this IHtmlHelper htmlHelper, string resourceKey, string? defaultValue = null)
    {
        var localizationService = htmlHelper.ViewContext.HttpContext.RequestServices
            .GetService<ILocalizationService>();

        if (localizationService == null)
            return new HtmlString(defaultValue ?? resourceKey);

        var value = localizationService.GetResource(resourceKey, defaultValue);
        return new HtmlString(value);
    }

    /// <summary>
    /// Format parametreli ceviri
    /// Ornek: @Html.TFormat("Common.Welcome", userName) -> "Hosgeldiniz, {0}"
    /// </summary>
    public static IHtmlContent TFormat(this IHtmlHelper htmlHelper, string resourceKey, params object[] args)
    {
        var localizationService = htmlHelper.ViewContext.HttpContext.RequestServices
            .GetService<ILocalizationService>();

        if (localizationService == null)
            return new HtmlString(resourceKey);

        var value = localizationService.T(resourceKey, args);
        return new HtmlString(value);
    }

    /// <summary>
    /// Belirli bir dil icin ceviri
    /// </summary>
    public static IHtmlContent T(this IHtmlHelper htmlHelper, string resourceKey, int languageId, string? defaultValue = null)
    {
        var localizationService = htmlHelper.ViewContext.HttpContext.RequestServices
            .GetService<ILocalizationService>();

        if (localizationService == null)
            return new HtmlString(defaultValue ?? resourceKey);

        var value = localizationService.GetResource(resourceKey, languageId, defaultValue);
        return new HtmlString(value);
    }

    /// <summary>
    /// Mevcut dil kodunu al
    /// </summary>
    public static string CurrentLanguageCode(this IHtmlHelper htmlHelper)
    {
        var localizationService = htmlHelper.ViewContext.HttpContext.RequestServices
            .GetService<ILocalizationService>();

        return localizationService?.GetCurrentLanguageCode() ?? "tr";
    }

    /// <summary>
    /// Mevcut dil ID'sini al
    /// </summary>
    public static int CurrentLanguageId(this IHtmlHelper htmlHelper)
    {
        var localizationService = htmlHelper.ViewContext.HttpContext.RequestServices
            .GetService<ILocalizationService>();

        return localizationService?.GetCurrentLanguageId() ?? 0;
    }
}
