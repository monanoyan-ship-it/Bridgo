using Bridgo.Models.Entities;
using Bridgo.Models.Enums;
using Microsoft.EntityFrameworkCore;

namespace Bridgo.Data;

/// <summary>
/// RBAC verilerini seed eder
/// Platform modulleri ve roller
/// NOT: Capabilities artik TypeDefinitions'da static olarak tanimli, seed edilmiyor
/// </summary>
public static class RbacSeeder
{
    public static async Task SeedAsync(ApplicationDbContext context)
    {
        // Moduller yoksa ekle
        if (!await context.PlatformModules.AnyAsync())
        {
            await SeedModulesAsync(context);
        }

        // Capability-Modul mapping'leri Admin Panel'den yapilir
        // Platform butonuna tiklandiginda tum PlatformModules gosterilir
        // Diger capability'lere modul atamak icin Admin Panel kullanilir

        // Roller yoksa ekle
        if (!await context.CompanyRoles.AnyAsync())
        {
            await SeedRolesAsync(context);
        }

        // Varsayilan rol izinleri
        if (!await context.CompanyRoleModulePermissions.AnyAsync())
        {
            await SeedRolePermissionsAsync(context);
        }
    }

    private static async Task SeedModulesAsync(ApplicationDbContext context)
    {
        // ========== SECTION MODULLERI (IsMenuSection = true) ==========
        var sections = new List<PlatformModule>
        {
            new()
            {
                Name = "Actions",
                DisplayName = "Islemler",
                DisplayNameResourceKey = "Dashboard.Section.Actions",
                Icon = "bi-lightning",
                Route = null, // Section olduğu için route yok
                DisplayOrder = 10,
                IsMenuItem = true,
                IsActive = true,
                IsMenuSection = true
            },
            new()
            {
                Name = "Account",
                DisplayName = "Hesap",
                DisplayNameResourceKey = "Dashboard.Section.Account",
                Icon = "bi-person-circle",
                Route = null,
                DisplayOrder = 20,
                IsMenuItem = true,
                IsActive = true,
                IsMenuSection = true
            },
            new()
            {
                Name = "Apps",
                DisplayName = "Uygulamalar",
                DisplayNameResourceKey = "Dashboard.Section.Apps",
                Icon = "bi-grid-3x3-gap",
                Route = null,
                DisplayOrder = 30,
                IsMenuItem = true,
                IsActive = true,
                IsMenuSection = true
            },
            new()
            {
                Name = "Settings",
                DisplayName = "Ayarlar",
                DisplayNameResourceKey = "Dashboard.Section.Settings",
                Icon = "bi-gear",
                Route = null,
                DisplayOrder = 40,
                IsMenuItem = true,
                IsActive = true,
                IsMenuSection = true
            },
            new()
            {
                Name = "Service Providers",
                DisplayName = "Servis Saglayicilar",
                DisplayNameResourceKey = "Dashboard.Section.ServiceProviders",
                Icon = "bi-building",
                Route = null,
                DisplayOrder = 50,
                IsMenuItem = true,
                IsActive = true,
                IsMenuSection = true
            },
            new()
            {
                Name = "Catalog",
                DisplayName = "Katalog",
                DisplayNameResourceKey = "Dashboard.Section.Catalog",
                Icon = "bi-box-seam",
                Route = null,
                DisplayOrder = 5, // Actions'dan once
                IsMenuItem = true,
                IsActive = true,
                IsMenuSection = true
            }
        };

        context.PlatformModules.AddRange(sections);
        await context.SaveChangesAsync();

        // Section ID'lerini al (DisplayNameResourceKey ile)
        var actionsSection = await context.PlatformModules.FirstAsync(m => m.DisplayNameResourceKey == "Dashboard.Section.Actions");
        var accountSection = await context.PlatformModules.FirstAsync(m => m.DisplayNameResourceKey == "Dashboard.Section.Account");
        var appsSection = await context.PlatformModules.FirstAsync(m => m.DisplayNameResourceKey == "Dashboard.Section.Apps");
        var settingsSection = await context.PlatformModules.FirstAsync(m => m.DisplayNameResourceKey == "Dashboard.Section.Settings");
        var serviceProvidersSection = await context.PlatformModules.FirstAsync(m => m.DisplayNameResourceKey == "Dashboard.Section.ServiceProviders");
        var catalogSection = await context.PlatformModules.FirstAsync(m => m.DisplayNameResourceKey == "Dashboard.Section.Catalog");

        // ========== MODUL TANIMLARI ==========
        var modules = new List<PlatformModule>
        {
            // Dashboard (root, section değil)
            new()
            {
                ParentId = null,
                Name = "Dashboard",
                DisplayName = "Genel Bakis",
                DisplayNameResourceKey = "Module.Dashboard",
                Description = "Ana panel",
                Icon = "bi-speedometer2",
                Route = "/Dashboard",
                DisplayOrder = 1,
                IsMenuItem = true,
                IsActive = true,
                IsMenuSection = false
            },

            // Capability-bazli Dashboard'lar
            new()
            {
                ParentId = null,
                Name = "SellerDashboard",
                DisplayName = "Satici Paneli",
                DisplayNameResourceKey = "Module.SellerDashboard",
                Description = "Satici dashboard",
                Icon = "bi-shop",
                Route = "/Dashboard?cap=seller",
                DisplayOrder = 2,
                IsMenuItem = false,
                IsActive = true,
                IsMenuSection = false
            },
            new()
            {
                ParentId = null,
                Name = "BuyerDashboard",
                DisplayName = "Alici Paneli",
                DisplayNameResourceKey = "Module.BuyerDashboard",
                Description = "Alici dashboard",
                Icon = "bi-cart",
                Route = "/Dashboard?cap=buyer",
                DisplayOrder = 3,
                IsMenuItem = false,
                IsActive = true,
                IsMenuSection = false
            },
            new()
            {
                ParentId = null,
                Name = "CarrierDashboard",
                DisplayName = "Tasimaci Paneli",
                DisplayNameResourceKey = "Module.CarrierDashboard",
                Description = "Tasimaci dashboard",
                Icon = "bi-truck",
                Route = "/Dashboard?cap=carrier",
                DisplayOrder = 4,
                IsMenuItem = false,
                IsActive = true,
                IsMenuSection = false
            },
            new()
            {
                ParentId = null,
                Name = "InsuranceDashboard",
                DisplayName = "Sigorta Paneli",
                DisplayNameResourceKey = "Module.InsuranceDashboard",
                Description = "Sigorta dashboard",
                Icon = "bi-shield-check",
                Route = "/Dashboard?cap=insurance",
                DisplayOrder = 5,
                IsMenuItem = false,
                IsActive = true,
                IsMenuSection = false
            },
            new()
            {
                ParentId = null,
                Name = "CustomsDashboard",
                DisplayName = "Gumruk Paneli",
                DisplayNameResourceKey = "Module.CustomsDashboard",
                Description = "Gumruk dashboard",
                Icon = "bi-flag",
                Route = "/Dashboard?cap=customs",
                DisplayOrder = 6,
                IsMenuItem = false,
                IsActive = true,
                IsMenuSection = false
            },
            new()
            {
                ParentId = null,
                Name = "SurveyDashboard",
                DisplayName = "Gozetim Paneli",
                DisplayNameResourceKey = "Module.SurveyDashboard",
                Description = "Gozetim dashboard",
                Icon = "bi-clipboard-check",
                Route = "/Dashboard?cap=survey",
                DisplayOrder = 7,
                IsMenuItem = false,
                IsActive = true,
                IsMenuSection = false
            },
            new()
            {
                ParentId = null,
                Name = "InvestorDashboard",
                DisplayName = "Yatirimci Paneli",
                DisplayNameResourceKey = "Module.InvestorDashboard",
                Description = "Yatirimci dashboard",
                Icon = "bi-graph-up-arrow",
                Route = "/Dashboard?cap=investor",
                DisplayOrder = 8,
                IsMenuItem = false,
                IsActive = true,
                IsMenuSection = false
            },

            // ========== FEED (Platform-level, tum capability'ler) ==========
            new()
            {
                ParentId = null,
                Name = "Feed",
                DisplayName = "Feed",
                DisplayNameResourceKey = "Module.Feed",
                Description = "Sosyal paylasim akisi",
                Icon = "bi-rss",
                Route = "/Feed",
                DisplayOrder = 2,
                IsMenuItem = true,
                IsActive = true,
                IsMenuSection = false
            },

            // ========== ACTIONS ==========
            new()
            {
                ParentId = actionsSection.Id,
                Name = "Proposals",
                DisplayName = "Teklif Yönetimi",
                DisplayNameResourceKey = "Module.ProposalManagement",
                Description = "Teklif yönetimi",
                Icon = "bi-file-earmark-text",
                Route = "/Proposals",
                DisplayOrder = 1,
                IsMenuItem = true,
                IsActive = true,
                IsMenuSection = false
            },
            new()
            {
                ParentId = actionsSection.Id,
                Name = "Auctions",
                DisplayName = "Acik Artirmalar",
                DisplayNameResourceKey = "Module.Auctions",
                Description = "Acik artirmalar",
                Icon = "bi-hammer",
                Route = "/Auctions",
                DisplayOrder = 3,
                IsMenuItem = true,
                IsActive = true,
                IsMenuSection = false
            },
            new()
            {
                ParentId = actionsSection.Id,
                Name = "Orders",
                DisplayName = "Siparisler",
                DisplayNameResourceKey = "Module.Orders",
                Description = "Siparisler",
                Icon = "bi-bag",
                Route = "/Dashboard/Orders",
                DisplayOrder = 4,
                IsMenuItem = true,
                IsActive = true,
                IsMenuSection = false
            },
            new()
            {
                ParentId = actionsSection.Id,
                Name = "Downloadable Products",
                DisplayName = "Indirilebilir Urunler",
                DisplayNameResourceKey = "Module.DownloadableProducts",
                Description = "Indirilebilir urunler",
                Icon = "bi-download",
                Route = "/Dashboard/DownloadableProducts",
                DisplayOrder = 5,
                IsMenuItem = true,
                IsActive = true,
                IsMenuSection = false
            },
            new()
            {
                ParentId = actionsSection.Id,
                Name = "Back in Stock",
                DisplayName = "Stok Bildirimleri",
                DisplayNameResourceKey = "Module.BackInStock",
                Description = "Stok bildirimleri",
                Icon = "bi-bell",
                Route = "/Dashboard/BackInStock",
                DisplayOrder = 6,
                IsMenuItem = true,
                IsActive = true,
                IsMenuSection = false
            },
            new()
            {
                ParentId = actionsSection.Id,
                Name = "Reward Points",
                DisplayName = "Odul Puanlari",
                DisplayNameResourceKey = "Module.RewardPoints",
                Description = "Odul puanlari",
                Icon = "bi-star",
                Route = "/Dashboard/RewardPoints",
                DisplayOrder = 7,
                IsMenuItem = true,
                IsActive = true,
                IsMenuSection = false
            },
            new()
            {
                ParentId = actionsSection.Id,
                Name = "Messages",
                DisplayName = "Mesajlar",
                DisplayNameResourceKey = "Module.Messages",
                Description = "Mesajlar",
                Icon = "bi-chat-dots",
                Route = "/Messages",
                DisplayOrder = 8,
                IsMenuItem = true,
                IsActive = true,
                IsMenuSection = false
            },
            new()
            {
                ParentId = actionsSection.Id,
                Name = "My Product Reviews",
                DisplayName = "Urun Degerlendirmelerim",
                DisplayNameResourceKey = "Module.ProductReviews",
                Description = "Urun degerlendirmelerim",
                Icon = "bi-chat-square-text",
                Route = "/Dashboard/ProductReviews",
                DisplayOrder = 9,
                IsMenuItem = true,
                IsActive = true,
                IsMenuSection = false
            },
            new()
            {
                ParentId = actionsSection.Id,
                Name = "My Demands",
                DisplayName = "Taleplerim",
                DisplayNameResourceKey = "Module.MyDemands",
                Description = "Talep olusturma ve teklif takibi",
                Icon = "bi-megaphone",
                Route = "/Demands/MyDemands",
                DisplayOrder = 10,
                IsMenuItem = true,
                IsActive = true,
                IsMenuSection = false
            },

            // ========== CATALOG ==========
            new()
            {
                ParentId = catalogSection.Id,
                Name = "Products",
                DisplayName = "Urunler",
                DisplayNameResourceKey = "Module.Products",
                Description = "Urun katalogu yonetimi",
                Icon = "bi-box-seam",
                Route = "/Products",
                DisplayOrder = 1,
                IsMenuItem = true,
                IsActive = true,
                IsMenuSection = false
            },
            new()
            {
                ParentId = catalogSection.Id,
                Name = "Categories",
                DisplayName = "Kategoriler",
                DisplayNameResourceKey = "Module.ProductCategories",
                Description = "Urun kategorileri yonetimi",
                Icon = "bi-folder",
                Route = "/Products/Categories",
                DisplayOrder = 2,
                IsMenuItem = true,
                IsActive = true,
                IsMenuSection = false
            },
            new()
            {
                ParentId = catalogSection.Id,
                Name = "Stock Management",
                DisplayName = "Stok Yonetimi",
                DisplayNameResourceKey = "Module.StockManagement",
                Description = "Stok yonetimi",
                Icon = "bi-boxes",
                Route = "/Stock/Movements",
                DisplayOrder = 4,
                IsMenuItem = true,
                IsActive = true,
                IsMenuSection = false
            },
            new()
            {
                ParentId = catalogSection.Id,
                Name = "Warehouses",
                DisplayName = "Depolar",
                DisplayNameResourceKey = "Module.Warehouses",
                Description = "Depo yonetimi",
                Icon = "bi-box-seam",
                Route = "/Warehouses",
                DisplayOrder = 5,
                IsMenuItem = true,
                IsActive = true,
                IsMenuSection = false
            },
            new()
            {
                ParentId = catalogSection.Id,
                Name = "Category Requests",
                DisplayName = "Kategori Talepleri",
                DisplayNameResourceKey = "Module.CategoryRequests",
                Description = "Kategori talepleri olusturma ve takip",
                Icon = "bi-folder-plus",
                Route = "/Products/Categories",
                DisplayOrder = 6,
                IsMenuItem = true,
                IsActive = true,
                IsMenuSection = false
            },

            // ========== ACCOUNT ==========
            new()
            {
                ParentId = accountSection.Id,
                Name = "Personal Info",
                DisplayName = "Kisisel Bilgiler",
                DisplayNameResourceKey = "Module.PersonalInfo",
                Description = "Kisisel bilgiler",
                Icon = "bi-person",
                Route = "/Settings/PersonalInfo",
                DisplayOrder = 1,
                IsMenuItem = true,
                IsActive = true,
                IsMenuSection = false
            },
            new()
            {
                ParentId = accountSection.Id,
                Name = "Company Info",
                DisplayName = "Firma Bilgileri",
                DisplayNameResourceKey = "Module.CompanyInfo",
                Description = "Firma bilgileri",
                Icon = "bi-building",
                Route = "/Company/Profile",
                DisplayOrder = 2,
                IsMenuItem = true,
                IsActive = true,
                IsMenuSection = false
            },
            new()
            {
                ParentId = accountSection.Id,
                Name = "My Documents",
                DisplayName = "Belgelerim",
                DisplayNameResourceKey = "Module.MyDocuments",
                Description = "Belgelerim",
                Icon = "bi-file-earmark",
                Route = "/Settings/Documents",
                DisplayOrder = 3,
                IsMenuItem = true,
                IsActive = true,
                IsMenuSection = false
            },
            new()
            {
                ParentId = accountSection.Id,
                Name = "Addresses",
                DisplayName = "Adresler",
                DisplayNameResourceKey = "Module.Addresses",
                Description = "Adresler",
                Icon = "bi-geo-alt",
                Route = "/Company/Addresses",
                DisplayOrder = 4,
                IsMenuItem = true,
                IsActive = true,
                IsMenuSection = false
            },
            new()
            {
                ParentId = accountSection.Id,
                Name = "My Contracts",
                DisplayName = "Sozlesmelerim",
                DisplayNameResourceKey = "Module.MyContracts",
                Description = "Sozlesmelerim",
                Icon = "bi-file-earmark-check",
                Route = "/Settings/Contracts",
                DisplayOrder = 5,
                IsMenuItem = true,
                IsActive = true,
                IsMenuSection = false
            },
            new()
            {
                ParentId = accountSection.Id,
                Name = "Verifications",
                DisplayName = "Dogrulamalar",
                DisplayNameResourceKey = "Module.Verifications",
                Description = "Dogrulamalar",
                Icon = "bi-patch-check",
                Route = "/Settings/Verifications",
                DisplayOrder = 6,
                IsMenuItem = true,
                IsActive = true,
                IsMenuSection = false
            },
            new()
            {
                ParentId = accountSection.Id,
                Name = "Billing & Bank Details",
                DisplayName = "Fatura ve Banka Bilgileri",
                DisplayNameResourceKey = "Module.BillingBank",
                Description = "Fatura ve banka bilgileri",
                Icon = "bi-credit-card",
                Route = "/Settings/Billing",
                DisplayOrder = 7,
                IsMenuItem = true,
                IsActive = true,
                IsMenuSection = false
            },
            new()
            {
                ParentId = accountSection.Id,
                Name = "Staff Management",
                DisplayName = "Personel Yonetimi",
                DisplayNameResourceKey = "Module.StaffManagement",
                Description = "Personel yonetimi",
                Icon = "bi-people",
                Route = "/Team",
                DisplayOrder = 8,
                IsMenuItem = true,
                IsActive = true,
                IsMenuSection = false
            },
            new()
            {
                ParentId = accountSection.Id,
                Name = "Stripe Account Connect",
                DisplayName = "Stripe Hesap Baglantisi",
                DisplayNameResourceKey = "Module.StripeConnect",
                Description = "Stripe hesap baglantisi",
                Icon = "bi-stripe",
                Route = "/Dashboard/StripeConnect",
                DisplayOrder = 9,
                IsMenuItem = true,
                IsActive = true,
                IsMenuSection = false
            },
            new()
            {
                ParentId = accountSection.Id,
                Name = "Supplier Profile",
                DisplayName = "Tedarikci Profili",
                DisplayNameResourceKey = "Module.SupplierProfile",
                Description = "Public olarak gorunecek tedarikci profili yonetimi",
                Icon = "bi-person-badge",
                Route = "/Suppliers/SupplierProfile",
                DisplayOrder = 10,
                IsMenuItem = true,
                IsActive = true,
                IsMenuSection = false
            },

            // ========== APPS ==========
            new()
            {
                ParentId = appsSection.Id,
                Name = "Apps.Logistics",
                DisplayName = "Lojistik",
                DisplayNameResourceKey = "Module.AppsLogistics",
                Description = "Lojistik uygulamalari",
                Icon = "bi-truck",
                Route = "/Dashboard/Apps/Logistics",
                DisplayOrder = 1,
                IsMenuItem = true,
                IsActive = true,
                IsMenuSection = false
            },
            new()
            {
                ParentId = appsSection.Id,
                Name = "Apps.Customs",
                DisplayName = "Gumruk",
                DisplayNameResourceKey = "Module.AppsCustoms",
                Description = "Gumruk uygulamalari",
                Icon = "bi-building-check",
                Route = "/Dashboard/Apps/Customs",
                DisplayOrder = 2,
                IsMenuItem = true,
                IsActive = true,
                IsMenuSection = false
            },
            new()
            {
                ParentId = appsSection.Id,
                Name = "Apps.Finance",
                DisplayName = "Finans",
                DisplayNameResourceKey = "Module.AppsFinance",
                Description = "Finans uygulamalari",
                Icon = "bi-bank",
                Route = "/Dashboard/Apps/Finance",
                DisplayOrder = 3,
                IsMenuItem = true,
                IsActive = true,
                IsMenuSection = false
            },
            new()
            {
                ParentId = appsSection.Id,
                Name = "Apps.SocialMedia",
                DisplayName = "Sosyal Medya",
                DisplayNameResourceKey = "Module.AppsSocialMedia",
                Description = "Sosyal medya uygulamalari",
                Icon = "bi-share",
                Route = "/Dashboard/Apps/SocialMedia",
                DisplayOrder = 4,
                IsMenuItem = true,
                IsActive = true,
                IsMenuSection = false
            },
            new()
            {
                ParentId = appsSection.Id,
                Name = "Apps.Marketplace",
                DisplayName = "Pazar Yeri",
                DisplayNameResourceKey = "Module.AppsMarketplace",
                Description = "Pazar yeri uygulamalari",
                Icon = "bi-shop-window",
                Route = "/Dashboard/Apps/Marketplace",
                DisplayOrder = 5,
                IsMenuItem = true,
                IsActive = true,
                IsMenuSection = false
            },

            // ========== SETTINGS ==========
            new()
            {
                ParentId = settingsSection.Id,
                Name = "Notifications",
                DisplayName = "Bildirimler",
                DisplayNameResourceKey = "Module.Notifications",
                Description = "Bildirim ayarlari",
                Icon = "bi-bell",
                Route = "/Settings/Notifications",
                DisplayOrder = 1,
                IsMenuItem = true,
                IsActive = true,
                IsMenuSection = false
            },
            new()
            {
                ParentId = settingsSection.Id,
                Name = "Subscriptions",
                DisplayName = "Abonelikler",
                DisplayNameResourceKey = "Module.Subscriptions",
                Description = "Abonelik ayarlari",
                Icon = "bi-card-checklist",
                Route = "/Settings/Subscription",
                DisplayOrder = 2,
                IsMenuItem = true,
                IsActive = true,
                IsMenuSection = false
            },
            new()
            {
                ParentId = settingsSection.Id,
                Name = "Password & Security",
                DisplayName = "Sifre ve Guvenlik",
                DisplayNameResourceKey = "Module.PasswordSecurity",
                Description = "Sifre ve guvenlik",
                Icon = "bi-shield-lock",
                Route = "/Settings/Security",
                DisplayOrder = 3,
                IsMenuItem = true,
                IsActive = true,
                IsMenuSection = false
            },
            new()
            {
                ParentId = settingsSection.Id,
                Name = "ServiceConnections",
                DisplayName = "Servis Baglantilari",
                DisplayNameResourceKey = "Module.ServiceConnections",
                Description = "Dis servis baglantilari",
                Icon = "bi-plug",
                Route = "/Settings/ServiceConnections",
                DisplayOrder = 4,
                IsMenuItem = true,
                IsActive = true,
                IsMenuSection = false
            },

            // ========== SERVICE PROVIDERS ==========
            new()
            {
                ParentId = serviceProvidersSection.Id,
                Name = "Customs Broker",
                DisplayName = "Gumruk Musavirleri",
                DisplayNameResourceKey = "Module.CustomsBroker",
                Description = "Gumruk musavirleri",
                Icon = "bi-building-check",
                Route = "/Dashboard/ServiceProviders/CustomsBroker",
                DisplayOrder = 1,
                IsMenuItem = true,
                IsActive = true,
                IsMenuSection = false
            },
            new()
            {
                ParentId = serviceProvidersSection.Id,
                Name = "Logistics Partner",
                DisplayName = "Lojistik Ortaklari",
                DisplayNameResourceKey = "Module.LogisticsPartner",
                Description = "Lojistik ortaklari",
                Icon = "bi-truck",
                Route = "/Dashboard/ServiceProviders/LogisticsPartner",
                DisplayOrder = 2,
                IsMenuItem = true,
                IsActive = true,
                IsMenuSection = false
            },
            new()
            {
                ParentId = serviceProvidersSection.Id,
                Name = "Financing Partner",
                DisplayName = "Finansman Ortaklari",
                DisplayNameResourceKey = "Module.FinancingPartner",
                Description = "Finansman ortaklari",
                Icon = "bi-cash-stack",
                Route = "/Dashboard/ServiceProviders/FinancingPartner",
                DisplayOrder = 3,
                IsMenuItem = true,
                IsActive = true,
                IsMenuSection = false
            },
            new()
            {
                ParentId = serviceProvidersSection.Id,
                Name = "Logistics Requests",
                DisplayName = "Lojistik Talepleri",
                DisplayNameResourceKey = "Module.LogisticsRequests",
                Description = "Lojistik talepleri ve teklifler",
                Icon = "bi-truck",
                Route = "/Services/Logistics",
                DisplayOrder = 11,
                IsMenuItem = true,
                IsActive = true,
                IsMenuSection = false
            },
            new()
            {
                ParentId = serviceProvidersSection.Id,
                Name = "Customs Requests",
                DisplayName = "Gumruk Talepleri",
                DisplayNameResourceKey = "Module.CustomsRequests",
                Description = "Gumruk talepleri ve teklifler",
                Icon = "bi-flag",
                Route = "/Services/Customs",
                DisplayOrder = 12,
                IsMenuItem = true,
                IsActive = true,
                IsMenuSection = false
            },
            new()
            {
                ParentId = serviceProvidersSection.Id,
                Name = "Insurance Requests",
                DisplayName = "Sigorta Talepleri",
                DisplayNameResourceKey = "Module.InsuranceRequests",
                Description = "Sigorta talepleri ve teklifler",
                Icon = "bi-shield-check",
                Route = "/Services/Insurance",
                DisplayOrder = 13,
                IsMenuItem = true,
                IsActive = true,
                IsMenuSection = false
            },
            new()
            {
                ParentId = serviceProvidersSection.Id,
                Name = "Survey Requests",
                DisplayName = "Gozetim Talepleri",
                DisplayNameResourceKey = "Module.SurveyRequests",
                Description = "Gozetim talepleri ve teklifler",
                Icon = "bi-clipboard-check",
                Route = "/Services/Survey",
                DisplayOrder = 14,
                IsMenuItem = true,
                IsActive = true,
                IsMenuSection = false
            },

            // ========== MY JOBS (Provider's accepted jobs) ==========
            new()
            {
                ParentId = serviceProvidersSection.Id,
                Name = "MyLogisticsJobs",
                DisplayName = "Lojistik Islerim",
                DisplayNameResourceKey = "Module.MyLogisticsJobs",
                Description = "Kabul edilen lojistik isleri",
                Icon = "bi-truck",
                Route = "/Services/MyLogisticsJobs",
                DisplayOrder = 21,
                IsMenuItem = true,
                IsActive = true,
                IsMenuSection = false
            },
            new()
            {
                ParentId = serviceProvidersSection.Id,
                Name = "MyCustomsJobs",
                DisplayName = "Gumruk Islerim",
                DisplayNameResourceKey = "Module.MyCustomsJobs",
                Description = "Kabul edilen gumruk isleri",
                Icon = "bi-flag",
                Route = "/Services/MyCustomsJobs",
                DisplayOrder = 22,
                IsMenuItem = true,
                IsActive = true,
                IsMenuSection = false
            },
            new()
            {
                ParentId = serviceProvidersSection.Id,
                Name = "MyInsuranceJobs",
                DisplayName = "Sigorta Islerim",
                DisplayNameResourceKey = "Module.MyInsuranceJobs",
                Description = "Kabul edilen sigorta isleri",
                Icon = "bi-shield-check",
                Route = "/Services/MyInsuranceJobs",
                DisplayOrder = 23,
                IsMenuItem = true,
                IsActive = true,
                IsMenuSection = false
            },
            new()
            {
                ParentId = serviceProvidersSection.Id,
                Name = "MySurveyJobs",
                DisplayName = "Gozetim Islerim",
                DisplayNameResourceKey = "Module.MySurveyJobs",
                Description = "Kabul edilen gozetim isleri",
                Icon = "bi-search",
                Route = "/Services/MySurveyJobs",
                DisplayOrder = 24,
                IsMenuItem = true,
                IsActive = true,
                IsMenuSection = false
            },

            // ========== FINANCING & INVESTMENT (Apps Section) ==========
            new()
            {
                ParentId = appsSection.Id,
                Name = "FinancingRequests",
                DisplayName = "Finansman Taleplerim",
                DisplayNameResourceKey = "Module.FinancingRequests",
                Description = "Finansman talepleri",
                Icon = "bi-cash-stack",
                Route = "/Financing/MyRequests",
                DisplayOrder = 10,
                IsMenuItem = true,
                IsActive = true,
                IsMenuSection = false
            },
            new()
            {
                ParentId = appsSection.Id,
                Name = "InvestmentOpportunities",
                DisplayName = "Yatirim Firsatlari",
                DisplayNameResourceKey = "Module.InvestmentOpportunities",
                Description = "Yatirim firsatlari",
                Icon = "bi-graph-up-arrow",
                Route = "/Investment/Opportunities",
                DisplayOrder = 11,
                IsMenuItem = true,
                IsActive = true,
                IsMenuSection = false
            },
            new()
            {
                ParentId = appsSection.Id,
                Name = "MyInvestments",
                DisplayName = "Yatirimlarim",
                DisplayNameResourceKey = "Module.MyInvestments",
                Description = "Kabul edilen yatirim tekliflerim",
                Icon = "bi-piggy-bank",
                Route = "/Investment/MyInvestments",
                DisplayOrder = 12,
                IsMenuItem = true,
                IsActive = true,
                IsMenuSection = false
            },

            // ========== REPORTS (Actions Section) ==========
            new()
            {
                ParentId = actionsSection.Id,
                Name = "Reports",
                DisplayName = "Raporlar",
                DisplayNameResourceKey = "Module.Reports",
                Description = "Raporlar ve analizler",
                Icon = "bi-bar-chart-line",
                Route = "/Reports",
                DisplayOrder = 15,
                IsMenuItem = true,
                IsActive = true,
                IsMenuSection = false
            },

            // ========== ORDERS (Actions Section) ==========
            new()
            {
                ParentId = actionsSection.Id,
                Name = "MyOrders",
                DisplayName = "Siparislerim",
                DisplayNameResourceKey = "Module.MyOrders",
                Description = "Buyer siparisleri",
                Icon = "bi-bag-check",
                Route = "/Orders/MyOrders",
                DisplayOrder = 4,
                IsMenuItem = true,
                IsActive = true,
                IsMenuSection = false
            },
            new()
            {
                ParentId = actionsSection.Id,
                Name = "SellerOrders",
                DisplayName = "Gelen Siparisler",
                DisplayNameResourceKey = "Module.SellerOrders",
                Description = "Seller gelen siparisleri",
                Icon = "bi-inbox",
                Route = "/Orders/SellerOrders",
                DisplayOrder = 5,
                IsMenuItem = true,
                IsActive = true,
                IsMenuSection = false
            },
            new()
            {
                ParentId = actionsSection.Id,
                Name = "SupplierOffers",
                DisplayName = "Gelen Talepler",
                DisplayNameResourceKey = "Module.SupplierOffers",
                Description = "Seller gelen talepleri",
                Icon = "bi-megaphone",
                Route = "/Demands/SupplierOffers",
                DisplayOrder = 11,
                IsMenuItem = true,
                IsActive = true,
                IsMenuSection = false
            },

            // ========== SUPPLIERS (Account Section) ==========
            new()
            {
                ParentId = accountSection.Id,
                Name = "DiscoverSuppliers",
                DisplayName = "Tedarikci Kesfet",
                DisplayNameResourceKey = "Module.DiscoverSuppliers",
                Description = "Tedarikci arama ve kesif",
                Icon = "bi-search",
                Route = "/Suppliers/DiscoverSuppliers",
                DisplayOrder = 11,
                IsMenuItem = true,
                IsActive = true,
                IsMenuSection = false
            },
            new()
            {
                ParentId = accountSection.Id,
                Name = "FavoriteSuppliers",
                DisplayName = "Favori Tedarikciler",
                DisplayNameResourceKey = "Module.FavoriteSuppliers",
                Description = "Favori tedarikci listesi",
                Icon = "bi-heart",
                Route = "/Suppliers/FavoriteSuppliers",
                DisplayOrder = 12,
                IsMenuItem = true,
                IsActive = true,
                IsMenuSection = false
            }
        };

        context.PlatformModules.AddRange(modules);
        await context.SaveChangesAsync();

        // Feed modulu tum capability'lere atansin
        var feedModule = await context.PlatformModules.FirstOrDefaultAsync(m => m.DisplayNameResourceKey == "Module.Feed");
        if (feedModule != null)
        {
            var allCapabilityIds = new[] {
                Capabilities.Ids.Seller, Capabilities.Ids.Buyer, Capabilities.Ids.Carrier,
                Capabilities.Ids.Insurance, Capabilities.Ids.Customs, Capabilities.Ids.Survey,
                Capabilities.Ids.Investor
            };
            foreach (var capId in allCapabilityIds)
            {
                context.CapabilityModuleMappings.Add(new CapabilityModuleMapping
                {
                    CapabilityId = capId,
                    PlatformModuleId = feedModule.Id
                });
            }
            await context.SaveChangesAsync();
        }

        // Auctions modulu Seller ve Buyer capability'lerine atansin
        var auctionsModule = await context.PlatformModules.FirstOrDefaultAsync(m => m.DisplayNameResourceKey == "Module.Auctions");
        if (auctionsModule != null)
        {
            var auctionCapabilityIds = new[] { Capabilities.Ids.Seller, Capabilities.Ids.Buyer };
            foreach (var capId in auctionCapabilityIds)
            {
                context.CapabilityModuleMappings.Add(new CapabilityModuleMapping
                {
                    CapabilityId = capId,
                    PlatformModuleId = auctionsModule.Id
                });
            }
            await context.SaveChangesAsync();
        }
    }

    private static async Task SeedRolesAsync(ApplicationDbContext context)
    {
        // Capabilities artik TypeDefinitions'da static olarak tanimli
        // Seller capability ID = Capabilities.Ids.Seller (2)
        var roles = new List<CompanyRole>
        {
            new()
            {
                CapabilityId = Capabilities.Ids.Seller,
                Name = "Catalog Manager",
                NameResourceKey = "Role.CatalogManager",
                Description = "Urun ve kategori yonetimi",
                IsDefault = true,
                IsActive = true
            },
            new()
            {
                CapabilityId = Capabilities.Ids.Seller,
                Name = "Stock Manager",
                NameResourceKey = "Role.StockManager",
                Description = "Stok ve fiyat yonetimi",
                IsActive = true
            }
        };

        context.CompanyRoles.AddRange(roles);
        await context.SaveChangesAsync();
    }

    private static async Task SeedRolePermissionsAsync(ApplicationDbContext context)
    {
        // Tum modulleri al
        var modules = await context.PlatformModules.ToListAsync();

        // Seller rol izinleri
        var sellerRoles = await context.CompanyRoles
            .Where(r => r.CapabilityId == Capabilities.Ids.Seller)
            .ToListAsync();

        var permissions = new List<CompanyRoleModulePermission>();

        // Catalog Manager - products, categories, warehouses, category-requests
        var catalogManager = sellerRoles.FirstOrDefault(r => r.Name == "Catalog Manager");
        if (catalogManager != null)
        {
            var catalogKeys = new[] { "Module.Products", "Module.ProductCategories", "Module.Warehouses", "Module.CategoryRequests" };
            foreach (var key in catalogKeys)
            {
                var module = modules.FirstOrDefault(m => m.DisplayNameResourceKey == key);
                if (module != null)
                {
                    permissions.Add(new CompanyRoleModulePermission
                    {
                        CompanyRoleId = catalogManager.Id,
                        PlatformModuleId = module.Id,
                        CanView = true,
                        CanCreate = true,
                        CanEdit = true,
                        CanDelete = true
                    });
                }
            }
        }

        // Stock Manager - stock-management, warehouses
        var stockManager = sellerRoles.FirstOrDefault(r => r.Name == "Stock Manager");
        if (stockManager != null)
        {
            var stockKeys = new[] { "Module.StockManagement", "Module.Warehouses" };
            foreach (var key in stockKeys)
            {
                var module = modules.FirstOrDefault(m => m.DisplayNameResourceKey == key);
                if (module != null)
                {
                    permissions.Add(new CompanyRoleModulePermission
                    {
                        CompanyRoleId = stockManager.Id,
                        PlatformModuleId = module.Id,
                        CanView = true,
                        CanCreate = true,
                        CanEdit = true,
                        CanDelete = true
                    });
                }
            }
        }

        if (permissions.Any())
        {
            context.CompanyRoleModulePermissions.AddRange(permissions);
            await context.SaveChangesAsync();
        }
    }
}
