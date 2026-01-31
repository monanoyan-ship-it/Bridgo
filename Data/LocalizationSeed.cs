using System.Xml.Linq;
using Bridgo.Models.Entities;
using Microsoft.EntityFrameworkCore;

namespace Bridgo.Data;

public static class LocalizationSeed
{
    public static async Task SeedLocalizationAsync(ApplicationDbContext context, string? basePath = null)
    {
        // Seed all languages
        await SeedLanguagesAsync(context);

        // Seed localization resources for active languages
        var turkishLang = await context.Languages.FirstOrDefaultAsync(l => l.LanguageCulture == "tr-TR");
        var englishLang = await context.Languages.FirstOrDefaultAsync(l => l.LanguageCulture == "en-US");

        // Oncelikle XML dosyalarindan import et
        if (!string.IsNullOrEmpty(basePath))
        {
            if (turkishLang != null)
            {
                var trXmlPath = Path.Combine(basePath, "App_Data", "Localization", "resources.tr.xml");
                await ImportFromXmlFileAsync(context, turkishLang.Id, trXmlPath);
            }

            if (englishLang != null)
            {
                var enXmlPath = Path.Combine(basePath, "App_Data", "Localization", "resources.en.xml");
                await ImportFromXmlFileAsync(context, englishLang.Id, enXmlPath);
            }
        }

        // Fallback: Dictionary'den eksik olanlari ekle
        if (turkishLang != null)
        {
            var turkishResources = GetTurkishResources();
            await SeedResourcesAsync(context, turkishLang.Id, turkishResources);
        }

        if (englishLang != null)
        {
            var englishResources = GetEnglishResources();
            await SeedResourcesAsync(context, englishLang.Id, englishResources);
        }
    }

    private static async Task ImportFromXmlFileAsync(ApplicationDbContext context, int languageId, string filePath)
    {
        if (!File.Exists(filePath))
        {
            Console.WriteLine($"XML dosyasi bulunamadi: {filePath}");
            return;
        }

        try
        {
            var xmlContent = await File.ReadAllTextAsync(filePath);
            var doc = XDocument.Parse(xmlContent);
            var resources = doc.Descendants("LocaleResource");

            var existingKeys = await context.LocaleStringResources
                .Where(r => r.LanguageId == languageId)
                .Select(r => r.ResourceName)
                .ToListAsync();

            var newResources = new List<LocaleStringResource>();
            int updatedCount = 0;

            foreach (var resource in resources)
            {
                var name = resource.Attribute("Name")?.Value;
                var value = resource.Element("Value")?.Value;

                if (string.IsNullOrEmpty(name) || value == null)
                    continue;

                if (!existingKeys.Contains(name))
                {
                    newResources.Add(new LocaleStringResource
                    {
                        LanguageId = languageId,
                        ResourceName = name,
                        ResourceValue = value
                    });
                }
                else
                {
                    // Mevcut kaydi guncelle (XML'deki deger daha guncel olabilir)
                    var existing = await context.LocaleStringResources
                        .FirstOrDefaultAsync(r => r.LanguageId == languageId && r.ResourceName == name);
                    if (existing != null && existing.ResourceValue != value)
                    {
                        existing.ResourceValue = value;
                        updatedCount++;
                    }
                }
            }

            if (newResources.Any())
            {
                context.LocaleStringResources.AddRange(newResources);
                await context.SaveChangesAsync();
                Console.WriteLine($"[Localization] {filePath}: {newResources.Count} yeni kaynak eklendi, {updatedCount} guncellendi");
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"XML import hatasi ({filePath}): {ex.Message}");
        }
    }

    private static async Task SeedLanguagesAsync(ApplicationDbContext context)
    {
        var existingCultures = await context.Languages
            .Select(l => l.LanguageCulture)
            .ToListAsync();

        var allLanguages = GetAllLanguages();
        var newLanguages = allLanguages
            .Where(l => !existingCultures.Contains(l.LanguageCulture))
            .ToList();

        // Tek tek ekle - hata veren dilleri atla
        foreach (var language in newLanguages)
        {
            try
            {
                context.Languages.Add(language);
                await context.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                // Hata veren dili atla, devam et
                context.Entry(language).State = Microsoft.EntityFrameworkCore.EntityState.Detached;
                Console.WriteLine($"Dil eklenemedi: {language.Name} - {ex.Message}");
            }
        }
    }

    private static List<Language> GetAllLanguages()
    {
        return new List<Language>
        {
            // Aktif diller
            new() { Name = "Turkish", NativeName = "Türkçe", LanguageCulture = "tr-TR", UniqueSeoCode = "tr", Iso3Code = "tur", FlagEmoji = "🇹🇷", IsDefault = true, IsActive = true, DisplayOrder = 1 },
            new() { Name = "English", NativeName = "English", LanguageCulture = "en-US", UniqueSeoCode = "en", Iso3Code = "eng", FlagEmoji = "🇺🇸", IsDefault = false, IsActive = true, DisplayOrder = 2 },

            // Bati Avrupa
            new() { Name = "German", NativeName = "Deutsch", LanguageCulture = "de-DE", UniqueSeoCode = "de", Iso3Code = "deu", FlagEmoji = "🇩🇪", IsActive = false, DisplayOrder = 10 },
            new() { Name = "French", NativeName = "Français", LanguageCulture = "fr-FR", UniqueSeoCode = "fr", Iso3Code = "fra", FlagEmoji = "🇫🇷", IsActive = false, DisplayOrder = 11 },
            new() { Name = "Spanish", NativeName = "Español", LanguageCulture = "es-ES", UniqueSeoCode = "es", Iso3Code = "spa", FlagEmoji = "🇪🇸", IsActive = false, DisplayOrder = 12 },
            new() { Name = "Italian", NativeName = "Italiano", LanguageCulture = "it-IT", UniqueSeoCode = "it", Iso3Code = "ita", FlagEmoji = "🇮🇹", IsActive = false, DisplayOrder = 13 },
            new() { Name = "Portuguese", NativeName = "Português", LanguageCulture = "pt-PT", UniqueSeoCode = "pt", Iso3Code = "por", FlagEmoji = "🇵🇹", IsActive = false, DisplayOrder = 14 },
            new() { Name = "Dutch", NativeName = "Nederlands", LanguageCulture = "nl-NL", UniqueSeoCode = "nl", Iso3Code = "nld", FlagEmoji = "🇳🇱", IsActive = false, DisplayOrder = 15 },
            new() { Name = "Polish", NativeName = "Polski", LanguageCulture = "pl-PL", UniqueSeoCode = "pl", Iso3Code = "pol", FlagEmoji = "🇵🇱", IsActive = false, DisplayOrder = 16 },
            new() { Name = "Romanian", NativeName = "Română", LanguageCulture = "ro-RO", UniqueSeoCode = "ro", Iso3Code = "ron", FlagEmoji = "🇷🇴", IsActive = false, DisplayOrder = 17 },
            new() { Name = "Czech", NativeName = "Čeština", LanguageCulture = "cs-CZ", UniqueSeoCode = "cs", Iso3Code = "ces", FlagEmoji = "🇨🇿", IsActive = false, DisplayOrder = 18 },
            new() { Name = "Hungarian", NativeName = "Magyar", LanguageCulture = "hu-HU", UniqueSeoCode = "hu", Iso3Code = "hun", FlagEmoji = "🇭🇺", IsActive = false, DisplayOrder = 19 },

            // Balkanlar ve Guney Avrupa
            new() { Name = "Greek", NativeName = "Ελληνικά", LanguageCulture = "el-GR", UniqueSeoCode = "el", Iso3Code = "ell", FlagEmoji = "🇬🇷", IsActive = false, DisplayOrder = 20 },
            new() { Name = "Bulgarian", NativeName = "Български", LanguageCulture = "bg-BG", UniqueSeoCode = "bg", Iso3Code = "bul", FlagEmoji = "🇧🇬", IsActive = false, DisplayOrder = 21 },
            new() { Name = "Serbian", NativeName = "Српски", LanguageCulture = "sr-RS", UniqueSeoCode = "sr", Iso3Code = "srp", FlagEmoji = "🇷🇸", IsActive = false, DisplayOrder = 22 },
            new() { Name = "Croatian", NativeName = "Hrvatski", LanguageCulture = "hr-HR", UniqueSeoCode = "hr", Iso3Code = "hrv", FlagEmoji = "🇭🇷", IsActive = false, DisplayOrder = 23 },
            new() { Name = "Slovenian", NativeName = "Slovenščina", LanguageCulture = "sl-SI", UniqueSeoCode = "sl", Iso3Code = "slv", FlagEmoji = "🇸🇮", IsActive = false, DisplayOrder = 24 },
            new() { Name = "Bosnian", NativeName = "Bosanski", LanguageCulture = "bs-BA", UniqueSeoCode = "bs", Iso3Code = "bos", FlagEmoji = "🇧🇦", IsActive = false, DisplayOrder = 25 },
            new() { Name = "Albanian", NativeName = "Shqip", LanguageCulture = "sq-AL", UniqueSeoCode = "sq", Iso3Code = "sqi", FlagEmoji = "🇦🇱", IsActive = false, DisplayOrder = 26 },
            new() { Name = "Macedonian", NativeName = "Македонски", LanguageCulture = "mk-MK", UniqueSeoCode = "mk", Iso3Code = "mkd", FlagEmoji = "🇲🇰", IsActive = false, DisplayOrder = 27 },

            // Kuzey Avrupa
            new() { Name = "Swedish", NativeName = "Svenska", LanguageCulture = "sv-SE", UniqueSeoCode = "sv", Iso3Code = "swe", FlagEmoji = "🇸🇪", IsActive = false, DisplayOrder = 30 },
            new() { Name = "Norwegian", NativeName = "Norsk", LanguageCulture = "nb-NO", UniqueSeoCode = "no", Iso3Code = "nor", FlagEmoji = "🇳🇴", IsActive = false, DisplayOrder = 31 },
            new() { Name = "Danish", NativeName = "Dansk", LanguageCulture = "da-DK", UniqueSeoCode = "da", Iso3Code = "dan", FlagEmoji = "🇩🇰", IsActive = false, DisplayOrder = 32 },
            new() { Name = "Finnish", NativeName = "Suomi", LanguageCulture = "fi-FI", UniqueSeoCode = "fi", Iso3Code = "fin", FlagEmoji = "🇫🇮", IsActive = false, DisplayOrder = 33 },
            new() { Name = "Estonian", NativeName = "Eesti", LanguageCulture = "et-EE", UniqueSeoCode = "et", Iso3Code = "est", FlagEmoji = "🇪🇪", IsActive = false, DisplayOrder = 34 },
            new() { Name = "Latvian", NativeName = "Latviešu", LanguageCulture = "lv-LV", UniqueSeoCode = "lv", Iso3Code = "lav", FlagEmoji = "🇱🇻", IsActive = false, DisplayOrder = 35 },
            new() { Name = "Lithuanian", NativeName = "Lietuvių", LanguageCulture = "lt-LT", UniqueSeoCode = "lt", Iso3Code = "lit", FlagEmoji = "🇱🇹", IsActive = false, DisplayOrder = 36 },

            // Orta Dogu
            new() { Name = "Arabic", NativeName = "العربية", LanguageCulture = "ar-SA", UniqueSeoCode = "ar", Iso3Code = "ara", FlagEmoji = "🇸🇦", Rtl = true, IsActive = false, DisplayOrder = 40 },
            new() { Name = "Hebrew", NativeName = "עברית", LanguageCulture = "he-IL", UniqueSeoCode = "he", Iso3Code = "heb", FlagEmoji = "🇮🇱", Rtl = true, IsActive = false, DisplayOrder = 41 },
            new() { Name = "Persian", NativeName = "فارسی", LanguageCulture = "fa-IR", UniqueSeoCode = "fa", Iso3Code = "fas", FlagEmoji = "🇮🇷", Rtl = true, IsActive = false, DisplayOrder = 42 },
            new() { Name = "Kurdish", NativeName = "Kurdî", LanguageCulture = "ku-TR", UniqueSeoCode = "ku", Iso3Code = "kur", FlagEmoji = "🏳️", IsActive = false, DisplayOrder = 43 },

            // Asya
            new() { Name = "Chinese Simplified", NativeName = "简体中文", LanguageCulture = "zh-CN", UniqueSeoCode = "zh", Iso3Code = "zho", FlagEmoji = "🇨🇳", IsActive = false, DisplayOrder = 50 },
            new() { Name = "Chinese Traditional", NativeName = "繁體中文", LanguageCulture = "zh-TW", UniqueSeoCode = "zh-tw", Iso3Code = "zho", FlagEmoji = "🇹🇼", IsActive = false, DisplayOrder = 51 },
            new() { Name = "Japanese", NativeName = "日本語", LanguageCulture = "ja-JP", UniqueSeoCode = "ja", Iso3Code = "jpn", FlagEmoji = "🇯🇵", IsActive = false, DisplayOrder = 52 },
            new() { Name = "Korean", NativeName = "한국어", LanguageCulture = "ko-KR", UniqueSeoCode = "ko", Iso3Code = "kor", FlagEmoji = "🇰🇷", IsActive = false, DisplayOrder = 53 },
            new() { Name = "Vietnamese", NativeName = "Tiếng Việt", LanguageCulture = "vi-VN", UniqueSeoCode = "vi", Iso3Code = "vie", FlagEmoji = "🇻🇳", IsActive = false, DisplayOrder = 54 },
            new() { Name = "Thai", NativeName = "ไทย", LanguageCulture = "th-TH", UniqueSeoCode = "th", Iso3Code = "tha", FlagEmoji = "🇹🇭", IsActive = false, DisplayOrder = 55 },
            new() { Name = "Indonesian", NativeName = "Bahasa Indonesia", LanguageCulture = "id-ID", UniqueSeoCode = "id", Iso3Code = "ind", FlagEmoji = "🇮🇩", IsActive = false, DisplayOrder = 56 },
            new() { Name = "Malay", NativeName = "Bahasa Melayu", LanguageCulture = "ms-MY", UniqueSeoCode = "ms", Iso3Code = "msa", FlagEmoji = "🇲🇾", IsActive = false, DisplayOrder = 57 },
            new() { Name = "Hindi", NativeName = "हिन्दी", LanguageCulture = "hi-IN", UniqueSeoCode = "hi", Iso3Code = "hin", FlagEmoji = "🇮🇳", IsActive = false, DisplayOrder = 58 },
            new() { Name = "Bengali", NativeName = "বাংলা", LanguageCulture = "bn-BD", UniqueSeoCode = "bn", Iso3Code = "ben", FlagEmoji = "🇧🇩", IsActive = false, DisplayOrder = 59 },
            new() { Name = "Urdu", NativeName = "اردو", LanguageCulture = "ur-PK", UniqueSeoCode = "ur", Iso3Code = "urd", FlagEmoji = "🇵🇰", Rtl = true, IsActive = false, DisplayOrder = 60 },

            // Eski Sovyet
            new() { Name = "Russian", NativeName = "Русский", LanguageCulture = "ru-RU", UniqueSeoCode = "ru", Iso3Code = "rus", FlagEmoji = "🇷🇺", IsActive = false, DisplayOrder = 61 },
            new() { Name = "Ukrainian", NativeName = "Українська", LanguageCulture = "uk-UA", UniqueSeoCode = "uk", Iso3Code = "ukr", FlagEmoji = "🇺🇦", IsActive = false, DisplayOrder = 62 },
            new() { Name = "Kazakh", NativeName = "Қазақша", LanguageCulture = "kk-KZ", UniqueSeoCode = "kk", Iso3Code = "kaz", FlagEmoji = "🇰🇿", IsActive = false, DisplayOrder = 63 },
            new() { Name = "Uzbek", NativeName = "Oʻzbekcha", LanguageCulture = "uz-UZ", UniqueSeoCode = "uz", Iso3Code = "uzb", FlagEmoji = "🇺🇿", IsActive = false, DisplayOrder = 64 },
            new() { Name = "Azerbaijani", NativeName = "Azərbaycanca", LanguageCulture = "az-AZ", UniqueSeoCode = "az", Iso3Code = "aze", FlagEmoji = "🇦🇿", IsActive = false, DisplayOrder = 65 },
            new() { Name = "Georgian", NativeName = "ქართული", LanguageCulture = "ka-GE", UniqueSeoCode = "ka", Iso3Code = "kat", FlagEmoji = "🇬🇪", IsActive = false, DisplayOrder = 66 },
            new() { Name = "Armenian", NativeName = "Hayeren", LanguageCulture = "hy-AM", UniqueSeoCode = "hy", Iso3Code = "hye", FlagEmoji = "🇦🇲", IsActive = false, DisplayOrder = 67 },
            new() { Name = "Mongolian", NativeName = "Монгол", LanguageCulture = "mn-MN", UniqueSeoCode = "mn", Iso3Code = "mon", FlagEmoji = "🇲🇳", IsActive = false, DisplayOrder = 68 },

            // Afrika
            new() { Name = "Swahili", NativeName = "Kiswahili", LanguageCulture = "sw-KE", UniqueSeoCode = "sw", Iso3Code = "swa", FlagEmoji = "🇰🇪", IsActive = false, DisplayOrder = 70 },
            new() { Name = "Afrikaans", NativeName = "Afrikaans", LanguageCulture = "af-ZA", UniqueSeoCode = "af", Iso3Code = "afr", FlagEmoji = "🇿🇦", IsActive = false, DisplayOrder = 71 },

            // Diger
            new() { Name = "Slovak", NativeName = "Slovenčina", LanguageCulture = "sk-SK", UniqueSeoCode = "sk", Iso3Code = "slk", FlagEmoji = "🇸🇰", IsActive = false, DisplayOrder = 80 },
            new() { Name = "Catalan", NativeName = "Català", LanguageCulture = "ca-ES", UniqueSeoCode = "ca", Iso3Code = "cat", FlagEmoji = "🏴󠁥󠁳󠁣󠁴󠁿", IsActive = false, DisplayOrder = 81 },
            new() { Name = "Basque", NativeName = "Euskara", LanguageCulture = "eu-ES", UniqueSeoCode = "eu", Iso3Code = "eus", FlagEmoji = "🏴󠁥󠁳󠁰󠁶󠁿", IsActive = false, DisplayOrder = 82 },
            new() { Name = "Galician", NativeName = "Galego", LanguageCulture = "gl-ES", UniqueSeoCode = "gl", Iso3Code = "glg", FlagEmoji = "🏴󠁥󠁳󠁧󠁡󠁿", IsActive = false, DisplayOrder = 83 },
            new() { Name = "Irish", NativeName = "Gaeilge", LanguageCulture = "ga-IE", UniqueSeoCode = "ga", Iso3Code = "gle", FlagEmoji = "🇮🇪", IsActive = false, DisplayOrder = 84 },
            new() { Name = "Welsh", NativeName = "Cymraeg", LanguageCulture = "cy-GB", UniqueSeoCode = "cy", Iso3Code = "cym", FlagEmoji = "🏴󠁧󠁢󠁷󠁬󠁳󠁿", IsActive = false, DisplayOrder = 85 },
            new() { Name = "Icelandic", NativeName = "Íslenska", LanguageCulture = "is-IS", UniqueSeoCode = "is", Iso3Code = "isl", FlagEmoji = "🇮🇸", IsActive = false, DisplayOrder = 86 },
            new() { Name = "Maltese", NativeName = "Malti", LanguageCulture = "mt-MT", UniqueSeoCode = "mt", Iso3Code = "mlt", FlagEmoji = "🇲🇹", IsActive = false, DisplayOrder = 87 },
            new() { Name = "Filipino", NativeName = "Filipino", LanguageCulture = "fil-PH", UniqueSeoCode = "fil", Iso3Code = "fil", FlagEmoji = "🇵🇭", IsActive = false, DisplayOrder = 88 },
        };
    }

    private static async Task SeedResourcesAsync(ApplicationDbContext context, int languageId, Dictionary<string, string> resources)
    {
        var existingKeys = await context.LocaleStringResources
            .Where(r => r.LanguageId == languageId)
            .Select(r => r.ResourceName)
            .ToListAsync();

        var newResources = resources
            .Where(r => !existingKeys.Contains(r.Key))
            .Select(r => new LocaleStringResource
            {
                LanguageId = languageId,
                ResourceName = r.Key,
                ResourceValue = r.Value
            })
            .ToList();

        if (newResources.Any())
        {
            context.LocaleStringResources.AddRange(newResources);
            await context.SaveChangesAsync();
        }
    }

    private static Dictionary<string, string> GetTurkishResources()
    {
        return new Dictionary<string, string>
        {
            // ========================================
            // COMMON
            // ========================================
            ["Common.AppName"] = "Bridgo B2B Platform",
            ["Common.Save"] = "Kaydet",
            ["Common.Cancel"] = "Iptal",
            ["Common.Delete"] = "Sil",
            ["Common.Edit"] = "Duzenle",
            ["Common.Add"] = "Ekle",
            ["Common.Search"] = "Ara",
            ["Common.Filter"] = "Filtrele",
            ["Common.Close"] = "Kapat",
            ["Common.Confirm"] = "Onayla",
            ["Common.Yes"] = "Evet",
            ["Common.No"] = "Hayir",
            ["Common.Back"] = "Geri",
            ["Common.Next"] = "Devam",
            ["Common.Loading"] = "Yukleniyor...",
            ["Common.NoData"] = "Veri bulunamadi",
            ["Common.Error"] = "Hata",
            ["Common.Success"] = "Basarili",
            ["Common.Warning"] = "Uyari",
            ["Common.Required"] = "Zorunlu alan",
            ["Common.Actions"] = "Islemler",
            ["Common.Status"] = "Durum",
            ["Common.Active"] = "Aktif",
            ["Common.Inactive"] = "Pasif",
            ["Common.All"] = "Tumu",
            ["Common.SelectAll"] = "Tumunu Sec",
            ["Common.Clear"] = "Temizle",
            ["Common.View"] = "Goruntule",
            ["Common.Update"] = "Guncelle",
            ["Common.Saving"] = "Kaydediliyor...",
            ["Common.Select"] = "Secin...",

            // ========================================
            // NAVIGATION
            // ========================================
            ["Nav.Home"] = "Ana Sayfa",
            ["Nav.Dashboard"] = "Dashboard",
            ["Nav.AdminPanel"] = "Admin Panel",
            ["Nav.Profile"] = "Profil",
            ["Nav.Logout"] = "Cikis Yap",
            ["Nav.Login"] = "Giris Yap",
            ["Nav.Register"] = "Uye Ol",

            // ========================================
            // ACCOUNT / LOGIN
            // ========================================
            ["Account.Login.Title"] = "Giris Yap",
            ["Account.Login.Submit"] = "Giris Yap",
            ["Account.Login.RememberMe"] = "Beni hatirla",
            ["Account.Login.NoAccount"] = "Hesabiniz yok mu?",
            ["Account.Login.InvalidAttempt"] = "Gecersiz giris denemesi.",
            ["Account.Login.LockedOut"] = "Hesabiniz gecici olarak kilitlendi. Lutfen daha sonra tekrar deneyin.",

            ["Account.Fields.Email"] = "E-posta",
            ["Account.Fields.Email.Placeholder"] = "ornek@email.com",
            ["Account.Fields.Password"] = "Sifre",
            ["Account.Fields.Password.Placeholder"] = "En az 6 karakter",
            ["Account.Fields.ConfirmPassword"] = "Sifre Tekrar",

            // ========================================
            // ACCOUNT / PROFILE
            // ========================================
            ["Account.Profile.Title"] = "Profil",
            ["Account.Profile.Info"] = "Profil Bilgileri",
            ["Account.Profile.Name"] = "Ad Soyad",
            ["Account.Profile.Phone"] = "Telefon",
            ["Account.Profile.LastLogin"] = "Son Giris",
            ["Account.Profile.ChangePassword"] = "Sifre Degistir",
            ["Account.Profile.CurrentPassword"] = "Mevcut Sifre",
            ["Account.Profile.NewPassword"] = "Yeni Sifre",
            ["Account.Profile.ConfirmPassword"] = "Yeni Sifre Tekrar",
            ["Account.Profile.PasswordHint"] = "En az 6 karakter",
            ["Account.Profile.ChangePasswordBtn"] = "Sifreyi Degistir",
            ["Account.Profile.FillAllFields"] = "Lutfen tum alanlari doldurun",
            ["Account.Profile.PasswordMismatch"] = "Yeni sifreler eslesmedi",
            ["Account.Profile.PasswordTooShort"] = "Sifre en az 6 karakter olmali",

            // ========================================
            // ACCOUNT / REGISTER
            // ========================================
            ["Account.Register.Title"] = "Kayit Ol",
            ["Account.Register.Step1"] = "Hesap",
            ["Account.Register.Step2"] = "Firma",
            ["Account.Register.Step3"] = "Adres",
            ["Account.Register.Step4"] = "Kullanim",
            ["Account.Register.Step5"] = "Ulkeler",

            ["Account.Register.Step1.Title"] = "Hesap Bilgileri",
            ["Account.Register.Step1.Description"] = "Hesap bilgilerinizi girin",
            ["Account.Register.Step2.Title"] = "Firma Bilgileri",
            ["Account.Register.Step2.Description"] = "Firma bilgilerinizi girin",
            ["Account.Register.Step3.Title"] = "Adres Bilgileri",
            ["Account.Register.Step3.Description"] = "Merkez adresinizi girin",
            ["Account.Register.Step4.Title"] = "Kullanim Amaci",
            ["Account.Register.Step4.Description"] = "Bridgo'i nasil kullanacaksiniz? (Birden fazla secebilirsiniz)",
            ["Account.Register.Step5.Title"] = "Aktif Ulkeler",
            ["Account.Register.Step5.Description"] = "Hangi ulkelerde aktif olacaksiniz?",

            ["Account.Register.Fields.FirstName"] = "Ad",
            ["Account.Register.Fields.LastName"] = "Soyad",
            ["Account.Register.Fields.CompanyName"] = "Firma Adi",
            ["Account.Register.Fields.TaxNumber"] = "Vergi Numarasi",
            ["Account.Register.Fields.TaxOffice"] = "Vergi Dairesi",
            ["Account.Register.Fields.Phone"] = "Telefon",
            ["Account.Register.Fields.Website"] = "Website",
            ["Account.Register.Fields.City"] = "Sehir",
            ["Account.Register.Fields.District"] = "Ilce",
            ["Account.Register.Fields.Neighborhood"] = "Mahalle",
            ["Account.Register.Fields.Address"] = "Adres",
            ["Account.Register.Fields.PostalCode"] = "Posta Kodu",

            ["Account.Register.Submit"] = "Kayit Ol",
            ["Account.Register.Submitting"] = "Kaydediliyor...",
            ["Account.Register.HasAccount"] = "Zaten uyemiz misiniz?",
            ["Account.Register.EmailHint"] = "Kurumsal e-posta kullanmaniz tavsiye edilir",

            // Capabilities
            ["Account.Register.Capability.Buyer"] = "Alici",
            ["Account.Register.Capability.Buyer.Desc"] = "Urun ve hizmet satin almak istiyorum",
            ["Account.Register.Capability.Seller"] = "Satici",
            ["Account.Register.Capability.Seller.Desc"] = "Urun ve hizmet satmak istiyorum",
            ["Account.Register.Capability.ServiceProvider"] = "Hizmet Saglayici",
            ["Account.Register.Capability.ServiceProvider.Desc"] = "Hangi hizmetleri sagliyorsunuz?",
            ["Account.Register.Capability.Carrier"] = "Tasimaci / Lojistik",
            ["Account.Register.Capability.Insurance"] = "Sigorta",
            ["Account.Register.Capability.Customs"] = "Gumruk Musavirligi",

            // Validation
            ["Account.Register.Validation.FirstNameRequired"] = "Ad gereklidir",
            ["Account.Register.Validation.LastNameRequired"] = "Soyad gereklidir",
            ["Account.Register.Validation.EmailRequired"] = "E-posta gereklidir",
            ["Account.Register.Validation.EmailInvalid"] = "Gecerli bir e-posta adresi giriniz",
            ["Account.Register.Validation.EmailExists"] = "Bu e-posta adresi zaten kayitli.",
            ["Account.Register.Validation.PasswordRequired"] = "Sifre gereklidir",
            ["Account.Register.Validation.PasswordMinLength"] = "Sifre en az 6 karakter olmalidir",
            ["Account.Register.Validation.PasswordMismatch"] = "Sifreler eslesmedi",
            ["Account.Register.Validation.CompanyNameRequired"] = "Firma adi gereklidir",
            ["Account.Register.Validation.CityRequired"] = "Sehir gereklidir",
            ["Account.Register.Validation.DistrictRequired"] = "Ilce gereklidir",
            ["Account.Register.Validation.AddressRequired"] = "Adres gereklidir",
            ["Account.Register.Validation.CapabilityRequired"] = "En az bir kullanim amaci secmelisiniz",
            ["Account.Register.Validation.CountryRequired"] = "En az bir ulke secmelisiniz",

            // ========================================
            // HOME PAGE
            // ========================================
            ["Home.Title"] = "Ana Sayfa",

            ["Home.Banner1.Title"] = "B2B Ticaretin Yeni Adresi",
            ["Home.Banner1.Description"] = "Bridgo ile tedarikci ve alicilarinizi tek platformda bulusturun",
            ["Home.Banner1.Button"] = "Firsatlari Incele",

            ["Home.Banner2.Title"] = "Yeni Sezon Urunleri",
            ["Home.Banner2.Description"] = "En uygun fiyatlarla toptan alisveris firsati",
            ["Home.Banner2.Button"] = "Urunleri Gor",

            ["Home.Banner3.Title"] = "Lojistik Cozumleri",
            ["Home.Banner3.Description"] = "Turkiye geneli hizli ve guvenli teslimat",
            ["Home.Banner3.Button"] = "Detayli Bilgi",

            ["Home.Stats.RegisteredCompanies"] = "Kayitli Firma",
            ["Home.Stats.CompletedOrders"] = "Tamamlanan Siparis",
            ["Home.Stats.CitiesServed"] = "Il'de Hizmet",

            ["Home.Features.Title"] = "Neden Bridgo?",
            ["Home.Features.Subtitle"] = "B2B ticaretin tum ihtiyaclari tek platformda",

            ["Home.Features.SecureTrade.Title"] = "Guvenli Ticaret",
            ["Home.Features.SecureTrade.Description"] = "Dogrulanmis firmalar, guvenli odeme altyapisi ve 7/24 destek",

            ["Home.Features.EfficientManagement.Title"] = "Verimli Yonetim",
            ["Home.Features.EfficientManagement.Description"] = "Siparis takibi, stok yonetimi ve raporlama araclari",

            ["Home.Features.WideNetwork.Title"] = "Genis Ag",
            ["Home.Features.WideNetwork.Description"] = "Binlerce tedarikci ve alici ile baglanti kurun",

            ["Home.CTA.Title"] = "Hemen Baslayin",
            ["Home.CTA.Description"] = "Ucretsiz kayit olun ve B2B ticaretin avantajlarindan yararlanin",

            // ========================================
            // ADMIN
            // ========================================
            ["Admin.Title"] = "Admin Panel",
            ["Admin.Section.Rbac"] = "RBAC",
            ["Admin.Section.System"] = "Sistem",
            ["Admin.BackToSite"] = "Siteye Don",
            ["Admin.Menu.Dashboard"] = "Genel Bakis",
            ["Admin.Menu.Capabilities"] = "Yetenekler",
            ["Admin.Menu.Modules"] = "Moduller",
            ["Admin.Menu.Roles"] = "Roller",
            ["Admin.Menu.Vendors"] = "Firmalar",
            ["Admin.Menu.Users"] = "Kullanicilar",
            ["Admin.Menu.Geography"] = "Cografi Veriler",
            ["Admin.Menu.Languages"] = "Diller",
            ["Admin.Menu.Localization"] = "Ceviriler",

            // ADMIN TABLE
            ["Admin.Table.Order"] = "Sira",
            ["Admin.Table.Name"] = "Ad",
            ["Admin.Table.Code"] = "Kod",
            ["Admin.Table.Description"] = "Aciklama",
            ["Admin.Table.Icon"] = "Icon",
            ["Admin.Table.Status"] = "Durum",
            ["Admin.Table.Actions"] = "Islemler",
            ["Admin.Table.VendorCount"] = "Firma Sayisi",
            ["Admin.Table.Capability"] = "Yetenek",
            ["Admin.Table.FullName"] = "Ad Soyad",
            ["Admin.Table.Email"] = "E-posta",
            ["Admin.Table.Company"] = "Firma",
            ["Admin.Table.VendorRole"] = "Firma Rolu",
            ["Admin.Table.SystemRole"] = "Sistem Rolu",
            ["Admin.Table.LastLogin"] = "Son Giris",
            ["Admin.Table.FirstName"] = "Ad",
            ["Admin.Table.LastName"] = "Soyad",
            ["Admin.Table.Phone"] = "Telefon",
            ["Admin.Table.Password"] = "Sifre",

            // ADMIN USERS
            ["Admin.Users.Title"] = "Kullanici Yonetimi",
            ["Admin.Users.AllUsers"] = "Tum Kullanicilar",
            ["Admin.Users.AllRoles"] = "Tum Roller",
            ["Admin.Users.Add"] = "Kullanici Ekle",
            ["Admin.Users.Empty"] = "Henuz kullanici kaydi bulunmuyor",
            ["Admin.Users.Activate"] = "Aktif Yap",
            ["Admin.Users.Deactivate"] = "Pasif Yap",
            ["Admin.Users.NoCompany"] = "Firma Yok (Sistem Kullanicisi)",
            ["Admin.Users.IsSystemAdmin"] = "Sistem Yoneticisi",
            ["Admin.Users.PasswordHint"] = "En az 6 karakter, buyuk/kucuk harf ve rakam icermeli",

            // ADMIN VENDORS
            ["Admin.Vendors.Title"] = "Firma Yonetimi",
            ["Admin.Vendors.AllVendors"] = "Tum Firmalar",
            ["Admin.Vendors.AllStatuses"] = "Tum Durumlar",
            ["Admin.Vendors.Empty"] = "Henuz firma kaydi bulunmuyor",
            ["Admin.Vendors.Approve"] = "Onayla",
            ["Admin.Vendors.Suspend"] = "Askiya Al",
            ["Admin.Vendors.Reactivate"] = "Aktif Et",

            // ADMIN LANGUAGES
            ["Admin.Languages.Title"] = "Dil Yonetimi",
            ["Admin.Languages.AllLanguages"] = "Tum Diller",
            ["Admin.Languages.AddNew"] = "Yeni Dil",
            ["Admin.Languages.Edit"] = "Dil Duzenle",
            ["Admin.Languages.Language"] = "Dil",
            ["Admin.Languages.Name"] = "Dil Adi",
            ["Admin.Languages.NativeName"] = "Yerel Adi",
            ["Admin.Languages.Culture"] = "Kultur Kodu",
            ["Admin.Languages.SeoCode"] = "SEO Kodu",
            ["Admin.Languages.Iso3Code"] = "ISO3 Kodu",
            ["Admin.Languages.FlagEmoji"] = "Bayrak Emoji",
            ["Admin.Languages.FlagImage"] = "Bayrak Dosyasi",
            ["Admin.Languages.Rtl"] = "Sagdan Sola (RTL)",
            ["Admin.Languages.GeoTranslation"] = "Cografi Ceviriler",
            ["Admin.Languages.Default"] = "Varsayilan",
            ["Admin.Languages.SetDefault"] = "Varsayilan Yap",
            ["Admin.Languages.Resources"] = "Kaynaklar",
            ["Admin.Languages.ResourceCount"] = "Kaynak",
            ["Admin.Languages.Empty"] = "Henuz dil tanimlanmamis",

            // ADMIN LOCALIZATION
            ["Admin.Localization.Title"] = "Ceviri Yonetimi",
            ["Admin.Localization.Resources"] = "Ceviri Kaynaklari",
            ["Admin.Localization.AddNew"] = "Yeni Kaynak",
            ["Admin.Localization.Key"] = "Anahtar",
            ["Admin.Localization.Value"] = "Deger",
            ["Admin.Localization.Export"] = "Disa Aktar",
            ["Admin.Localization.Import"] = "Ice Aktar",
            ["Admin.Localization.ImportFromFile"] = "Dosyadan Yukle",
            ["Admin.Localization.TotalResources"] = "kaynak",
            ["Admin.Localization.Empty"] = "Bu dil icin henuz kaynak tanimlanmamis",
            ["Admin.Localization.NoResults"] = "Arama kriterine uygun kaynak bulunamadi",

            // ========================================
            // VENDOR ROLES
            // ========================================
            ["VendorRole.Owner"] = "Sahip",
            ["VendorRole.Admin"] = "Admin",
            ["VendorRole.Manager"] = "Yonetici",
            ["VendorRole.Employee"] = "Calisan",

            // ========================================
            // DASHBOARD
            // ========================================
            ["Dashboard.Title"] = "Dashboard",
            ["Dashboard.Overview"] = "Genel Bakis",
            ["Dashboard.Company"] = "Firma",
            ["Dashboard.ReturnHome"] = "Ana Sayfaya Don",
            ["Dashboard.Logout"] = "Cikis",
            ["Dashboard.Menu"] = "Menu",

            ["Dashboard.Section.Dashboard"] = "Dashboard",
            ["Dashboard.Section.Actions"] = "Islemler",
            ["Dashboard.Section.Account"] = "Hesap",
            ["Dashboard.Section.Apps"] = "Uygulamalar",
            ["Dashboard.Section.Settings"] = "Ayarlar",
            ["Dashboard.Section.ServiceProviders"] = "Hizmet Saglayicilar",

            // ========================================
            // COMPANY
            // ========================================
            ["Company.Profile"] = "Firma Profili",
            ["Company.Addresses"] = "Adresler",
            ["Company.Team"] = "Ekip Uyeleri",
            ["Company.Branches"] = "Subeler",
            ["Company.Settings"] = "Firma Ayarlari",

            // ========================================
            // BRANCHES
            // ========================================
            ["Branches.Title"] = "Subeler",
            ["Branches.Add"] = "Yeni Sube",
            ["Branches.Edit"] = "Sube Duzenle",
            ["Branches.Code"] = "Kod",
            ["Branches.Name"] = "Sube Adi",
            ["Branches.Phone"] = "Telefon",
            ["Branches.Address"] = "Adres",
            ["Branches.Status"] = "Durum",
            ["Branches.SaveSuccess"] = "Sube kaydedildi",
            ["Branches.DeleteConfirm"] = "Bu subeyi silmek istediginize emin misiniz?",
            ["Branches.DeleteSuccess"] = "Sube silindi",
            ["Branches.CodeExists"] = "Bu kod zaten kullaniliyor",
            ["Branches.NotFound"] = "Sube bulunamadi",

            // ========================================
            // TEAM
            // ========================================
            ["Team.Title"] = "Ekip Uyeleri",
            ["Team.Invite"] = "Davet Gonder",
            ["Team.Role"] = "Rol",
            ["Team.Status"] = "Durum",
            ["Team.JoinedAt"] = "Katilma Tarihi",
            ["Team.InvitedAt"] = "Davet Tarihi",

            ["Team.Role.Owner"] = "Sahip",
            ["Team.Role.Admin"] = "Yonetici",
            ["Team.Role.Supervisor"] = "Supervisor",
            ["Team.Role.Manager"] = "Mudur",
            ["Team.Role.Employee"] = "Calisan",

            ["Team.Status.Pending"] = "Bekliyor",
            ["Team.Status.Active"] = "Aktif",
            ["Team.Status.Inactive"] = "Pasif",
            ["Team.Status.Rejected"] = "Reddedildi",

            ["Team.Source.Invite"] = "Davet",
            ["Team.Source.Request"] = "Talep",
            ["Team.Source.DomainMatch"] = "Domain Eslesmesi",
            ["Team.Source.OwnerCreated"] = "Kurucu",

            ["Team.Invite.EmailRequired"] = "E-posta zorunludur",
            ["Team.Invite.Success"] = "Davet gonderildi",
            ["Team.Invite.Failed"] = "Davet gonderilemedi",

            // ========================================
            // VENDOR SETUP
            // ========================================
            ["VendorSetup.Title"] = "Firma Secimi",
            ["VendorSetup.CreateNew"] = "Yeni Firma Olustur",
            ["VendorSetup.JoinExisting"] = "Mevcut Firmaya Katil",
            ["VendorSetup.CompanyName"] = "Firma Adi",
            ["VendorSetup.CompanyCode"] = "Firma Kodu",
            ["VendorSetup.Message"] = "Mesaj (Opsiyonel)",
            ["VendorSetup.CreateSuccess"] = "Firma olusturuldu",
            ["VendorSetup.JoinSuccess"] = "Katilma isteginiz gonderildi. Firma yetkililerinin onayini bekleyin.",
            ["VendorSetup.JoinFailed"] = "Istek gonderilemedi",

            // ========================================
            // ADDRESS TYPES
            // ========================================
            ["Address.Type.Billing"] = "Fatura Adresi",
            ["Address.Type.Shipping"] = "Teslimat Adresi",
            ["Address.Type.Headquarters"] = "Merkez",
            ["Address.Type.Warehouse"] = "Depo",
            ["Address.Type.Branch"] = "Sube",
            ["Address.Type.Return"] = "Iade Adresi",

            // ========================================
            // MESSAGES
            // ========================================
            ["Message.SaveSuccess"] = "Kaydedildi",
            ["Message.SaveFailed"] = "Kaydetme basarisiz",
            ["Message.DeleteSuccess"] = "Silindi",
            ["Message.DeleteFailed"] = "Silme basarisiz",
            ["Message.UpdateSuccess"] = "Guncellendi",
            ["Message.UpdateFailed"] = "Guncelleme basarisiz",
            ["Message.LoadFailed"] = "Veriler yuklenemedi",
            ["Message.GenericError"] = "Bir hata olustu. Lutfen tekrar deneyin.",
            ["Message.FillRequiredFields"] = "Lutfen zorunlu alanlari doldurun",
            ["Message.InvalidData"] = "Gecersiz veri",
            ["Message.Unauthorized"] = "Bu islemi yapmaya yetkiniz yok",
            ["Message.NotFound"] = "Kayit bulunamadi",

            // ========================================
            // ACCESS DENIED
            // ========================================
            ["AccessDenied.Title"] = "Erisim Engellendi",
            ["AccessDenied.Message"] = "Bu sayfaya erismek icin yetkiniz bulunmamaktadir.",
            ["AccessDenied.ReturnHome"] = "Ana Sayfaya Don",

            // ========================================
            // CAPABILITIES
            // ========================================
            ["Capability.Platform"] = "Platform",
            ["Capability.Seller"] = "Satici",
            ["Capability.Buyer"] = "Alici",
            ["Capability.Carrier"] = "Tasimaci",
            ["Capability.Insurance"] = "Sigorta",
            ["Capability.Customs"] = "Gumruk",

            // ========================================
            // MODULES (Menu Items)
            // ========================================
            ["Module.Dashboard"] = "Genel Bakis",
            ["Module.ProposalManagement"] = "Teklif Yönetimi",
            ["Module.Auctions"] = "Acik Artirmalar",
            ["Module.Orders"] = "Siparisler",
            ["Module.DownloadableProducts"] = "Indirilebilir Urunler",
            ["Module.BackInStock"] = "Stok Bildirimleri",
            ["Module.RewardPoints"] = "Odul Puanlari",
            ["Module.Messages"] = "Mesajlar",
            ["Module.ProductReviews"] = "Urun Degerlendirmelerim",
            ["Module.PersonalInfo"] = "Kisisel Bilgiler",
            ["Module.CompanyInfo"] = "Firma Bilgileri",
            ["Module.MyDocuments"] = "Belgelerim",
            ["Module.Addresses"] = "Adresler",
            ["Module.MyContracts"] = "Sozlesmelerim",
            ["Module.Verifications"] = "Dogrulamalar",
            ["Module.BillingBank"] = "Fatura ve Banka Bilgileri",
            ["Module.StaffManagement"] = "Personel Yonetimi",
            ["Module.StripeConnect"] = "Stripe Hesap Baglantisi",
            ["Module.AppsLogistics"] = "Lojistik Uygulamalari",
            ["Module.AppsCustoms"] = "Gumruk Uygulamalari",
            ["Module.AppsFinance"] = "Finans Uygulamalari",
            ["Module.AppsSocialMedia"] = "Sosyal Medya Uygulamalari",
            ["Module.AppsMarketplace"] = "Pazar Yeri Uygulamalari",
            ["Module.Notifications"] = "Bildirimler",
            ["Module.Subscriptions"] = "Abonelikler",
            ["Module.PasswordSecurity"] = "Sifre ve Guvenlik",
            ["Module.CustomsBroker"] = "Gumruk Musavirleri",
            ["Module.LogisticsPartner"] = "Lojistik Ortaklari",
            ["Module.FinancingPartner"] = "Finansman Ortaklari",
            ["Module.Warehouses"] = "Depolar",
            ["Module.Products"] = "Urunler",
            ["Module.ProductCategories"] = "Kategoriler",
            ["Module.PriceLists"] = "Fiyat Listeleri",
            ["Module.StockManagement"] = "Stok Yonetimi",

            // ========================================
            // COMPANY ROLES
            // ========================================
            ["Role.ProposalAuctionsStaff"] = "Teklif/İhale Personeli",
            ["Role.OrderStaff"] = "Siparis Personeli",
            ["Role.AccountManager"] = "Hesap Yoneticisi"
        };
    }

    private static Dictionary<string, string> GetEnglishResources()
    {
        return new Dictionary<string, string>
        {
            // ========================================
            // COMMON
            // ========================================
            ["Common.AppName"] = "Bridgo B2B Platform",
            ["Common.Save"] = "Save",
            ["Common.Cancel"] = "Cancel",
            ["Common.Delete"] = "Delete",
            ["Common.Edit"] = "Edit",
            ["Common.Add"] = "Add",
            ["Common.Search"] = "Search",
            ["Common.Filter"] = "Filter",
            ["Common.Close"] = "Close",
            ["Common.Confirm"] = "Confirm",
            ["Common.Yes"] = "Yes",
            ["Common.No"] = "No",
            ["Common.Back"] = "Back",
            ["Common.Next"] = "Continue",
            ["Common.Loading"] = "Loading...",
            ["Common.NoData"] = "No data found",
            ["Common.Error"] = "Error",
            ["Common.Success"] = "Success",
            ["Common.Warning"] = "Warning",
            ["Common.Required"] = "Required field",
            ["Common.Actions"] = "Actions",
            ["Common.Status"] = "Status",
            ["Common.Active"] = "Active",
            ["Common.Inactive"] = "Inactive",
            ["Common.All"] = "All",
            ["Common.SelectAll"] = "Select All",
            ["Common.Clear"] = "Clear",
            ["Common.View"] = "View",
            ["Common.Update"] = "Update",
            ["Common.Saving"] = "Saving...",
            ["Common.Select"] = "Select...",

            // ========================================
            // NAVIGATION
            // ========================================
            ["Nav.Home"] = "Home",
            ["Nav.Dashboard"] = "Dashboard",
            ["Nav.AdminPanel"] = "Admin Panel",
            ["Nav.Profile"] = "Profile",
            ["Nav.Logout"] = "Logout",
            ["Nav.Login"] = "Login",
            ["Nav.Register"] = "Sign Up",

            // ========================================
            // ACCOUNT / LOGIN
            // ========================================
            ["Account.Login.Title"] = "Login",
            ["Account.Login.Submit"] = "Login",
            ["Account.Login.RememberMe"] = "Remember me",
            ["Account.Login.NoAccount"] = "Don't have an account?",
            ["Account.Login.InvalidAttempt"] = "Invalid login attempt.",
            ["Account.Login.LockedOut"] = "Your account has been temporarily locked. Please try again later.",

            ["Account.Fields.Email"] = "Email",
            ["Account.Fields.Email.Placeholder"] = "example@email.com",
            ["Account.Fields.Password"] = "Password",
            ["Account.Fields.Password.Placeholder"] = "At least 6 characters",
            ["Account.Fields.ConfirmPassword"] = "Confirm Password",

            // ========================================
            // ACCOUNT / PROFILE
            // ========================================
            ["Account.Profile.Title"] = "Profile",
            ["Account.Profile.Info"] = "Profile Information",
            ["Account.Profile.Name"] = "Full Name",
            ["Account.Profile.Phone"] = "Phone",
            ["Account.Profile.LastLogin"] = "Last Login",
            ["Account.Profile.ChangePassword"] = "Change Password",
            ["Account.Profile.CurrentPassword"] = "Current Password",
            ["Account.Profile.NewPassword"] = "New Password",
            ["Account.Profile.ConfirmPassword"] = "Confirm New Password",
            ["Account.Profile.PasswordHint"] = "At least 6 characters",
            ["Account.Profile.ChangePasswordBtn"] = "Change Password",
            ["Account.Profile.FillAllFields"] = "Please fill in all fields",
            ["Account.Profile.PasswordMismatch"] = "New passwords do not match",
            ["Account.Profile.PasswordTooShort"] = "Password must be at least 6 characters",

            // ========================================
            // ACCOUNT / REGISTER
            // ========================================
            ["Account.Register.Title"] = "Sign Up",
            ["Account.Register.Step1"] = "Account",
            ["Account.Register.Step2"] = "Company",
            ["Account.Register.Step3"] = "Address",
            ["Account.Register.Step4"] = "Usage",
            ["Account.Register.Step5"] = "Countries",

            ["Account.Register.Step1.Title"] = "Account Information",
            ["Account.Register.Step1.Description"] = "Enter your account details",
            ["Account.Register.Step2.Title"] = "Company Information",
            ["Account.Register.Step2.Description"] = "Enter your company details",
            ["Account.Register.Step3.Title"] = "Address Information",
            ["Account.Register.Step3.Description"] = "Enter your headquarters address",
            ["Account.Register.Step4.Title"] = "Usage Purpose",
            ["Account.Register.Step4.Description"] = "How will you use Bridgo? (You can select multiple)",
            ["Account.Register.Step5.Title"] = "Active Countries",
            ["Account.Register.Step5.Description"] = "In which countries will you be active?",

            ["Account.Register.Fields.FirstName"] = "First Name",
            ["Account.Register.Fields.LastName"] = "Last Name",
            ["Account.Register.Fields.CompanyName"] = "Company Name",
            ["Account.Register.Fields.TaxNumber"] = "Tax Number",
            ["Account.Register.Fields.TaxOffice"] = "Tax Office",
            ["Account.Register.Fields.Phone"] = "Phone",
            ["Account.Register.Fields.Website"] = "Website",
            ["Account.Register.Fields.City"] = "City",
            ["Account.Register.Fields.District"] = "District",
            ["Account.Register.Fields.Neighborhood"] = "Neighborhood",
            ["Account.Register.Fields.Address"] = "Address",
            ["Account.Register.Fields.PostalCode"] = "Postal Code",

            ["Account.Register.Submit"] = "Sign Up",
            ["Account.Register.Submitting"] = "Signing up...",
            ["Account.Register.HasAccount"] = "Already a member?",
            ["Account.Register.EmailHint"] = "Using a corporate email is recommended",

            // Capabilities
            ["Account.Register.Capability.Buyer"] = "Buyer",
            ["Account.Register.Capability.Buyer.Desc"] = "I want to purchase products and services",
            ["Account.Register.Capability.Seller"] = "Seller",
            ["Account.Register.Capability.Seller.Desc"] = "I want to sell products and services",
            ["Account.Register.Capability.ServiceProvider"] = "Service Provider",
            ["Account.Register.Capability.ServiceProvider.Desc"] = "Which services do you provide?",
            ["Account.Register.Capability.Carrier"] = "Carrier / Logistics",
            ["Account.Register.Capability.Insurance"] = "Insurance",
            ["Account.Register.Capability.Customs"] = "Customs Brokerage",

            // Validation
            ["Account.Register.Validation.FirstNameRequired"] = "First name is required",
            ["Account.Register.Validation.LastNameRequired"] = "Last name is required",
            ["Account.Register.Validation.EmailRequired"] = "Email is required",
            ["Account.Register.Validation.EmailInvalid"] = "Please enter a valid email address",
            ["Account.Register.Validation.EmailExists"] = "This email address is already registered.",
            ["Account.Register.Validation.PasswordRequired"] = "Password is required",
            ["Account.Register.Validation.PasswordMinLength"] = "Password must be at least 6 characters",
            ["Account.Register.Validation.PasswordMismatch"] = "Passwords do not match",
            ["Account.Register.Validation.CompanyNameRequired"] = "Company name is required",
            ["Account.Register.Validation.CityRequired"] = "City is required",
            ["Account.Register.Validation.DistrictRequired"] = "District is required",
            ["Account.Register.Validation.AddressRequired"] = "Address is required",
            ["Account.Register.Validation.CapabilityRequired"] = "Please select at least one usage purpose",
            ["Account.Register.Validation.CountryRequired"] = "Please select at least one country",

            // ========================================
            // HOME PAGE
            // ========================================
            ["Home.Title"] = "Home",

            ["Home.Banner1.Title"] = "The New Address of B2B Trade",
            ["Home.Banner1.Description"] = "Connect your suppliers and buyers on a single platform with Bridgo",
            ["Home.Banner1.Button"] = "Explore Opportunities",

            ["Home.Banner2.Title"] = "New Season Products",
            ["Home.Banner2.Description"] = "Wholesale shopping opportunity at the best prices",
            ["Home.Banner2.Button"] = "View Products",

            ["Home.Banner3.Title"] = "Logistics Solutions",
            ["Home.Banner3.Description"] = "Fast and secure delivery throughout Turkey",
            ["Home.Banner3.Button"] = "Learn More",

            ["Home.Stats.RegisteredCompanies"] = "Registered Companies",
            ["Home.Stats.CompletedOrders"] = "Completed Orders",
            ["Home.Stats.CitiesServed"] = "Cities Served",

            ["Home.Features.Title"] = "Why Bridgo?",
            ["Home.Features.Subtitle"] = "All B2B trade needs on a single platform",

            ["Home.Features.SecureTrade.Title"] = "Secure Trade",
            ["Home.Features.SecureTrade.Description"] = "Verified companies, secure payment infrastructure and 24/7 support",

            ["Home.Features.EfficientManagement.Title"] = "Efficient Management",
            ["Home.Features.EfficientManagement.Description"] = "Order tracking, inventory management and reporting tools",

            ["Home.Features.WideNetwork.Title"] = "Wide Network",
            ["Home.Features.WideNetwork.Description"] = "Connect with thousands of suppliers and buyers",

            ["Home.CTA.Title"] = "Get Started Now",
            ["Home.CTA.Description"] = "Sign up for free and take advantage of B2B trade benefits",

            // ========================================
            // ADMIN
            // ========================================
            ["Admin.Title"] = "Admin Panel",
            ["Admin.Section.Rbac"] = "RBAC",
            ["Admin.Section.System"] = "System",
            ["Admin.BackToSite"] = "Back to Site",
            ["Admin.Menu.Dashboard"] = "Overview",
            ["Admin.Menu.Capabilities"] = "Capabilities",
            ["Admin.Menu.Modules"] = "Modules",
            ["Admin.Menu.Roles"] = "Roles",
            ["Admin.Menu.Vendors"] = "Vendors",
            ["Admin.Menu.Users"] = "Users",
            ["Admin.Menu.Geography"] = "Geography",
            ["Admin.Menu.Languages"] = "Languages",
            ["Admin.Menu.Localization"] = "Localization",

            // ADMIN TABLE
            ["Admin.Table.Order"] = "Order",
            ["Admin.Table.Name"] = "Name",
            ["Admin.Table.Code"] = "Code",
            ["Admin.Table.Description"] = "Description",
            ["Admin.Table.Icon"] = "Icon",
            ["Admin.Table.Status"] = "Status",
            ["Admin.Table.Actions"] = "Actions",
            ["Admin.Table.VendorCount"] = "Vendor Count",
            ["Admin.Table.Capability"] = "Capability",
            ["Admin.Table.FullName"] = "Full Name",
            ["Admin.Table.Email"] = "Email",
            ["Admin.Table.Company"] = "Company",
            ["Admin.Table.VendorRole"] = "Vendor Role",
            ["Admin.Table.SystemRole"] = "System Role",
            ["Admin.Table.LastLogin"] = "Last Login",
            ["Admin.Table.FirstName"] = "First Name",
            ["Admin.Table.LastName"] = "Last Name",
            ["Admin.Table.Phone"] = "Phone",
            ["Admin.Table.Password"] = "Password",

            // ADMIN USERS
            ["Admin.Users.Title"] = "User Management",
            ["Admin.Users.AllUsers"] = "All Users",
            ["Admin.Users.AllRoles"] = "All Roles",
            ["Admin.Users.Add"] = "Add User",
            ["Admin.Users.Empty"] = "No users found",
            ["Admin.Users.Activate"] = "Activate",
            ["Admin.Users.Deactivate"] = "Deactivate",
            ["Admin.Users.NoCompany"] = "No Company (System User)",
            ["Admin.Users.IsSystemAdmin"] = "System Administrator",
            ["Admin.Users.PasswordHint"] = "At least 6 characters, uppercase/lowercase letters and numbers",

            // ADMIN VENDORS
            ["Admin.Vendors.Title"] = "Vendor Management",
            ["Admin.Vendors.AllVendors"] = "All Vendors",
            ["Admin.Vendors.AllStatuses"] = "All Statuses",
            ["Admin.Vendors.Empty"] = "No vendors found",
            ["Admin.Vendors.Approve"] = "Approve",
            ["Admin.Vendors.Suspend"] = "Suspend",
            ["Admin.Vendors.Reactivate"] = "Reactivate",

            // ADMIN LANGUAGES
            ["Admin.Languages.Title"] = "Language Management",
            ["Admin.Languages.AllLanguages"] = "All Languages",
            ["Admin.Languages.AddNew"] = "New Language",
            ["Admin.Languages.Edit"] = "Edit Language",
            ["Admin.Languages.Language"] = "Language",
            ["Admin.Languages.Name"] = "Language Name",
            ["Admin.Languages.NativeName"] = "Native Name",
            ["Admin.Languages.Culture"] = "Culture Code",
            ["Admin.Languages.SeoCode"] = "SEO Code",
            ["Admin.Languages.Iso3Code"] = "ISO3 Code",
            ["Admin.Languages.FlagEmoji"] = "Flag Emoji",
            ["Admin.Languages.FlagImage"] = "Flag File",
            ["Admin.Languages.Rtl"] = "Right to Left (RTL)",
            ["Admin.Languages.GeoTranslation"] = "Geographic Translations",
            ["Admin.Languages.Default"] = "Default",
            ["Admin.Languages.SetDefault"] = "Set as Default",
            ["Admin.Languages.Resources"] = "Resources",
            ["Admin.Languages.ResourceCount"] = "Resources",
            ["Admin.Languages.Empty"] = "No languages defined yet",

            // ADMIN LOCALIZATION
            ["Admin.Localization.Title"] = "Localization Management",
            ["Admin.Localization.Resources"] = "Localization Resources",
            ["Admin.Localization.AddNew"] = "New Resource",
            ["Admin.Localization.Key"] = "Key",
            ["Admin.Localization.Value"] = "Value",
            ["Admin.Localization.Export"] = "Export",
            ["Admin.Localization.Import"] = "Import",
            ["Admin.Localization.ImportFromFile"] = "Load from File",
            ["Admin.Localization.TotalResources"] = "resources",
            ["Admin.Localization.Empty"] = "No resources defined for this language",
            ["Admin.Localization.NoResults"] = "No resources found matching search criteria",

            // ========================================
            // VENDOR ROLES
            // ========================================
            ["VendorRole.Owner"] = "Owner",
            ["VendorRole.Admin"] = "Admin",
            ["VendorRole.Manager"] = "Manager",
            ["VendorRole.Employee"] = "Employee",

            // ========================================
            // DASHBOARD
            // ========================================
            ["Dashboard.Title"] = "Dashboard",
            ["Dashboard.Overview"] = "Overview",
            ["Dashboard.Company"] = "Company",
            ["Dashboard.ReturnHome"] = "Return to Home",
            ["Dashboard.Logout"] = "Logout",
            ["Dashboard.Menu"] = "Menu",

            ["Dashboard.Section.Dashboard"] = "Dashboard",
            ["Dashboard.Section.Actions"] = "Actions",
            ["Dashboard.Section.Account"] = "Account",
            ["Dashboard.Section.Apps"] = "Apps",
            ["Dashboard.Section.Settings"] = "Settings",
            ["Dashboard.Section.ServiceProviders"] = "Service Providers",

            // ========================================
            // COMPANY
            // ========================================
            ["Company.Profile"] = "Company Profile",
            ["Company.Addresses"] = "Addresses",
            ["Company.Team"] = "Team Members",
            ["Company.Branches"] = "Branches",
            ["Company.Settings"] = "Company Settings",

            // ========================================
            // BRANCHES
            // ========================================
            ["Branches.Title"] = "Branches",
            ["Branches.Add"] = "New Branch",
            ["Branches.Edit"] = "Edit Branch",
            ["Branches.Code"] = "Code",
            ["Branches.Name"] = "Branch Name",
            ["Branches.Phone"] = "Phone",
            ["Branches.Address"] = "Address",
            ["Branches.Status"] = "Status",
            ["Branches.SaveSuccess"] = "Branch saved",
            ["Branches.DeleteConfirm"] = "Are you sure you want to delete this branch?",
            ["Branches.DeleteSuccess"] = "Branch deleted",
            ["Branches.CodeExists"] = "This code is already in use",
            ["Branches.NotFound"] = "Branch not found",

            // ========================================
            // TEAM
            // ========================================
            ["Team.Title"] = "Team Members",
            ["Team.Invite"] = "Send Invitation",
            ["Team.Role"] = "Role",
            ["Team.Status"] = "Status",
            ["Team.JoinedAt"] = "Joined At",
            ["Team.InvitedAt"] = "Invited At",

            ["Team.Role.Owner"] = "Owner",
            ["Team.Role.Admin"] = "Admin",
            ["Team.Role.Supervisor"] = "Supervisor",
            ["Team.Role.Manager"] = "Manager",
            ["Team.Role.Employee"] = "Employee",

            ["Team.Status.Pending"] = "Pending",
            ["Team.Status.Active"] = "Active",
            ["Team.Status.Inactive"] = "Inactive",
            ["Team.Status.Rejected"] = "Rejected",

            ["Team.Source.Invite"] = "Invitation",
            ["Team.Source.Request"] = "Request",
            ["Team.Source.DomainMatch"] = "Domain Match",
            ["Team.Source.OwnerCreated"] = "Founder",

            ["Team.Invite.EmailRequired"] = "Email is required",
            ["Team.Invite.Success"] = "Invitation sent",
            ["Team.Invite.Failed"] = "Failed to send invitation",

            // ========================================
            // VENDOR SETUP
            // ========================================
            ["VendorSetup.Title"] = "Company Selection",
            ["VendorSetup.CreateNew"] = "Create New Company",
            ["VendorSetup.JoinExisting"] = "Join Existing Company",
            ["VendorSetup.CompanyName"] = "Company Name",
            ["VendorSetup.CompanyCode"] = "Company Code",
            ["VendorSetup.Message"] = "Message (Optional)",
            ["VendorSetup.CreateSuccess"] = "Company created",
            ["VendorSetup.JoinSuccess"] = "Your join request has been sent. Please wait for approval from company administrators.",
            ["VendorSetup.JoinFailed"] = "Failed to send request",

            // ========================================
            // ADDRESS TYPES
            // ========================================
            ["Address.Type.Billing"] = "Billing Address",
            ["Address.Type.Shipping"] = "Shipping Address",
            ["Address.Type.Headquarters"] = "Headquarters",
            ["Address.Type.Warehouse"] = "Warehouse",
            ["Address.Type.Branch"] = "Branch",
            ["Address.Type.Return"] = "Return Address",

            // ========================================
            // MESSAGES
            // ========================================
            ["Message.SaveSuccess"] = "Saved",
            ["Message.SaveFailed"] = "Save failed",
            ["Message.DeleteSuccess"] = "Deleted",
            ["Message.DeleteFailed"] = "Delete failed",
            ["Message.UpdateSuccess"] = "Updated",
            ["Message.UpdateFailed"] = "Update failed",
            ["Message.LoadFailed"] = "Failed to load data",
            ["Message.GenericError"] = "An error occurred. Please try again.",
            ["Message.FillRequiredFields"] = "Please fill in the required fields",
            ["Message.InvalidData"] = "Invalid data",
            ["Message.Unauthorized"] = "You are not authorized to perform this action",
            ["Message.NotFound"] = "Record not found",

            // ========================================
            // ACCESS DENIED
            // ========================================
            ["AccessDenied.Title"] = "Access Denied",
            ["AccessDenied.Message"] = "You do not have permission to access this page.",
            ["AccessDenied.ReturnHome"] = "Return to Home",

            // ========================================
            // CAPABILITIES
            // ========================================
            ["Capability.Platform"] = "Platform",
            ["Capability.Seller"] = "Seller",
            ["Capability.Buyer"] = "Buyer",
            ["Capability.Carrier"] = "Carrier",
            ["Capability.Insurance"] = "Insurance",
            ["Capability.Customs"] = "Customs",

            // ========================================
            // MODULES (Menu Items)
            // ========================================
            ["Module.Dashboard"] = "Overview",
            ["Module.ProposalManagement"] = "Proposal Management",
            ["Module.Auctions"] = "Auctions",
            ["Module.Orders"] = "Orders",
            ["Module.DownloadableProducts"] = "Downloadable Products",
            ["Module.BackInStock"] = "Back in Stock",
            ["Module.RewardPoints"] = "Reward Points",
            ["Module.Messages"] = "Messages",
            ["Module.ProductReviews"] = "My Product Reviews",
            ["Module.PersonalInfo"] = "Personal Info",
            ["Module.CompanyInfo"] = "Company Info",
            ["Module.MyDocuments"] = "My Documents",
            ["Module.Addresses"] = "Addresses",
            ["Module.MyContracts"] = "My Contracts",
            ["Module.Verifications"] = "Verifications",
            ["Module.BillingBank"] = "Billing & Bank Details",
            ["Module.StaffManagement"] = "Staff Management",
            ["Module.StripeConnect"] = "Stripe Account Connect",
            ["Module.AppsLogistics"] = "Logistics Apps",
            ["Module.AppsCustoms"] = "Customs Apps",
            ["Module.AppsFinance"] = "Finance Apps",
            ["Module.AppsSocialMedia"] = "Social Media Apps",
            ["Module.AppsMarketplace"] = "Marketplace Apps",
            ["Module.Notifications"] = "Notifications",
            ["Module.Subscriptions"] = "Subscriptions",
            ["Module.PasswordSecurity"] = "Password & Security",
            ["Module.CustomsBroker"] = "Customs Broker",
            ["Module.LogisticsPartner"] = "Logistics Partner",
            ["Module.FinancingPartner"] = "Financing Partner",
            ["Module.Warehouses"] = "Warehouses",
            ["Module.Products"] = "Products",
            ["Module.ProductCategories"] = "Categories",
            ["Module.PriceLists"] = "Price Lists",
            ["Module.StockManagement"] = "Stock Management",

            // ========================================
            // COMPANY ROLES
            // ========================================
            ["Role.ProposalAuctionsStaff"] = "Proposal/Auctions Staff",
            ["Role.OrderStaff"] = "Order Staff",
            ["Role.AccountManager"] = "Account Manager"
        };
    }
}
