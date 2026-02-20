using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Bridgo.Models.Identity;
using Bridgo.Models.Entities;

namespace Bridgo.Data;

public class ApplicationDbContext : IdentityDbContext<ApplicationUser, ApplicationRole, int>
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
        : base(options)
    {
    }

    public DbSet<Branch> Branches => Set<Branch>();
    public DbSet<Vendor> Vendors => Set<Vendor>();
    public DbSet<Address> Addresses => Set<Address>();
    public DbSet<VendorTeamMember> VendorTeamMembers => Set<VendorTeamMember>();
    public DbSet<Language> Languages => Set<Language>();
    public DbSet<LocaleStringResource> LocaleStringResources => Set<LocaleStringResource>();

    // RBAC - Role Based Access Control
    // NOT: VendorCapabilities tablosu kaldirildi, Capabilities artik TypeDefinitions'da static olarak tanimli
    public DbSet<VendorCapabilityMapping> VendorCapabilityMappings => Set<VendorCapabilityMapping>();
    public DbSet<PlatformModule> PlatformModules => Set<PlatformModule>();
    public DbSet<CapabilityModuleMapping> CapabilityModuleMappings => Set<CapabilityModuleMapping>();
    public DbSet<CompanyRole> CompanyRoles => Set<CompanyRole>();
    public DbSet<CompanyRoleUserMapping> CompanyRoleUserMappings => Set<CompanyRoleUserMapping>();
    public DbSet<CompanyRoleModulePermission> CompanyRoleModulePermissions => Set<CompanyRoleModulePermission>();

    // Geography - Ülke, Eyalet
    public DbSet<Country> Countries => Set<Country>();
    public DbSet<CountryTranslation> CountryTranslations => Set<CountryTranslation>();
    public DbSet<State> States => Set<State>();
    public DbSet<StateTranslation> StateTranslations => Set<StateTranslation>();

    // Logging
    public DbSet<AppLog> AppLogs => Set<AppLog>();

    // Products - Seller Module
    public DbSet<Product> Products => Set<Product>();
    public DbSet<ProductCategory> ProductCategories => Set<ProductCategory>();
    public DbSet<ProductImage> ProductImages => Set<ProductImage>();
    public DbSet<ProductPriceTier> ProductPriceTiers => Set<ProductPriceTier>();
    public DbSet<ProductAttribute> ProductAttributes => Set<ProductAttribute>();
    public DbSet<ProductAttributeValue> ProductAttributeValues => Set<ProductAttributeValue>();
    public DbSet<ProductAttributeMapping> ProductAttributeMappings => Set<ProductAttributeMapping>();
    public DbSet<CategoryRequest> CategoryRequests => Set<CategoryRequest>();

    // Warehouse - Stok Yonetimi
    public DbSet<Warehouse> Warehouses => Set<Warehouse>();
    public DbSet<ProductWarehouseStock> ProductWarehouseStocks => Set<ProductWarehouseStock>();
    public DbSet<StockMovement> StockMovements => Set<StockMovement>();
    public DbSet<ProductPackaging> ProductPackagings => Set<ProductPackaging>();

    // Demands - Talep/Arayis Sistemi
    public DbSet<PublicDemand> PublicDemands => Set<PublicDemand>();
    public DbSet<DemandResponse> DemandResponses => Set<DemandResponse>();
    public DbSet<DemandAttachment> DemandAttachments => Set<DemandAttachment>();
    public DbSet<DemandModification> DemandModifications => Set<DemandModification>();
    public DbSet<DemandResponseAttachment> DemandResponseAttachments => Set<DemandResponseAttachment>();
    public DbSet<NegotiationRound> NegotiationRounds => Set<NegotiationRound>();

    // Supplier Discovery - Uretici Kesfi
    public DbSet<SupplierProfile> SupplierProfiles => Set<SupplierProfile>();
    public DbSet<CategorySubscription> CategorySubscriptions => Set<CategorySubscription>();

    // Notifications - Bildirim Sistemi
    public DbSet<Notification> Notifications => Set<Notification>();

    // User Settings - Kullanici Ayarlari
    public DbSet<UserSettings> UserSettings => Set<UserSettings>();

    // Product Inquiries - Urun Fiyat Istekleri
    public DbSet<ProductInquiry> ProductInquiries => Set<ProductInquiry>();
    public DbSet<ProductInquiryResponse> ProductInquiryResponses => Set<ProductInquiryResponse>();

    // Cart - Sepet Sistemi
    public DbSet<Cart> Carts => Set<Cart>();
    public DbSet<CartItem> CartItems => Set<CartItem>();

    // Orders - Siparis Sistemi
    public DbSet<Order> Orders => Set<Order>();
    public DbSet<OrderItem> OrderItems => Set<OrderItem>();
    public DbSet<OrderShipment> OrderShipments => Set<OrderShipment>();
    public DbSet<OrderShipmentItem> OrderShipmentItems => Set<OrderShipmentItem>();
    public DbSet<OrderStatusHistory> OrderStatusHistory => Set<OrderStatusHistory>();
    public DbSet<StripePayment> StripePayments => Set<StripePayment>();
    public DbSet<OrderServiceRequest> OrderServiceRequests => Set<OrderServiceRequest>();
    public DbSet<OrderServiceQuote> OrderServiceQuotes => Set<OrderServiceQuote>();
    public DbSet<OrderParticipant> OrderParticipants => Set<OrderParticipant>();
    public DbSet<OrderTask> OrderTasks => Set<OrderTask>();
    public DbSet<OrderInvestment> OrderInvestments => Set<OrderInvestment>();

    // Customs Declarations - Gumruk Beyannameleri (Evrim API)
    public DbSet<CustomsDeclaration> CustomsDeclarations => Set<CustomsDeclaration>();

    // Vendor Account - Banka, Belge, Abonelik, Servis Baglantilari
    public DbSet<VendorBankAccount> VendorBankAccounts => Set<VendorBankAccount>();
    public DbSet<VendorDocument> VendorDocuments => Set<VendorDocument>();
    public DbSet<SubscriptionPlan> SubscriptionPlans => Set<SubscriptionPlan>();
    public DbSet<VendorSubscription> VendorSubscriptions => Set<VendorSubscription>();
    public DbSet<SubscriptionInvoice> SubscriptionInvoices => Set<SubscriptionInvoice>();
    public DbSet<VendorServiceConnection> VendorServiceConnections => Set<VendorServiceConnection>();

    // Buyer Features - Favori Tedarikciler, Degerlendirmeler
    public DbSet<FavoriteVendor> FavoriteVendors => Set<FavoriteVendor>();
    public DbSet<VendorReview> VendorReviews => Set<VendorReview>();
    public DbSet<ProductReview> ProductReviews => Set<ProductReview>();

    // Messaging - Mesajlasma Sistemi
    public DbSet<MessageThread> MessageThreads => Set<MessageThread>();
    public DbSet<Message> Messages => Set<Message>();
    public DbSet<MessageAttachment> MessageAttachments => Set<MessageAttachment>();

    // Investment/Financing - Yatirim ve Finansman Sistemi
    public DbSet<FinancingRequest> FinancingRequests => Set<FinancingRequest>();
    public DbSet<InvestmentOffer> InvestmentOffers => Set<InvestmentOffer>();

    // Capability Request - Hizmet Talebi
    public DbSet<CapabilityRequest> CapabilityRequests => Set<CapabilityRequest>();

    // JWT Refresh Tokens
    public DbSet<RefreshToken> RefreshTokens => Set<RefreshToken>();

    // Waitlist - Landing Page Email Capture
    public DbSet<WaitlistEntry> WaitlistEntries => Set<WaitlistEntry>();

    // Capability Profile - Her capability icin profil
    public DbSet<CapabilityProfile> CapabilityProfiles => Set<CapabilityProfile>();

    // Profile Contact Requests - Firma profil sayfasindan gelen mesajlar
    public DbSet<ProfileContactRequest> ProfileContactRequests => Set<ProfileContactRequest>();

    // Document Management - Belge Yonetimi
    public DbSet<DocumentTemplate> DocumentTemplates => Set<DocumentTemplate>();
    public DbSet<GeneratedDocument> GeneratedDocuments => Set<GeneratedDocument>();
    public DbSet<DocumentSignature> DocumentSignatures => Set<DocumentSignature>();
    public DbSet<DocumentNumberSequence> DocumentNumberSequences => Set<DocumentNumberSequence>();
    public DbSet<TransactionDocumentMapping> TransactionDocumentMappings => Set<TransactionDocumentMapping>();

    // Social Feed - Sosyal Paylasim Sistemi
    public DbSet<SocialPost> SocialPosts => Set<SocialPost>();
    public DbSet<SocialPostImage> SocialPostImages => Set<SocialPostImage>();
    public DbSet<SocialPostLike> SocialPostLikes => Set<SocialPostLike>();
    public DbSet<SocialPostComment> SocialPostComments => Set<SocialPostComment>();
    public DbSet<VendorFollow> VendorFollows => Set<VendorFollow>();
    public DbSet<SocialPostHashtag> SocialPostHashtags => Set<SocialPostHashtag>();
    public DbSet<SocialPostReport> SocialPostReports => Set<SocialPostReport>();
    public DbSet<SponsoredPost> SponsoredPosts => Set<SponsoredPost>();

    // Auctions - Acik Artirma Sistemi
    public DbSet<Auction> Auctions => Set<Auction>();
    public DbSet<AuctionBid> AuctionBids => Set<AuctionBid>();
    public DbSet<AuctionWatcher> AuctionWatchers => Set<AuctionWatcher>();

    // Reward Points - Odul Puan Sistemi
    public DbSet<RewardPointHistory> RewardPointHistories => Set<RewardPointHistory>();

    // Back in Stock - Stok Bildirim Sistemi
    public DbSet<BackInStockSubscription> BackInStockSubscriptions => Set<BackInStockSubscription>();

    // Type Tables - Artik code-based (ITypeResolver.cs): AddressTypes, VendorStatuses, etc.

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);

        // Identity tablolarini ozellesir
        builder.Entity<ApplicationUser>(entity =>
        {
            entity.ToTable("Users");
            entity.Property(u => u.FirstName).HasMaxLength(100);
            entity.Property(u => u.LastName).HasMaxLength(100);
        });

        builder.Entity<ApplicationRole>(entity =>
        {
            entity.ToTable("Roles");
            entity.Property(r => r.Description).HasMaxLength(500);
        });

        builder.Entity<Microsoft.AspNetCore.Identity.IdentityUserRole<int>>(entity =>
        {
            entity.ToTable("UserRoles");
        });

        builder.Entity<Microsoft.AspNetCore.Identity.IdentityUserClaim<int>>(entity =>
        {
            entity.ToTable("UserClaims");
        });

        builder.Entity<Microsoft.AspNetCore.Identity.IdentityUserLogin<int>>(entity =>
        {
            entity.ToTable("UserLogins");
        });

        builder.Entity<Microsoft.AspNetCore.Identity.IdentityRoleClaim<int>>(entity =>
        {
            entity.ToTable("RoleClaims");
        });

        builder.Entity<Microsoft.AspNetCore.Identity.IdentityUserToken<int>>(entity =>
        {
            entity.ToTable("UserTokens");
        });

        // Branch configuration
        builder.Entity<Branch>(entity =>
        {
            entity.ToTable("Branches");
            entity.HasIndex(b => b.Code).IsUnique();
            entity.Property(b => b.Code).HasMaxLength(20).IsRequired();
            entity.Property(b => b.Name).HasMaxLength(200).IsRequired();
            entity.HasQueryFilter(b => !b.IsDeleted);
        });

        // Vendor configuration
        builder.Entity<Vendor>(entity =>
        {
            entity.ToTable("Vendors");
            entity.Property(v => v.CompanyName).HasMaxLength(300).IsRequired();
            entity.Property(v => v.Email).HasMaxLength(200).IsRequired();
            entity.Property(v => v.Phone).HasMaxLength(20);
            entity.Property(v => v.TaxNumber).HasMaxLength(20);
            entity.Property(v => v.TaxOffice).HasMaxLength(100);
            entity.Property(v => v.TradeRegistryNo).HasMaxLength(50);
            entity.Property(v => v.MersisNo).HasMaxLength(20);
            entity.Property(v => v.Iban).HasMaxLength(34);
            entity.HasQueryFilter(v => !v.IsDeleted);

            // VendorStatusId - code-based type (VendorStatuses static class)

            // Vendor -> Addresses (one-to-many)
            entity.HasMany(v => v.Addresses)
                  .WithOne(a => a.Vendor)
                  .HasForeignKey(a => a.VendorId)
                  .OnDelete(DeleteBehavior.Cascade);

            // Vendor -> Users (one-to-many)
            entity.HasMany(v => v.Users)
                  .WithOne(u => u.Vendor)
                  .HasForeignKey(u => u.VendorId)
                  .OnDelete(DeleteBehavior.SetNull);

            // Vendor -> Warehouses (one-to-many)
            entity.HasMany(v => v.Warehouses)
                  .WithOne(w => w.Vendor)
                  .HasForeignKey(w => w.VendorId)
                  .OnDelete(DeleteBehavior.Cascade);
        });

        // Address configuration
        builder.Entity<Address>(entity =>
        {
            entity.ToTable("Addresses");
            entity.Property(a => a.Title).HasMaxLength(100).IsRequired();
            entity.Property(a => a.City).HasMaxLength(100).IsRequired();
            entity.Property(a => a.District).HasMaxLength(100).IsRequired();
            entity.Property(a => a.AddressLine).HasMaxLength(90).IsRequired();
            entity.Property(a => a.PostalCode).HasMaxLength(10);
            entity.Property(a => a.ContactName).HasMaxLength(200);
            entity.Property(a => a.ContactPhone).HasMaxLength(20);
            entity.HasQueryFilter(a => !a.IsDeleted);

            // FK relationships
            entity.HasOne(a => a.CountryEntity)
                  .WithMany()
                  .HasForeignKey(a => a.CountryId)
                  .OnDelete(DeleteBehavior.SetNull);

            entity.HasOne(a => a.StateEntity)
                  .WithMany()
                  .HasForeignKey(a => a.StateId)
                  .OnDelete(DeleteBehavior.SetNull);
        });

        // Language configuration
        builder.Entity<Language>(entity =>
        {
            entity.ToTable("Languages");
            entity.HasIndex(l => l.LanguageCulture).IsUnique();
            entity.HasIndex(l => l.UniqueSeoCode).IsUnique();
            entity.Property(l => l.Name).HasMaxLength(100).IsRequired();
            entity.Property(l => l.NativeName).HasMaxLength(100);
            entity.Property(l => l.LanguageCulture).HasMaxLength(10).IsRequired();
            entity.Property(l => l.UniqueSeoCode).HasMaxLength(5).IsRequired();
            entity.Property(l => l.Iso3Code).HasMaxLength(5);
            entity.Property(l => l.FlagEmoji).HasMaxLength(10);
            entity.Property(l => l.FlagImageFileName).HasMaxLength(100);
            entity.HasQueryFilter(l => !l.IsDeleted);
        });

        // LocaleStringResource configuration
        builder.Entity<LocaleStringResource>(entity =>
        {
            entity.ToTable("LocaleStringResources");
            entity.HasIndex(r => new { r.LanguageId, r.ResourceName }).IsUnique();
            entity.Property(r => r.ResourceName).HasMaxLength(500).IsRequired();
            entity.Property(r => r.ResourceValue).IsRequired();

            entity.HasOne(r => r.Language)
                  .WithMany(l => l.LocaleStringResources)
                  .HasForeignKey(r => r.LanguageId)
                  .OnDelete(DeleteBehavior.Cascade);
        });

        // User-Vendor relationship
        builder.Entity<ApplicationUser>(entity =>
        {
            entity.HasOne(u => u.Vendor)
                  .WithMany(v => v.Users)
                  .HasForeignKey(u => u.VendorId)
                  .OnDelete(DeleteBehavior.SetNull);
        });

        // VendorTeamMember configuration (davet + join request birlesmis model)
        builder.Entity<VendorTeamMember>(entity =>
        {
            entity.ToTable("VendorTeamMembers");
            entity.Property(m => m.Email).HasMaxLength(200).IsRequired();
            entity.Property(m => m.Name).HasMaxLength(200);
            entity.Property(m => m.InvitationToken).HasMaxLength(100);
            entity.Property(m => m.Message).HasMaxLength(1000);
            entity.Property(m => m.RejectionReason).HasMaxLength(500);

            // Indexes
            entity.HasIndex(m => m.InvitationToken).IsUnique().HasFilter("\"InvitationToken\" IS NOT NULL");
            entity.HasIndex(m => new { m.VendorId, m.Email });
            entity.HasIndex(m => new { m.VendorId, m.UserId });

            entity.HasQueryFilter(m => !m.IsDeleted);

            // TeamMemberStatusId - code-based type (TeamMemberStatuses static class)

            // Vendor iliskisi
            entity.HasOne(m => m.Vendor)
                  .WithMany()
                  .HasForeignKey(m => m.VendorId)
                  .OnDelete(DeleteBehavior.Cascade);

            // User iliskisi (nullable - davet kabul edilince dolar)
            entity.HasOne(m => m.User)
                  .WithMany()
                  .HasForeignKey(m => m.UserId)
                  .OnDelete(DeleteBehavior.SetNull);

            // ProcessedByUser iliskisi
            entity.HasOne(m => m.ProcessedByUser)
                  .WithMany()
                  .HasForeignKey(m => m.ProcessedByUserId)
                  .OnDelete(DeleteBehavior.SetNull);
        });

        // =========================================
        // RBAC - Role Based Access Control
        // =========================================

        // NOT: VendorCapabilities tablosu kaldirildi, Capabilities artik TypeDefinitions'da static

        // VendorCapabilityMapping configuration (Vendor-Capability many-to-many)
        // CapabilityId artik FK degil, sadece int deger (TypeDefinitions.Capabilities.Ids'den)
        builder.Entity<VendorCapabilityMapping>(entity =>
        {
            entity.ToTable("VendorCapabilityMappings");
            entity.HasIndex(m => new { m.VendorId, m.CapabilityId }).IsUnique();
            entity.HasQueryFilter(m => !m.IsDeleted);

            entity.HasOne(m => m.Vendor)
                  .WithMany(v => v.Capabilities)
                  .HasForeignKey(m => m.VendorId)
                  .OnDelete(DeleteBehavior.Cascade);
        });

        // PlatformModule configuration (Master modul listesi)
        builder.Entity<PlatformModule>(entity =>
        {
            entity.ToTable("PlatformModules");

            // DisplayNameResourceKey benzersiz - modul tanımlama icin kullanilir
            entity.HasIndex(m => m.DisplayNameResourceKey).IsUnique();
            entity.Property(m => m.DisplayNameResourceKey).HasMaxLength(100).IsRequired();
            entity.Property(m => m.Name).HasMaxLength(100).IsRequired();
            entity.Property(m => m.DisplayName).HasMaxLength(100);
            entity.Property(m => m.Description).HasMaxLength(500);
            entity.Property(m => m.Icon).HasMaxLength(50);
            entity.Property(m => m.Route).HasMaxLength(200);
            entity.HasQueryFilter(m => !m.IsDeleted);

            // Self-referencing for hierarchy (parent-child)
            entity.HasOne(m => m.Parent)
                  .WithMany(m => m.Children)
                  .HasForeignKey(m => m.ParentId)
                  .OnDelete(DeleteBehavior.Restrict);
        });

        // CapabilityModuleMapping configuration (Capability-Modul gorunurluk)
        // CapabilityId artik FK degil, sadece int deger (TypeDefinitions.Capabilities.Ids'den)
        builder.Entity<CapabilityModuleMapping>(entity =>
        {
            entity.ToTable("CapabilityModuleMappings");
            entity.HasIndex(m => new { m.CapabilityId, m.PlatformModuleId }).IsUnique();

            entity.HasOne(m => m.PlatformModule)
                  .WithMany(p => p.CapabilityMappings)
                  .HasForeignKey(m => m.PlatformModuleId)
                  .OnDelete(DeleteBehavior.Cascade);
        });

        // CompanyRole configuration (Firma ici roller)
        // CapabilityId artik FK degil, sadece int deger (TypeDefinitions.Capabilities.Ids'den)
        builder.Entity<CompanyRole>(entity =>
        {
            entity.ToTable("CompanyRoles");
            entity.Property(r => r.Name).HasMaxLength(100).IsRequired();
            entity.Property(r => r.NameResourceKey).HasMaxLength(100);
            entity.Property(r => r.Description).HasMaxLength(500);
            entity.HasQueryFilter(r => !r.IsDeleted);
        });

        // CompanyRoleUserMapping configuration (User-Firma-Rol many-to-many)
        builder.Entity<CompanyRoleUserMapping>(entity =>
        {
            entity.ToTable("CompanyRoleUserMappings");
            entity.HasIndex(r => new { r.UserId, r.VendorId, r.CompanyRoleId }).IsUnique();
            entity.HasQueryFilter(r => !r.IsDeleted);

            entity.HasOne(r => r.User)
                  .WithMany()
                  .HasForeignKey(r => r.UserId)
                  .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(r => r.Vendor)
                  .WithMany()
                  .HasForeignKey(r => r.VendorId)
                  .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(r => r.CompanyRole)
                  .WithMany(cr => cr.UserMappings)
                  .HasForeignKey(r => r.CompanyRoleId)
                  .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(r => r.AssignedByUser)
                  .WithMany()
                  .HasForeignKey(r => r.AssignedByUserId)
                  .OnDelete(DeleteBehavior.SetNull);
        });

        // CompanyRoleModulePermission configuration (Rol-Modul izinleri)
        // Not: Soft delete yok - silinen izin kalici silinir
        builder.Entity<CompanyRoleModulePermission>(entity =>
        {
            entity.ToTable("CompanyRoleModulePermissions");
            entity.HasIndex(p => new { p.CompanyRoleId, p.PlatformModuleId }).IsUnique();
            entity.Property(p => p.CustomPermissions).HasColumnType("jsonb");
            // No query filter - hard delete only

            entity.HasOne(p => p.CompanyRole)
                  .WithMany(r => r.ModulePermissions)
                  .HasForeignKey(p => p.CompanyRoleId)
                  .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(p => p.PlatformModule)
                  .WithMany(m => m.RolePermissions)
                  .HasForeignKey(p => p.PlatformModuleId)
                  .OnDelete(DeleteBehavior.Cascade);
        });

        // =========================================
        // GEOGRAPHY - Ülke, Eyalet, Şehir
        // =========================================

        // Country configuration
        builder.Entity<Country>(entity =>
        {
            entity.ToTable("Countries");
            entity.HasIndex(c => c.Iso2Code).IsUnique();
            entity.HasIndex(c => c.Iso3Code).IsUnique();
            entity.Property(c => c.Name).HasMaxLength(100).IsRequired();
            entity.Property(c => c.OfficialName).HasMaxLength(200);
            entity.Property(c => c.Iso2Code).HasMaxLength(2).IsRequired();
            entity.Property(c => c.Iso3Code).HasMaxLength(3).IsRequired();
            entity.Property(c => c.PhoneCode).HasMaxLength(10);
            entity.Property(c => c.CurrencyCode).HasMaxLength(3);
            entity.Property(c => c.CurrencyName).HasMaxLength(50);
            entity.Property(c => c.CurrencySymbol).HasMaxLength(5);
            entity.Property(c => c.FlagEmoji).HasMaxLength(10);
            entity.Property(c => c.Region).HasMaxLength(50);
            entity.Property(c => c.SubRegion).HasMaxLength(50);
            entity.Property(c => c.Capital).HasMaxLength(100);
            entity.HasQueryFilter(c => !c.IsDeleted);
        });

        // CountryTranslation configuration
        builder.Entity<CountryTranslation>(entity =>
        {
            entity.ToTable("CountryTranslations");
            entity.HasIndex(t => new { t.CountryId, t.LanguageCode }).IsUnique();
            entity.Property(t => t.LanguageCode).HasMaxLength(5).IsRequired();
            entity.Property(t => t.Name).HasMaxLength(100).IsRequired();
            entity.Property(t => t.OfficialName).HasMaxLength(200);
            entity.HasQueryFilter(t => !t.IsDeleted);

            entity.HasOne(t => t.Country)
                  .WithMany(c => c.Translations)
                  .HasForeignKey(t => t.CountryId)
                  .OnDelete(DeleteBehavior.Cascade);
        });

        // State configuration
        builder.Entity<State>(entity =>
        {
            entity.ToTable("States");
            entity.HasIndex(s => new { s.CountryId, s.Code }).IsUnique().HasFilter("\"Code\" IS NOT NULL");
            entity.Property(s => s.Name).HasMaxLength(100).IsRequired();
            entity.Property(s => s.Code).HasMaxLength(10);
            entity.Property(s => s.Type).HasMaxLength(50);
            entity.HasQueryFilter(s => !s.IsDeleted);

            entity.HasOne(s => s.Country)
                  .WithMany(c => c.States)
                  .HasForeignKey(s => s.CountryId)
                  .OnDelete(DeleteBehavior.Cascade);
        });

        // StateTranslation configuration
        builder.Entity<StateTranslation>(entity =>
        {
            entity.ToTable("StateTranslations");
            entity.HasIndex(t => new { t.StateId, t.LanguageCode }).IsUnique();
            entity.Property(t => t.LanguageCode).HasMaxLength(5).IsRequired();
            entity.Property(t => t.Name).HasMaxLength(100).IsRequired();
            entity.HasQueryFilter(t => !t.IsDeleted);

            entity.HasOne(t => t.State)
                  .WithMany(s => s.Translations)
                  .HasForeignKey(t => t.StateId)
                  .OnDelete(DeleteBehavior.Cascade);
        });

        // =========================================
        // LOGGING - Uygulama Loglari
        // =========================================

        builder.Entity<AppLog>(entity =>
        {
            entity.ToTable("AppLogs");

            // Performans icin index'ler
            entity.HasIndex(l => l.Timestamp).IsDescending();
            entity.HasIndex(l => l.Level);
            entity.HasIndex(l => new { l.Timestamp, l.Level });

            // Properties JSONB olarak saklanir
            entity.Property(l => l.Properties).HasColumnType("jsonb");
        });

        // =========================================
        // PRODUCTS - Satici Urun Modulu
        // =========================================

        // ProductCategory configuration (Global - Admin yonetimli)
        builder.Entity<ProductCategory>(entity =>
        {
            entity.ToTable("ProductCategories");
            entity.HasIndex(c => c.Slug).IsUnique().HasFilter("\"Slug\" IS NOT NULL");
            entity.Property(c => c.Name).HasMaxLength(100).IsRequired();
            entity.Property(c => c.Description).HasMaxLength(500);
            entity.Property(c => c.Icon).HasMaxLength(100);
            entity.Property(c => c.ImageUrl).HasMaxLength(200);
            entity.Property(c => c.Slug).HasMaxLength(200);
            entity.Property(c => c.MetaTitle).HasMaxLength(200);
            entity.Property(c => c.MetaDescription).HasMaxLength(500);
            entity.HasQueryFilter(c => !c.IsDeleted);

            // Self-referencing for hierarchy
            entity.HasOne(c => c.Parent)
                  .WithMany(c => c.Children)
                  .HasForeignKey(c => c.ParentId)
                  .OnDelete(DeleteBehavior.Restrict);
        });

        // Product configuration
        builder.Entity<Product>(entity =>
        {
            entity.ToTable("Products");
            entity.HasIndex(p => new { p.VendorId, p.SKU }).IsUnique().HasFilter("\"SKU\" IS NOT NULL");
            entity.HasIndex(p => new { p.VendorId, p.Slug }).IsUnique().HasFilter("\"Slug\" IS NOT NULL");
            entity.HasIndex(p => new { p.VendorId, p.ProductStatusId });
            entity.HasIndex(p => p.Barcode);
            entity.Property(p => p.Name).HasMaxLength(200).IsRequired();
            entity.Property(p => p.SKU).HasMaxLength(150);
            entity.Property(p => p.Barcode).HasMaxLength(100);
            entity.Property(p => p.Description).HasMaxLength(4000);
            entity.Property(p => p.ShortDescription).HasMaxLength(500);
            entity.Property(p => p.Currency).HasMaxLength(10);
            entity.Property(p => p.Tags).HasMaxLength(500);
            entity.Property(p => p.MetaTitle).HasMaxLength(200);
            entity.Property(p => p.MetaDescription).HasMaxLength(500);
            entity.Property(p => p.Slug).HasMaxLength(200);
            entity.Property(p => p.Price).HasPrecision(18, 4);
            entity.Property(p => p.CompareAtPrice).HasPrecision(18, 4);
            entity.Property(p => p.CostPrice).HasPrecision(18, 4);
            entity.Property(p => p.Weight).HasPrecision(10, 3);
            entity.Property(p => p.Length).HasPrecision(10, 2);
            entity.Property(p => p.Width).HasPrecision(10, 2);
            entity.Property(p => p.Height).HasPrecision(10, 2);
            entity.HasQueryFilter(p => !p.IsDeleted);

            // ProductStatusId - code-based type (ProductStatuses static class)

            // Vendor iliskisi
            entity.HasOne(p => p.Vendor)
                  .WithMany()
                  .HasForeignKey(p => p.VendorId)
                  .OnDelete(DeleteBehavior.Cascade);

            // Category iliskisi
            entity.HasOne(p => p.Category)
                  .WithMany(c => c.Products)
                  .HasForeignKey(p => p.CategoryId)
                  .OnDelete(DeleteBehavior.SetNull);
        });

        // ProductImage configuration
        builder.Entity<ProductImage>(entity =>
        {
            entity.ToTable("ProductImages");
            entity.HasIndex(i => new { i.ProductId, i.DisplayOrder });
            entity.Property(i => i.Url).HasMaxLength(500).IsRequired();
            entity.Property(i => i.AltText).HasMaxLength(200);
            entity.Property(i => i.Title).HasMaxLength(150);
            entity.Property(i => i.MimeType).HasMaxLength(50);
            entity.HasQueryFilter(i => !i.IsDeleted);

            // Product iliskisi
            entity.HasOne(i => i.Product)
                  .WithMany(p => p.Images)
                  .HasForeignKey(i => i.ProductId)
                  .OnDelete(DeleteBehavior.Cascade);
        });

        // ProductPriceTier configuration (Miktar bazli fiyat esikleri)
        builder.Entity<ProductPriceTier>(entity =>
        {
            entity.ToTable("ProductPriceTiers");
            entity.HasIndex(t => new { t.ProductId, t.MinQuantity }).IsUnique();
            entity.Property(t => t.Price).HasPrecision(18, 4);
            entity.Property(t => t.Description).HasMaxLength(100);
            entity.HasQueryFilter(t => !t.IsDeleted);

            // Product iliskisi
            entity.HasOne(t => t.Product)
                  .WithMany(p => p.PriceTiers)
                  .HasForeignKey(t => t.ProductId)
                  .OnDelete(DeleteBehavior.Cascade);
        });

        // ProductAttribute configuration (Urun ozellikleri)
        builder.Entity<ProductAttribute>(entity =>
        {
            entity.ToTable("ProductAttributes");
            entity.HasIndex(a => new { a.CategoryId, a.Name });
            entity.Property(a => a.Name).HasMaxLength(100).IsRequired();
            entity.Property(a => a.NameResourceKey).HasMaxLength(200);
            entity.Property(a => a.AttributeType).HasMaxLength(50).IsRequired();
            entity.Property(a => a.Unit).HasMaxLength(20);
            entity.Property(a => a.Icon).HasMaxLength(50);
            entity.HasQueryFilter(a => !a.IsDeleted);

            // Category iliskisi (opsiyonel - null = global)
            entity.HasOne(a => a.Category)
                  .WithMany()
                  .HasForeignKey(a => a.CategoryId)
                  .OnDelete(DeleteBehavior.SetNull);
        });

        // ProductAttributeValue configuration
        builder.Entity<ProductAttributeValue>(entity =>
        {
            entity.ToTable("ProductAttributeValues");
            entity.HasIndex(v => new { v.AttributeId, v.Value });
            entity.Property(v => v.Value).HasMaxLength(200).IsRequired();
            entity.Property(v => v.ValueResourceKey).HasMaxLength(200);
            entity.Property(v => v.AdditionalData).HasMaxLength(500);
            entity.HasQueryFilter(v => !v.IsDeleted);

            // Attribute iliskisi
            entity.HasOne(v => v.Attribute)
                  .WithMany(a => a.Values)
                  .HasForeignKey(v => v.AttributeId)
                  .OnDelete(DeleteBehavior.Cascade);
        });

        // ProductAttributeMapping configuration
        builder.Entity<ProductAttributeMapping>(entity =>
        {
            entity.ToTable("ProductAttributeMappings");
            entity.HasIndex(m => new { m.ProductId, m.AttributeId });
            entity.Property(m => m.CustomValue).HasMaxLength(500);
            entity.HasQueryFilter(m => !m.IsDeleted);

            // Product iliskisi
            entity.HasOne(m => m.Product)
                  .WithMany(p => p.AttributeMappings)
                  .HasForeignKey(m => m.ProductId)
                  .OnDelete(DeleteBehavior.Cascade);

            // Attribute iliskisi
            entity.HasOne(m => m.Attribute)
                  .WithMany(a => a.ProductMappings)
                  .HasForeignKey(m => m.AttributeId)
                  .OnDelete(DeleteBehavior.Cascade);

            // AttributeValue iliskisi (opsiyonel)
            entity.HasOne(m => m.AttributeValue)
                  .WithMany()
                  .HasForeignKey(m => m.AttributeValueId)
                  .OnDelete(DeleteBehavior.SetNull);
        });

        // Warehouse configuration
        builder.Entity<Warehouse>(entity =>
        {
            entity.ToTable("Warehouses");
            entity.HasIndex(w => new { w.VendorId, w.Code }).IsUnique();
            entity.Property(w => w.Code).HasMaxLength(20).IsRequired();
            entity.Property(w => w.Name).HasMaxLength(100).IsRequired();
            entity.Property(w => w.Description).HasMaxLength(500);
            entity.Property(w => w.CapacityUnit).HasMaxLength(20);
            entity.Property(w => w.ContactPhone).HasMaxLength(20);
            entity.Property(w => w.ContactEmail).HasMaxLength(100);
            entity.Property(w => w.OperatingHours).HasMaxLength(500);
            entity.Property(w => w.TotalCapacity).HasPrecision(18, 2);
            entity.HasQueryFilter(w => !w.IsDeleted);

            // WarehouseTypeId - code-based type (WarehouseTypes static class)

            // Address iliskisi (opsiyonel)
            entity.HasOne(w => w.Address)
                  .WithMany()
                  .HasForeignKey(w => w.AddressId)
                  .OnDelete(DeleteBehavior.SetNull);

            // Manager iliskisi (opsiyonel)
            entity.HasOne(w => w.Manager)
                  .WithMany()
                  .HasForeignKey(w => w.ManagerUserId)
                  .OnDelete(DeleteBehavior.SetNull);
        });

        // ProductWarehouseStock configuration
        builder.Entity<ProductWarehouseStock>(entity =>
        {
            entity.ToTable("ProductWarehouseStocks");
            entity.HasIndex(s => new { s.ProductId, s.WarehouseId }).IsUnique();
            entity.Property(s => s.Quantity).HasPrecision(18, 4);
            entity.Property(s => s.ReservedQuantity).HasPrecision(18, 4);
            entity.Property(s => s.MinStockLevel).HasPrecision(18, 4);
            entity.Property(s => s.MaxStockLevel).HasPrecision(18, 4);
            entity.Property(s => s.ReorderPoint).HasPrecision(18, 4);
            entity.Property(s => s.ReorderQuantity).HasPrecision(18, 4);
            entity.Property(s => s.BinLocation).HasMaxLength(50);
            entity.Property(s => s.Zone).HasMaxLength(50);
            entity.Property(s => s.Notes).HasMaxLength(500);
            entity.HasQueryFilter(s => !s.IsDeleted);

            // Product iliskisi
            entity.HasOne(s => s.Product)
                  .WithMany(p => p.WarehouseStocks)
                  .HasForeignKey(s => s.ProductId)
                  .OnDelete(DeleteBehavior.Cascade);

            // Warehouse iliskisi
            entity.HasOne(s => s.Warehouse)
                  .WithMany(w => w.ProductStocks)
                  .HasForeignKey(s => s.WarehouseId)
                  .OnDelete(DeleteBehavior.Cascade);
        });

        // ProductPackaging configuration
        builder.Entity<ProductPackaging>(entity =>
        {
            entity.ToTable("ProductPackagings");
            entity.HasIndex(p => new { p.ProductId, p.UnitId }).IsUnique();
            entity.Property(p => p.Barcode).HasMaxLength(50);
            entity.Property(p => p.SKU).HasMaxLength(50);
            entity.Property(p => p.GrossWeight).HasPrecision(18, 4);
            entity.Property(p => p.NetWeight).HasPrecision(18, 4);
            entity.Property(p => p.Length).HasPrecision(18, 2);
            entity.Property(p => p.Width).HasPrecision(18, 2);
            entity.Property(p => p.Height).HasPrecision(18, 2);
            entity.HasQueryFilter(p => !p.IsDeleted);

            entity.HasOne(p => p.Product)
                  .WithMany(pr => pr.Packagings)
                  .HasForeignKey(p => p.ProductId)
                  .OnDelete(DeleteBehavior.Cascade);
        });

        // StockMovement configuration
        builder.Entity<StockMovement>(entity =>
        {
            entity.ToTable("StockMovements");
            entity.Property(s => s.Quantity).HasPrecision(18, 4);
            entity.Property(s => s.PreviousQuantity).HasPrecision(18, 4);
            entity.Property(s => s.NewQuantity).HasPrecision(18, 4);
            entity.Property(s => s.ReferenceNumber).HasMaxLength(50);
            entity.Property(s => s.Notes).HasMaxLength(500);
            entity.HasQueryFilter(s => !s.IsDeleted);

            entity.HasIndex(s => s.VendorId);
            entity.HasIndex(s => s.ProductId);
            entity.HasIndex(s => s.WarehouseId);
            entity.HasIndex(s => s.MovementDate);
            entity.HasIndex(s => new { s.ReferenceType, s.ReferenceId });

            // Vendor iliskisi
            entity.HasOne(s => s.Vendor)
                  .WithMany()
                  .HasForeignKey(s => s.VendorId)
                  .OnDelete(DeleteBehavior.Restrict);

            // Product iliskisi
            entity.HasOne(s => s.Product)
                  .WithMany()
                  .HasForeignKey(s => s.ProductId)
                  .OnDelete(DeleteBehavior.Cascade);

            // Warehouse iliskisi
            entity.HasOne(s => s.Warehouse)
                  .WithMany()
                  .HasForeignKey(s => s.WarehouseId)
                  .OnDelete(DeleteBehavior.Cascade);

            // TargetWarehouse iliskisi (transfer icin)
            entity.HasOne(s => s.TargetWarehouse)
                  .WithMany()
                  .HasForeignKey(s => s.TargetWarehouseId)
                  .OnDelete(DeleteBehavior.Restrict);
        });

        // CategoryRequest configuration
        builder.Entity<CategoryRequest>(entity =>
        {
            entity.ToTable("CategoryRequests");
            entity.Property(r => r.RequestedName).HasMaxLength(200).IsRequired();
            entity.Property(r => r.Description).HasMaxLength(1000);
            entity.Property(r => r.ReviewNote).HasMaxLength(500);
            entity.HasQueryFilter(r => !r.IsDeleted);

            // Vendor iliskisi
            entity.HasOne(r => r.Vendor)
                  .WithMany()
                  .HasForeignKey(r => r.VendorId)
                  .OnDelete(DeleteBehavior.Restrict);

            // RequestedBy user iliskisi
            entity.HasOne(r => r.RequestedByUser)
                  .WithMany()
                  .HasForeignKey(r => r.RequestedByUserId)
                  .OnDelete(DeleteBehavior.Restrict);

            // ReviewedBy user iliskisi
            entity.HasOne(r => r.ReviewedByUser)
                  .WithMany()
                  .HasForeignKey(r => r.ReviewedByUserId)
                  .OnDelete(DeleteBehavior.Restrict);

            // SuggestedParentCategory iliskisi
            entity.HasOne(r => r.SuggestedParentCategory)
                  .WithMany()
                  .HasForeignKey(r => r.SuggestedParentCategoryId)
                  .OnDelete(DeleteBehavior.SetNull);

            // CreatedCategory iliskisi
            entity.HasOne(r => r.CreatedCategory)
                  .WithMany()
                  .HasForeignKey(r => r.CreatedCategoryId)
                  .OnDelete(DeleteBehavior.SetNull);
        });

        // =========================================
        // DEMANDS - Talep/Arayis Sistemi
        // =========================================

        // PublicDemand configuration
        builder.Entity<PublicDemand>(entity =>
        {
            entity.ToTable("PublicDemands");
            entity.HasIndex(d => d.Slug).IsUnique();
            entity.HasIndex(d => new { d.Status, d.Visibility });
            entity.HasIndex(d => new { d.VendorId, d.Status });
            entity.HasIndex(d => d.CategoryId);
            entity.Property(d => d.Title).HasMaxLength(200).IsRequired();
            entity.Property(d => d.Slug).HasMaxLength(200).IsRequired();
            entity.Property(d => d.Description).HasMaxLength(4000);
            entity.Property(d => d.Unit).HasMaxLength(20);
            entity.Property(d => d.Tags).HasMaxLength(500);
            entity.Property(d => d.ModificationNotes).HasMaxLength(2000);
            entity.Property(d => d.City).HasMaxLength(100);
            entity.Property(d => d.BudgetCurrency).HasMaxLength(10);
            entity.Property(d => d.MetaTitle).HasMaxLength(200);
            entity.Property(d => d.MetaDescription).HasMaxLength(500);
            entity.Property(d => d.BudgetMin).HasPrecision(18, 4);
            entity.Property(d => d.BudgetMax).HasPrecision(18, 4);
            entity.HasQueryFilter(d => !d.IsDeleted);

            // Vendor iliskisi
            entity.HasOne(d => d.Vendor)
                  .WithMany()
                  .HasForeignKey(d => d.VendorId)
                  .OnDelete(DeleteBehavior.Cascade);

            // Category iliskisi
            entity.HasOne(d => d.Category)
                  .WithMany()
                  .HasForeignKey(d => d.CategoryId)
                  .OnDelete(DeleteBehavior.SetNull);

            // ReferenceProduct iliskisi
            entity.HasOne(d => d.ReferenceProduct)
                  .WithMany()
                  .HasForeignKey(d => d.ReferenceProductId)
                  .OnDelete(DeleteBehavior.SetNull);

            // Country iliskisi
            entity.HasOne(d => d.Country)
                  .WithMany()
                  .HasForeignKey(d => d.CountryId)
                  .OnDelete(DeleteBehavior.SetNull);
        });

        // DemandResponse configuration
        builder.Entity<DemandResponse>(entity =>
        {
            entity.ToTable("DemandResponses");
            entity.HasIndex(r => new { r.DemandId, r.SupplierVendorId });
            entity.HasIndex(r => r.Status);
            entity.Property(r => r.ExternalCompanyName).HasMaxLength(200);
            entity.Property(r => r.ExternalContactName).HasMaxLength(200);
            entity.Property(r => r.ExternalEmail).HasMaxLength(200);
            entity.Property(r => r.ExternalPhone).HasMaxLength(50);
            entity.Property(r => r.ExternalWebsite).HasMaxLength(500);
            entity.Property(r => r.Currency).HasMaxLength(10);
            entity.Property(r => r.Unit).HasMaxLength(20);
            entity.Property(r => r.Notes).HasMaxLength(2000);
            entity.Property(r => r.TermsAndConditions).HasMaxLength(1000);
            entity.Property(r => r.RejectionReason).HasMaxLength(500);
            entity.Property(r => r.UnitPrice).HasPrecision(18, 4);
            entity.Property(r => r.TotalPrice).HasPrecision(18, 4);
            entity.HasQueryFilter(r => !r.IsDeleted);

            // Demand iliskisi
            entity.HasOne(r => r.Demand)
                  .WithMany(d => d.Responses)
                  .HasForeignKey(r => r.DemandId)
                  .OnDelete(DeleteBehavior.Cascade);

            // SupplierVendor iliskisi (nullable)
            entity.HasOne(r => r.SupplierVendor)
                  .WithMany()
                  .HasForeignKey(r => r.SupplierVendorId)
                  .OnDelete(DeleteBehavior.SetNull);

            // CurrentTurnVendor iliskisi (pazarlik sirasi)
            entity.HasOne(r => r.CurrentTurnVendor)
                  .WithMany()
                  .HasForeignKey(r => r.CurrentTurnVendorId)
                  .OnDelete(DeleteBehavior.SetNull);
        });

        // NegotiationRound configuration (Pazarlik turlari)
        builder.Entity<NegotiationRound>(entity =>
        {
            entity.ToTable("NegotiationRounds");
            entity.HasIndex(r => r.DemandResponseId);
            entity.HasIndex(r => new { r.DemandResponseId, r.RoundNumber }).IsUnique();
            entity.HasIndex(r => new { r.Status, r.ExpiresAt });
            entity.Property(r => r.Currency).HasMaxLength(10);
            entity.Property(r => r.Unit).HasMaxLength(20);
            entity.Property(r => r.Notes).HasMaxLength(2000);
            entity.Property(r => r.TermsAndConditions).HasMaxLength(1000);
            entity.Property(r => r.RejectionReason).HasMaxLength(500);
            entity.Property(r => r.UnitPrice).HasPrecision(18, 4);
            entity.Property(r => r.TotalPrice).HasPrecision(18, 4);
            entity.HasQueryFilter(r => !r.IsDeleted);

            // DemandResponse iliskisi
            entity.HasOne(r => r.DemandResponse)
                  .WithMany(d => d.NegotiationRounds)
                  .HasForeignKey(r => r.DemandResponseId)
                  .OnDelete(DeleteBehavior.Cascade);

            // Initiator (karsi teklif veren) Vendor iliskisi
            entity.HasOne(r => r.Initiator)
                  .WithMany()
                  .HasForeignKey(r => r.InitiatorVendorId)
                  .OnDelete(DeleteBehavior.Restrict);
        });

        // DemandAttachment configuration
        builder.Entity<DemandAttachment>(entity =>
        {
            entity.ToTable("DemandAttachments");
            entity.HasIndex(a => a.DemandId);
            entity.Property(a => a.FileName).HasMaxLength(500).IsRequired();
            entity.Property(a => a.FilePath).HasMaxLength(1000).IsRequired();
            entity.Property(a => a.MimeType).HasMaxLength(100);
            entity.Property(a => a.Title).HasMaxLength(200);
            entity.Property(a => a.Description).HasMaxLength(500);
            entity.HasQueryFilter(a => !a.IsDeleted);

            entity.HasOne(a => a.Demand)
                  .WithMany(d => d.Attachments)
                  .HasForeignKey(a => a.DemandId)
                  .OnDelete(DeleteBehavior.Cascade);
        });

        // DemandModification configuration
        builder.Entity<DemandModification>(entity =>
        {
            entity.ToTable("DemandModifications");
            entity.HasIndex(m => m.DemandId);
            entity.Property(m => m.PropertyName).HasMaxLength(100).IsRequired();
            entity.Property(m => m.OriginalValue).HasMaxLength(200);
            entity.Property(m => m.DesiredValue).HasMaxLength(200).IsRequired();
            entity.Property(m => m.Notes).HasMaxLength(500);
            entity.HasQueryFilter(m => !m.IsDeleted);

            entity.HasOne(m => m.Demand)
                  .WithMany(d => d.Modifications)
                  .HasForeignKey(m => m.DemandId)
                  .OnDelete(DeleteBehavior.Cascade);
        });

        // DemandResponseAttachment configuration
        builder.Entity<DemandResponseAttachment>(entity =>
        {
            entity.ToTable("DemandResponseAttachments");
            entity.HasIndex(a => a.ResponseId);
            entity.Property(a => a.FileName).HasMaxLength(500).IsRequired();
            entity.Property(a => a.FilePath).HasMaxLength(1000).IsRequired();
            entity.Property(a => a.MimeType).HasMaxLength(100);
            entity.Property(a => a.Title).HasMaxLength(200);
            entity.HasQueryFilter(a => !a.IsDeleted);

            entity.HasOne(a => a.Response)
                  .WithMany(r => r.Attachments)
                  .HasForeignKey(a => a.ResponseId)
                  .OnDelete(DeleteBehavior.Cascade);
        });

        // =========================================
        // SUPPLIER DISCOVERY - Uretici Kesfi
        // =========================================

        // SupplierProfile configuration
        builder.Entity<SupplierProfile>(entity =>
        {
            entity.ToTable("SupplierProfiles");
            entity.HasIndex(s => s.Slug).IsUnique();
            entity.HasIndex(s => s.VendorId).IsUnique();
            entity.HasIndex(s => s.IsPubliclyVisible);
            entity.Property(s => s.Slug).HasMaxLength(200).IsRequired();
            entity.Property(s => s.DisplayName).HasMaxLength(200);
            entity.Property(s => s.Description).HasMaxLength(2000);
            entity.Property(s => s.ShortDescription).HasMaxLength(500);
            entity.Property(s => s.Tagline).HasMaxLength(500);
            entity.Property(s => s.Capabilities).HasMaxLength(2000);
            entity.Property(s => s.ProductionCapacity).HasMaxLength(1000);
            entity.Property(s => s.MinimumOrderValue).HasMaxLength(500);
            entity.Property(s => s.LeadTime).HasMaxLength(500);
            entity.Property(s => s.CategoryIds).HasMaxLength(500);
            entity.Property(s => s.Certifications).HasMaxLength(1000);
            entity.Property(s => s.City).HasMaxLength(100);
            entity.Property(s => s.ServiceRegions).HasMaxLength(500);
            entity.Property(s => s.PublicEmail).HasMaxLength(200);
            entity.Property(s => s.PublicPhone).HasMaxLength(50);
            entity.Property(s => s.PublicWebsite).HasMaxLength(500);
            entity.Property(s => s.CoverImageUrl).HasMaxLength(500);
            entity.Property(s => s.GalleryImages).HasMaxLength(500);
            entity.Property(s => s.MetaTitle).HasMaxLength(200);
            entity.Property(s => s.MetaDescription).HasMaxLength(500);
            entity.Property(s => s.MetaKeywords).HasMaxLength(500);
            entity.Property(s => s.AverageRating).HasPrecision(3, 2);
            entity.HasQueryFilter(s => !s.IsDeleted);

            // Vendor iliskisi (1:1)
            entity.HasOne(s => s.Vendor)
                  .WithMany()
                  .HasForeignKey(s => s.VendorId)
                  .OnDelete(DeleteBehavior.Cascade);

            // Country iliskisi
            entity.HasOne(s => s.Country)
                  .WithMany()
                  .HasForeignKey(s => s.CountryId)
                  .OnDelete(DeleteBehavior.SetNull);
        });

        // CategorySubscription configuration
        builder.Entity<CategorySubscription>(entity =>
        {
            entity.ToTable("CategorySubscriptions");
            entity.HasIndex(s => new { s.VendorId, s.CategoryId }).IsUnique();
            entity.Property(s => s.CountryFilter).HasMaxLength(500);
            entity.Property(s => s.KeywordFilter).HasMaxLength(500);
            entity.HasQueryFilter(s => !s.IsDeleted);

            // Vendor iliskisi
            entity.HasOne(s => s.Vendor)
                  .WithMany()
                  .HasForeignKey(s => s.VendorId)
                  .OnDelete(DeleteBehavior.Cascade);

            // Category iliskisi
            entity.HasOne(s => s.Category)
                  .WithMany()
                  .HasForeignKey(s => s.CategoryId)
                  .OnDelete(DeleteBehavior.Cascade);
        });

        // =========================================
        // NOTIFICATIONS - Bildirim Sistemi
        // =========================================

        builder.Entity<Notification>(entity =>
        {
            entity.ToTable("Notifications");
            entity.HasIndex(n => new { n.VendorId, n.IsRead, n.CreatedAt });
            entity.HasIndex(n => new { n.UserId, n.IsRead, n.CreatedAt });
            entity.HasIndex(n => n.CreatedAt).IsDescending();
            entity.Property(n => n.Title).HasMaxLength(200).IsRequired();
            entity.Property(n => n.Message).HasMaxLength(1000).IsRequired();
            entity.Property(n => n.EntityType).HasMaxLength(50);
            entity.Property(n => n.ActionUrl).HasMaxLength(500);
            entity.Property(n => n.Icon).HasMaxLength(50);
            entity.HasQueryFilter(n => !n.IsDeleted);

            // Vendor iliskisi
            entity.HasOne(n => n.Vendor)
                  .WithMany()
                  .HasForeignKey(n => n.VendorId)
                  .OnDelete(DeleteBehavior.Cascade);

            // User iliskisi (optional)
            entity.HasOne(n => n.User)
                  .WithMany()
                  .HasForeignKey(n => n.UserId)
                  .OnDelete(DeleteBehavior.SetNull);
        });

        // ============================================
        // PRODUCT INQUIRIES - Urun Fiyat Istekleri
        // ============================================
        builder.Entity<ProductInquiry>(entity =>
        {
            entity.ToTable("ProductInquiries");
            entity.HasKey(i => i.Id);
            entity.HasIndex(i => i.BuyerVendorId);
            entity.HasIndex(i => i.SellerVendorId);
            entity.HasIndex(i => i.ProductId);
            entity.HasIndex(i => i.Status);
            entity.HasIndex(i => i.CreatedAt).IsDescending();
            entity.HasQueryFilter(i => !i.IsDeleted);

            entity.Property(i => i.Unit).HasMaxLength(50);
            entity.Property(i => i.Message).HasMaxLength(2000);
            entity.Property(i => i.SpecialRequirements).HasMaxLength(2000);
            entity.Property(i => i.OfferedCurrency).HasMaxLength(10);

            entity.HasOne(i => i.Product)
                  .WithMany()
                  .HasForeignKey(i => i.ProductId)
                  .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(i => i.BuyerVendor)
                  .WithMany()
                  .HasForeignKey(i => i.BuyerVendorId)
                  .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(i => i.SellerVendor)
                  .WithMany()
                  .HasForeignKey(i => i.SellerVendorId)
                  .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(i => i.DeliveryAddress)
                  .WithMany()
                  .HasForeignKey(i => i.DeliveryAddressId)
                  .OnDelete(DeleteBehavior.SetNull);
        });

        builder.Entity<ProductInquiryResponse>(entity =>
        {
            entity.ToTable("ProductInquiryResponses");
            entity.HasKey(r => r.Id);
            entity.HasIndex(r => r.InquiryId);
            entity.HasIndex(r => r.Status);
            entity.HasQueryFilter(r => !r.IsDeleted);

            entity.Property(r => r.UnitPrice).HasPrecision(18, 2);
            entity.Property(r => r.TotalPrice).HasPrecision(18, 2);
            entity.Property(r => r.Currency).HasMaxLength(10).HasDefaultValue("TRY");
            entity.Property(r => r.OfferedUnit).HasMaxLength(50);
            entity.Property(r => r.Notes).HasMaxLength(2000);
            entity.Property(r => r.TermsAndConditions).HasMaxLength(4000);

            entity.HasOne(r => r.Inquiry)
                  .WithMany(i => i.Responses)
                  .HasForeignKey(r => r.InquiryId)
                  .OnDelete(DeleteBehavior.Cascade);
        });

        // ============================================
        // ORDERS - Siparis Sistemi
        // ============================================

        // Order configuration
        builder.Entity<Order>(entity =>
        {
            entity.ToTable("Orders");
            entity.HasKey(o => o.Id);
            entity.HasIndex(o => o.OrderNumber).IsUnique();
            entity.HasIndex(o => o.BuyerVendorId);
            entity.HasIndex(o => o.SellerVendorId);
            entity.HasIndex(o => o.Status);
            entity.HasIndex(o => new { o.BuyerVendorId, o.Status });
            entity.HasIndex(o => new { o.SellerVendorId, o.Status });
            entity.HasIndex(o => o.CreatedAt).IsDescending();
            entity.HasQueryFilter(o => !o.IsDeleted);

            entity.Property(o => o.OrderNumber).HasMaxLength(50).IsRequired();
            entity.Property(o => o.SubTotal).HasPrecision(18, 4);
            entity.Property(o => o.ShippingCost).HasPrecision(18, 4);
            entity.Property(o => o.TaxAmount).HasPrecision(18, 4);
            entity.Property(o => o.TotalAmount).HasPrecision(18, 4);
            entity.Property(o => o.Currency).HasMaxLength(10);
            entity.Property(o => o.Notes).HasMaxLength(2000);
            entity.Property(o => o.CancellationReason).HasMaxLength(500);
            entity.Property(o => o.StripePaymentIntentId).HasMaxLength(200);
            entity.Property(o => o.StripeChargeId).HasMaxLength(200);

            // BuyerVendor iliskisi
            entity.HasOne(o => o.BuyerVendor)
                  .WithMany()
                  .HasForeignKey(o => o.BuyerVendorId)
                  .OnDelete(DeleteBehavior.Restrict);

            // SellerVendor iliskisi
            entity.HasOne(o => o.SellerVendor)
                  .WithMany()
                  .HasForeignKey(o => o.SellerVendorId)
                  .OnDelete(DeleteBehavior.Restrict);

            // ShippingAddress iliskisi
            entity.HasOne(o => o.ShippingAddress)
                  .WithMany()
                  .HasForeignKey(o => o.ShippingAddressId)
                  .OnDelete(DeleteBehavior.SetNull);

            // BillingAddress iliskisi
            entity.HasOne(o => o.BillingAddress)
                  .WithMany()
                  .HasForeignKey(o => o.BillingAddressId)
                  .OnDelete(DeleteBehavior.SetNull);

            // SourceInquiry iliskisi
            entity.HasOne(o => o.SourceInquiry)
                  .WithMany()
                  .HasForeignKey(o => o.SourceInquiryId)
                  .OnDelete(DeleteBehavior.SetNull);

            // SourceDemand iliskisi
            entity.HasOne(o => o.SourceDemand)
                  .WithMany()
                  .HasForeignKey(o => o.SourceDemandId)
                  .OnDelete(DeleteBehavior.SetNull);

            // SourceDemandResponse iliskisi
            entity.HasOne(o => o.SourceDemandResponse)
                  .WithMany()
                  .HasForeignKey(o => o.SourceDemandResponseId)
                  .OnDelete(DeleteBehavior.SetNull);

            // FinancingRequest iliskisi (Siparis icin olusturulan finansman talebi)
            entity.HasOne(o => o.FinancingRequest)
                  .WithMany()
                  .HasForeignKey(o => o.FinancingRequestId)
                  .OnDelete(DeleteBehavior.SetNull);
        });

        // OrderItem configuration
        builder.Entity<OrderItem>(entity =>
        {
            entity.ToTable("OrderItems");
            entity.HasKey(i => i.Id);
            entity.HasIndex(i => i.OrderId);
            entity.HasIndex(i => i.ProductId);
            entity.HasQueryFilter(i => !i.IsDeleted);

            entity.Property(i => i.Unit).HasMaxLength(20);
            entity.Property(i => i.UnitPrice).HasPrecision(18, 4);
            entity.Property(i => i.TotalPrice).HasPrecision(18, 4);

            // Order iliskisi
            entity.HasOne(i => i.Order)
                  .WithMany(o => o.Items)
                  .HasForeignKey(i => i.OrderId)
                  .OnDelete(DeleteBehavior.Cascade);

            // Product iliskisi
            entity.HasOne(i => i.Product)
                  .WithMany()
                  .HasForeignKey(i => i.ProductId)
                  .OnDelete(DeleteBehavior.Restrict);

            // Warehouse iliskisi
            entity.HasOne(i => i.Warehouse)
                  .WithMany()
                  .HasForeignKey(i => i.WarehouseId)
                  .OnDelete(DeleteBehavior.SetNull);
        });

        // OrderShipment configuration
        builder.Entity<OrderShipment>(entity =>
        {
            entity.ToTable("OrderShipments");
            entity.HasKey(s => s.Id);
            entity.HasIndex(s => s.OrderId);
            entity.HasIndex(s => new { s.OrderId, s.ShipmentNumber }).IsUnique();
            entity.HasIndex(s => s.TrackingNumber);
            entity.HasQueryFilter(s => !s.IsDeleted);

            entity.Property(s => s.ShipmentNumber).HasMaxLength(50).IsRequired();
            entity.Property(s => s.CarrierCode).HasMaxLength(20);
            entity.Property(s => s.CarrierName).HasMaxLength(100);
            entity.Property(s => s.TrackingNumber).HasMaxLength(100);
            entity.Property(s => s.TrackingUrl).HasMaxLength(500);
            entity.Property(s => s.Notes).HasMaxLength(1000);

            // Order iliskisi
            entity.HasOne(s => s.Order)
                  .WithMany(o => o.Shipments)
                  .HasForeignKey(s => s.OrderId)
                  .OnDelete(DeleteBehavior.Cascade);
        });

        // OrderShipmentItem configuration
        builder.Entity<OrderShipmentItem>(entity =>
        {
            entity.ToTable("OrderShipmentItems");
            entity.HasKey(i => i.Id);
            entity.HasIndex(i => new { i.ShipmentId, i.OrderItemId }).IsUnique();
            entity.HasQueryFilter(i => !i.IsDeleted);

            // Shipment iliskisi
            entity.HasOne(i => i.Shipment)
                  .WithMany(s => s.Items)
                  .HasForeignKey(i => i.ShipmentId)
                  .OnDelete(DeleteBehavior.Cascade);

            // OrderItem iliskisi
            entity.HasOne(i => i.OrderItem)
                  .WithMany()
                  .HasForeignKey(i => i.OrderItemId)
                  .OnDelete(DeleteBehavior.Restrict);
        });

        // OrderStatusHistory configuration
        builder.Entity<OrderStatusHistory>(entity =>
        {
            entity.ToTable("OrderStatusHistory");
            entity.HasKey(h => h.Id);
            entity.HasIndex(h => h.OrderId);
            entity.HasIndex(h => new { h.OrderId, h.CreatedAt });
            entity.HasQueryFilter(h => !h.IsDeleted);

            entity.Property(h => h.Notes).HasMaxLength(500);

            // Order iliskisi
            entity.HasOne(h => h.Order)
                  .WithMany(o => o.StatusHistory)
                  .HasForeignKey(h => h.OrderId)
                  .OnDelete(DeleteBehavior.Cascade);
        });

        // StripePayment configuration
        builder.Entity<StripePayment>(entity =>
        {
            entity.ToTable("StripePayments");
            entity.HasKey(p => p.Id);
            entity.HasIndex(p => p.OrderId);
            entity.HasIndex(p => p.PaymentIntentId);
            entity.HasIndex(p => p.ChargeId);
            entity.HasQueryFilter(p => !p.IsDeleted);

            entity.Property(p => p.PaymentIntentId).HasMaxLength(200);
            entity.Property(p => p.ChargeId).HasMaxLength(200);
            entity.Property(p => p.Amount).HasPrecision(18, 4);
            entity.Property(p => p.Currency).HasMaxLength(10);
            entity.Property(p => p.Status).HasMaxLength(50);
            entity.Property(p => p.FailureCode).HasMaxLength(100);
            entity.Property(p => p.FailureMessage).HasMaxLength(500);
            entity.Property(p => p.ReceiptUrl).HasMaxLength(500);
            entity.Property(p => p.RefundedAmount).HasPrecision(18, 4);

            // Order iliskisi
            entity.HasOne(p => p.Order)
                  .WithMany(o => o.StripePayments)
                  .HasForeignKey(p => p.OrderId)
                  .OnDelete(DeleteBehavior.Cascade);
        });

        // ============================================
        // ORDER SERVICE REQUESTS (Lojistik, Gumruk, Sigorta)
        // ============================================

        // OrderServiceRequest configuration
        builder.Entity<OrderServiceRequest>(entity =>
        {
            entity.ToTable("OrderServiceRequests");
            entity.HasKey(r => r.Id);
            entity.HasIndex(r => r.OrderId);
            entity.HasIndex(r => new { r.OrderId, r.ServiceType });
            entity.HasIndex(r => r.Status);
            entity.HasQueryFilter(r => !r.IsDeleted);

            entity.Property(r => r.Title).HasMaxLength(200).IsRequired();
            entity.Property(r => r.Description).HasMaxLength(2000);
            entity.Property(r => r.WeightKg).HasPrecision(18, 4);
            entity.Property(r => r.VolumeM3).HasPrecision(18, 4);
            entity.Property(r => r.CargoValue).HasPrecision(18, 4);
            entity.Property(r => r.Currency).HasMaxLength(10);
            entity.Property(r => r.OriginCity).HasMaxLength(100);
            entity.Property(r => r.OriginAddress).HasMaxLength(500);
            entity.Property(r => r.DestinationCity).HasMaxLength(100);
            entity.Property(r => r.DestinationAddress).HasMaxLength(500);
            entity.Property(r => r.Incoterms).HasMaxLength(10);
            entity.Property(r => r.HsCode).HasMaxLength(20);

            // Order iliskisi
            entity.HasOne(r => r.Order)
                  .WithMany(o => o.ServiceRequests)
                  .HasForeignKey(r => r.OrderId)
                  .OnDelete(DeleteBehavior.Cascade);

            // OriginCountry iliskisi
            entity.HasOne(r => r.OriginCountry)
                  .WithMany()
                  .HasForeignKey(r => r.OriginCountryId)
                  .OnDelete(DeleteBehavior.SetNull);

            // DestinationCountry iliskisi
            entity.HasOne(r => r.DestinationCountry)
                  .WithMany()
                  .HasForeignKey(r => r.DestinationCountryId)
                  .OnDelete(DeleteBehavior.SetNull);

            // SelectedQuote iliskisi
            entity.HasOne(r => r.SelectedQuote)
                  .WithMany()
                  .HasForeignKey(r => r.SelectedQuoteId)
                  .OnDelete(DeleteBehavior.SetNull);

            // DependsOnServiceRequest iliskisi (Survey -> Logistics gibi)
            entity.HasOne(r => r.DependsOnServiceRequest)
                  .WithMany(r => r.DependentRequests)
                  .HasForeignKey(r => r.DependsOnServiceRequestId)
                  .OnDelete(DeleteBehavior.SetNull);

            entity.Property(r => r.TriggerSource).HasMaxLength(100);
        });

        // OrderServiceQuote configuration
        builder.Entity<OrderServiceQuote>(entity =>
        {
            entity.ToTable("OrderServiceQuotes");
            entity.HasKey(q => q.Id);
            entity.HasIndex(q => q.ServiceRequestId);
            entity.HasIndex(q => q.ProviderVendorId);
            entity.HasIndex(q => new { q.ServiceRequestId, q.ProviderVendorId }); // Unique degil - birden fazla teklif verilebilir
            entity.HasIndex(q => q.Status);
            entity.HasQueryFilter(q => !q.IsDeleted);

            entity.Property(q => q.QuoteAmount).HasPrecision(18, 4);
            entity.Property(q => q.Currency).HasMaxLength(10);
            entity.Property(q => q.IncludedServices).HasMaxLength(2000);
            entity.Property(q => q.AdditionalCosts).HasMaxLength(1000);
            entity.Property(q => q.Notes).HasMaxLength(2000);
            entity.Property(q => q.TermsAndConditions).HasMaxLength(4000);
            entity.Property(q => q.CarrierName).HasMaxLength(100);
            entity.Property(q => q.CoverageDetails).HasMaxLength(2000);
            entity.Property(q => q.Deductible).HasPrecision(18, 4);
            entity.Property(q => q.RejectionReason).HasMaxLength(500);

            // ServiceRequest iliskisi
            entity.HasOne(q => q.ServiceRequest)
                  .WithMany(r => r.Quotes)
                  .HasForeignKey(q => q.ServiceRequestId)
                  .OnDelete(DeleteBehavior.Cascade);

            // ProviderVendor iliskisi
            entity.HasOne(q => q.ProviderVendor)
                  .WithMany()
                  .HasForeignKey(q => q.ProviderVendorId)
                  .OnDelete(DeleteBehavior.Restrict);
        });

        // OrderParticipant configuration
        builder.Entity<OrderParticipant>(entity =>
        {
            entity.ToTable("OrderParticipants");
            entity.HasKey(p => p.Id);
            entity.HasIndex(p => p.OrderId);
            entity.HasIndex(p => new { p.OrderId, p.VendorId, p.Role }).IsUnique();
            entity.HasIndex(p => p.Status);
            entity.HasQueryFilter(p => !p.IsDeleted);

            entity.Property(p => p.Amount).HasPrecision(18, 4);
            entity.Property(p => p.Currency).HasMaxLength(10);
            entity.Property(p => p.CommissionAmount).HasPrecision(18, 4);
            entity.Property(p => p.NetAmount).HasPrecision(18, 4);
            entity.Property(p => p.PaymentReference).HasMaxLength(200);

            // Order iliskisi
            entity.HasOne(p => p.Order)
                  .WithMany(o => o.Participants)
                  .HasForeignKey(p => p.OrderId)
                  .OnDelete(DeleteBehavior.Cascade);

            // Vendor iliskisi
            entity.HasOne(p => p.Vendor)
                  .WithMany()
                  .HasForeignKey(p => p.VendorId)
                  .OnDelete(DeleteBehavior.Restrict);

            // ServiceQuote iliskisi
            entity.HasOne(p => p.ServiceQuote)
                  .WithMany()
                  .HasForeignKey(p => p.ServiceQuoteId)
                  .OnDelete(DeleteBehavior.SetNull);
        });

        // OrderTask configuration
        builder.Entity<OrderTask>(entity =>
        {
            entity.ToTable("OrderTasks");
            entity.HasKey(t => t.Id);
            entity.HasIndex(t => t.OrderId);
            entity.HasIndex(t => t.ParticipantId);
            entity.HasIndex(t => new { t.OrderId, t.SortOrder });
            entity.HasIndex(t => t.Status);
            entity.HasQueryFilter(t => !t.IsDeleted);

            entity.Property(t => t.Title).HasMaxLength(200).IsRequired();
            entity.Property(t => t.Description).HasMaxLength(2000);
            entity.Property(t => t.CompletionNotes).HasMaxLength(1000);
            entity.Property(t => t.ReferenceData).HasColumnType("jsonb");

            // Order iliskisi
            entity.HasOne(t => t.Order)
                  .WithMany(o => o.Tasks)
                  .HasForeignKey(t => t.OrderId)
                  .OnDelete(DeleteBehavior.Cascade);

            // Participant iliskisi
            entity.HasOne(t => t.Participant)
                  .WithMany(p => p.Tasks)
                  .HasForeignKey(t => t.ParticipantId)
                  .OnDelete(DeleteBehavior.Cascade);

            // DependsOnTask iliskisi (self-referencing)
            entity.HasOne(t => t.DependsOnTask)
                  .WithMany()
                  .HasForeignKey(t => t.DependsOnTaskId)
                  .OnDelete(DeleteBehavior.SetNull);
        });

        // OrderInvestment configuration
        builder.Entity<OrderInvestment>(entity =>
        {
            entity.ToTable("OrderInvestments");
            entity.HasKey(i => i.Id);
            entity.HasIndex(i => i.OrderId);
            entity.HasIndex(i => i.InvestorVendorId);
            entity.HasIndex(i => new { i.OrderId, i.InvestorVendorId }).IsUnique();
            entity.HasIndex(i => i.Status);
            entity.HasQueryFilter(i => !i.IsDeleted);

            entity.Property(i => i.Amount).HasPrecision(18, 4);
            entity.Property(i => i.Currency).HasMaxLength(10);
            entity.Property(i => i.PercentageOfTotal).HasPrecision(5, 2);
            entity.Property(i => i.ReturnRate).HasPrecision(5, 2);
            entity.Property(i => i.ExpectedReturn).HasPrecision(18, 4);
            entity.Property(i => i.RepaidAmount).HasPrecision(18, 4);
            entity.Property(i => i.TermsAndConditions).HasMaxLength(4000);
            entity.Property(i => i.Notes).HasMaxLength(2000);

            // Order iliskisi
            entity.HasOne(i => i.Order)
                  .WithMany(o => o.Investments)
                  .HasForeignKey(i => i.OrderId)
                  .OnDelete(DeleteBehavior.Cascade);

            // InvestorVendor iliskisi
            entity.HasOne(i => i.InvestorVendor)
                  .WithMany()
                  .HasForeignKey(i => i.InvestorVendorId)
                  .OnDelete(DeleteBehavior.Restrict);
        });

        // ============================================
        // CUSTOMS DECLARATIONS - Gumruk Beyannameleri
        // ============================================

        // CustomsDeclaration configuration
        builder.Entity<CustomsDeclaration>(entity =>
        {
            entity.ToTable("CustomsDeclarations");
            entity.HasKey(d => d.Id);
            entity.HasIndex(d => d.OrderId);
            entity.HasIndex(d => d.EvrimDosyaNo);
            entity.HasIndex(d => d.DeclarationNumber);
            entity.HasIndex(d => d.StatusId);
            entity.HasIndex(d => new { d.OrderId, d.DeclarationTypeId });
            entity.HasQueryFilter(d => !d.IsDeleted);

            entity.Property(d => d.EvrimDosyaNo).HasMaxLength(50);
            entity.Property(d => d.EvrimDosyaTipi).HasMaxLength(10);
            entity.Property(d => d.DeclarationNumber).HasMaxLength(50);
            entity.Property(d => d.CustomsOfficeCode).HasMaxLength(10);
            entity.Property(d => d.CustomsOfficeName).HasMaxLength(200);
            entity.Property(d => d.RegimeCode).HasMaxLength(10);
            entity.Property(d => d.DeliveryTerms).HasMaxLength(10);
            entity.Property(d => d.DeliveryPlace).HasMaxLength(200);
            entity.Property(d => d.TaxDue).HasPrecision(18, 4);
            entity.Property(d => d.TaxPaid).HasPrecision(18, 4);
            entity.Property(d => d.EvrimResponse).HasColumnType("text");
            entity.Property(d => d.LastError).HasMaxLength(2000);
            entity.Property(d => d.Notes).HasMaxLength(2000);

            // Order iliskisi
            entity.HasOne(d => d.Order)
                  .WithMany()
                  .HasForeignKey(d => d.OrderId)
                  .OnDelete(DeleteBehavior.Cascade);
        });

        // =========================================
        // BUYER FEATURES - Favori Tedarikciler, Degerlendirmeler
        // =========================================

        // FavoriteVendor configuration
        builder.Entity<FavoriteVendor>(entity =>
        {
            entity.ToTable("FavoriteVendors");
            entity.HasKey(f => f.Id);
            entity.HasIndex(f => new { f.BuyerVendorId, f.SellerVendorId }).IsUnique();
            entity.HasIndex(f => f.BuyerVendorId);
            entity.HasIndex(f => f.SellerVendorId);
            entity.HasQueryFilter(f => !f.IsDeleted);

            entity.Property(f => f.Notes).HasMaxLength(500);
            entity.Property(f => f.Tags).HasMaxLength(500);

            // BuyerVendor iliskisi
            entity.HasOne(f => f.BuyerVendor)
                  .WithMany()
                  .HasForeignKey(f => f.BuyerVendorId)
                  .OnDelete(DeleteBehavior.Cascade);

            // SellerVendor iliskisi
            entity.HasOne(f => f.SellerVendor)
                  .WithMany()
                  .HasForeignKey(f => f.SellerVendorId)
                  .OnDelete(DeleteBehavior.Restrict);
        });

        // VendorReview configuration
        builder.Entity<VendorReview>(entity =>
        {
            entity.ToTable("VendorReviews");
            entity.HasKey(r => r.Id);
            entity.HasIndex(r => r.ReviewerVendorId);
            entity.HasIndex(r => r.ReviewedVendorId);
            entity.HasIndex(r => r.OrderId);
            entity.HasIndex(r => new { r.ReviewerVendorId, r.OrderId }).IsUnique().HasFilter("\"OrderId\" IS NOT NULL");
            entity.HasQueryFilter(r => !r.IsDeleted);

            entity.Property(r => r.Title).HasMaxLength(200);
            entity.Property(r => r.Comment).HasMaxLength(2000);
            entity.Property(r => r.Pros).HasMaxLength(500);
            entity.Property(r => r.Cons).HasMaxLength(500);
            entity.Property(r => r.SellerResponse).HasMaxLength(2000);

            // ReviewerVendor iliskisi
            entity.HasOne(r => r.ReviewerVendor)
                  .WithMany()
                  .HasForeignKey(r => r.ReviewerVendorId)
                  .OnDelete(DeleteBehavior.Cascade);

            // ReviewedVendor iliskisi
            entity.HasOne(r => r.ReviewedVendor)
                  .WithMany()
                  .HasForeignKey(r => r.ReviewedVendorId)
                  .OnDelete(DeleteBehavior.Restrict);

            // Order iliskisi (optional)
            entity.HasOne(r => r.Order)
                  .WithMany()
                  .HasForeignKey(r => r.OrderId)
                  .OnDelete(DeleteBehavior.SetNull);
        });

        // ProductReview configuration
        builder.Entity<ProductReview>(entity =>
        {
            entity.ToTable("ProductReviews");
            entity.HasKey(r => r.Id);
            entity.HasIndex(r => r.BuyerVendorId);
            entity.HasIndex(r => r.SellerVendorId);
            entity.HasIndex(r => r.ProductId);
            entity.HasIndex(r => r.OrderId);
            entity.HasIndex(r => new { r.ProductId, r.BuyerVendorId }).IsUnique().HasFilter("\"IsDeleted\" = false");
            entity.HasQueryFilter(r => !r.IsDeleted);

            entity.Property(r => r.Title).HasMaxLength(200);
            entity.Property(r => r.Comment).HasMaxLength(2000);
            entity.Property(r => r.Pros).HasMaxLength(500);
            entity.Property(r => r.Cons).HasMaxLength(500);
            entity.Property(r => r.SellerResponse).HasMaxLength(2000);

            entity.HasOne(r => r.Product)
                  .WithMany()
                  .HasForeignKey(r => r.ProductId)
                  .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(r => r.BuyerVendor)
                  .WithMany()
                  .HasForeignKey(r => r.BuyerVendorId)
                  .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(r => r.SellerVendor)
                  .WithMany()
                  .HasForeignKey(r => r.SellerVendorId)
                  .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(r => r.Order)
                  .WithMany()
                  .HasForeignKey(r => r.OrderId)
                  .OnDelete(DeleteBehavior.SetNull);
        });

        // =========================================
        // MESSAGING - Mesajlasma Sistemi
        // =========================================

        // MessageThread configuration
        builder.Entity<MessageThread>(entity =>
        {
            entity.ToTable("MessageThreads");
            entity.HasIndex(t => t.InitiatorVendorId);
            entity.HasIndex(t => t.RecipientVendorId);
            entity.HasIndex(t => t.LastMessageAt);
            entity.HasIndex(t => new { t.ReferenceType, t.ReferenceId });
            entity.Property(t => t.Subject).HasMaxLength(200).IsRequired();
            entity.HasQueryFilter(t => !t.IsDeleted);

            // InitiatorVendor iliskisi
            entity.HasOne(t => t.InitiatorVendor)
                  .WithMany()
                  .HasForeignKey(t => t.InitiatorVendorId)
                  .OnDelete(DeleteBehavior.Restrict);

            // RecipientVendor iliskisi
            entity.HasOne(t => t.RecipientVendor)
                  .WithMany()
                  .HasForeignKey(t => t.RecipientVendorId)
                  .OnDelete(DeleteBehavior.Restrict);

            // InitiatorUser iliskisi
            entity.HasOne(t => t.InitiatorUser)
                  .WithMany()
                  .HasForeignKey(t => t.InitiatorUserId)
                  .OnDelete(DeleteBehavior.Restrict);
        });

        // Message configuration
        builder.Entity<Message>(entity =>
        {
            entity.ToTable("Messages");
            entity.HasIndex(m => m.ThreadId);
            entity.HasIndex(m => m.SenderVendorId);
            entity.HasIndex(m => new { m.ThreadId, m.CreatedAt });
            entity.HasQueryFilter(m => !m.IsDeleted);

            // Thread iliskisi
            entity.HasOne(m => m.Thread)
                  .WithMany(t => t.Messages)
                  .HasForeignKey(m => m.ThreadId)
                  .OnDelete(DeleteBehavior.Cascade);

            // SenderVendor iliskisi
            entity.HasOne(m => m.SenderVendor)
                  .WithMany()
                  .HasForeignKey(m => m.SenderVendorId)
                  .OnDelete(DeleteBehavior.Restrict);

            // SenderUser iliskisi
            entity.HasOne(m => m.SenderUser)
                  .WithMany()
                  .HasForeignKey(m => m.SenderUserId)
                  .OnDelete(DeleteBehavior.Restrict);
        });

        // MessageAttachment configuration
        builder.Entity<MessageAttachment>(entity =>
        {
            entity.ToTable("MessageAttachments");
            entity.HasIndex(a => a.MessageId);
            entity.Property(a => a.FileName).HasMaxLength(255).IsRequired();
            entity.Property(a => a.FilePath).HasMaxLength(500).IsRequired();
            entity.Property(a => a.ContentType).HasMaxLength(100);
            entity.HasQueryFilter(a => !a.IsDeleted);

            // Message iliskisi
            entity.HasOne(a => a.Message)
                  .WithMany(m => m.Attachments)
                  .HasForeignKey(a => a.MessageId)
                  .OnDelete(DeleteBehavior.Cascade);
        });

        // =========================================
        // INVESTMENT/FINANCING - Yatirim ve Finansman
        // =========================================

        // FinancingRequest configuration
        builder.Entity<FinancingRequest>(entity =>
        {
            entity.ToTable("FinancingRequests");
            entity.HasKey(f => f.Id);
            entity.HasIndex(f => f.RequesterVendorId);
            entity.HasIndex(f => f.FinancingType);
            entity.HasIndex(f => f.Status);
            entity.HasIndex(f => new { f.Status, f.CreatedAt });
            entity.HasIndex(f => f.RelatedOrderId);
            entity.HasQueryFilter(f => !f.IsDeleted);

            entity.Property(f => f.Title).HasMaxLength(200).IsRequired();
            entity.Property(f => f.Description).HasMaxLength(4000);
            entity.Property(f => f.RequestedAmount).HasPrecision(18, 4);
            entity.Property(f => f.Currency).HasMaxLength(10);
            entity.Property(f => f.TotalValue).HasPrecision(18, 4);
            entity.Property(f => f.MaxInterestRate).HasPrecision(5, 2);
            entity.Property(f => f.FundedAmount).HasPrecision(18, 4);
            entity.Property(f => f.CollateralDescription).HasMaxLength(2000);
            entity.Property(f => f.InvoiceNumber).HasMaxLength(50);
            entity.Property(f => f.DebtorName).HasMaxLength(200);
            entity.Property(f => f.DebtorTaxNumber).HasMaxLength(20);
            entity.Property(f => f.RiskNotes).HasMaxLength(1000);

            // RequesterVendor iliskisi
            entity.HasOne(f => f.RequesterVendor)
                  .WithMany()
                  .HasForeignKey(f => f.RequesterVendorId)
                  .OnDelete(DeleteBehavior.Restrict);

            // RelatedOrder iliskisi (optional)
            entity.HasOne(f => f.RelatedOrder)
                  .WithMany()
                  .HasForeignKey(f => f.RelatedOrderId)
                  .OnDelete(DeleteBehavior.SetNull);
        });

        // InvestmentOffer configuration
        builder.Entity<InvestmentOffer>(entity =>
        {
            entity.ToTable("InvestmentOffers");
            entity.HasKey(o => o.Id);
            entity.HasIndex(o => o.FinancingRequestId);
            entity.HasIndex(o => o.InvestorVendorId);
            entity.HasIndex(o => new { o.FinancingRequestId, o.InvestorVendorId });
            entity.HasIndex(o => o.Status);
            entity.HasIndex(o => new { o.InvestorVendorId, o.Status });
            entity.HasQueryFilter(o => !o.IsDeleted);

            entity.Property(o => o.OfferedAmount).HasPrecision(18, 4);
            entity.Property(o => o.InterestRate).HasPrecision(5, 2);
            entity.Property(o => o.TotalRepaymentAmount).HasPrecision(18, 4);
            entity.Property(o => o.Notes).HasMaxLength(2000);
            entity.Property(o => o.RejectionReason).HasMaxLength(500);
            entity.Property(o => o.TransferReference).HasMaxLength(100);
            entity.Property(o => o.RepaymentReference).HasMaxLength(100);

            // FinancingRequest iliskisi
            entity.HasOne(o => o.FinancingRequest)
                  .WithMany(f => f.Offers)
                  .HasForeignKey(o => o.FinancingRequestId)
                  .OnDelete(DeleteBehavior.Cascade);

            // InvestorVendor iliskisi
            entity.HasOne(o => o.InvestorVendor)
                  .WithMany()
                  .HasForeignKey(o => o.InvestorVendorId)
                  .OnDelete(DeleteBehavior.Restrict);

            // InvestorUser iliskisi
            entity.HasOne(o => o.InvestorUser)
                  .WithMany()
                  .HasForeignKey(o => o.InvestorUserId)
                  .OnDelete(DeleteBehavior.Restrict);

            // ResponseByUser iliskisi
            entity.HasOne(o => o.ResponseByUser)
                  .WithMany()
                  .HasForeignKey(o => o.ResponseByUserId)
                  .OnDelete(DeleteBehavior.SetNull);
        });
        // =========================================
        // WAITLIST - Landing Page Email Capture
        // =========================================

        builder.Entity<WaitlistEntry>(entity =>
        {
            entity.ToTable("WaitlistEntries");
            entity.HasIndex(w => w.Email).IsUnique();
            entity.Property(w => w.Email).HasMaxLength(256).IsRequired();
            entity.Property(w => w.IpAddress).HasMaxLength(50);
            entity.Property(w => w.UserAgent).HasMaxLength(500);
            entity.Property(w => w.Source).HasMaxLength(50);
        });

        // =========================================
        // JWT REFRESH TOKENS
        // =========================================

        builder.Entity<RefreshToken>(entity =>
        {
            entity.ToTable("RefreshTokens");
            entity.HasIndex(r => r.TokenHash).IsUnique();
            entity.HasIndex(r => r.UserId);
            entity.HasIndex(r => r.JwtId);
            entity.Property(r => r.TokenHash).HasMaxLength(128).IsRequired();
            entity.Property(r => r.JwtId).HasMaxLength(50).IsRequired();
            entity.Property(r => r.DeviceInfo).HasMaxLength(500);
            entity.Property(r => r.IpAddress).HasMaxLength(50);
            entity.HasQueryFilter(r => !r.IsDeleted);

            entity.HasOne(r => r.User)
                  .WithMany()
                  .HasForeignKey(r => r.UserId)
                  .OnDelete(DeleteBehavior.Cascade);
        });
        // =========================================
        // SOCIAL FEED - Sosyal Paylasim Sistemi
        // =========================================

        // SocialPost configuration
        builder.Entity<SocialPost>(entity =>
        {
            entity.ToTable("SocialPosts");
            entity.HasIndex(p => new { p.VendorId, p.StatusId });
            entity.HasIndex(p => new { p.StatusId, p.PublishedAt });
            entity.HasIndex(p => p.AuthorUserId);
            entity.HasIndex(p => p.PublishedAt).IsDescending();
            entity.Property(p => p.Title).HasMaxLength(200);
            entity.Property(p => p.Content).HasMaxLength(4000).IsRequired();
            entity.HasQueryFilter(p => !p.IsDeleted);

            // PostTypeId - code-based type (SocialPostTypes static class)
            // StatusId - code-based type (SocialPostStatuses static class)

            // Vendor iliskisi
            entity.HasOne(p => p.Vendor)
                  .WithMany()
                  .HasForeignKey(p => p.VendorId)
                  .OnDelete(DeleteBehavior.Cascade);

            // Author iliskisi
            entity.HasOne(p => p.Author)
                  .WithMany()
                  .HasForeignKey(p => p.AuthorUserId)
                  .OnDelete(DeleteBehavior.Restrict);

            // Product iliskisi (optional - ProductShowcase icin)
            entity.HasOne(p => p.Product)
                  .WithMany()
                  .HasForeignKey(p => p.ProductId)
                  .OnDelete(DeleteBehavior.SetNull);
        });

        // SocialPostImage configuration
        builder.Entity<SocialPostImage>(entity =>
        {
            entity.ToTable("SocialPostImages");
            entity.HasIndex(i => new { i.SocialPostId, i.DisplayOrder });
            entity.Property(i => i.Url).HasMaxLength(500).IsRequired();
            entity.Property(i => i.AltText).HasMaxLength(200);
            entity.Property(i => i.MimeType).HasMaxLength(50);
            entity.HasQueryFilter(i => !i.IsDeleted);

            entity.HasOne(i => i.SocialPost)
                  .WithMany(p => p.Images)
                  .HasForeignKey(i => i.SocialPostId)
                  .OnDelete(DeleteBehavior.Cascade);
        });

        // SocialPostLike configuration
        builder.Entity<SocialPostLike>(entity =>
        {
            entity.ToTable("SocialPostLikes");
            entity.HasIndex(l => new { l.SocialPostId, l.UserId }).IsUnique();
            entity.HasIndex(l => l.SocialPostId);
            entity.HasQueryFilter(l => !l.IsDeleted);

            entity.HasOne(l => l.SocialPost)
                  .WithMany(p => p.Likes)
                  .HasForeignKey(l => l.SocialPostId)
                  .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(l => l.User)
                  .WithMany()
                  .HasForeignKey(l => l.UserId)
                  .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(l => l.Vendor)
                  .WithMany()
                  .HasForeignKey(l => l.VendorId)
                  .OnDelete(DeleteBehavior.Restrict);
        });

        // SocialPostComment configuration
        builder.Entity<SocialPostComment>(entity =>
        {
            entity.ToTable("SocialPostComments");
            entity.HasIndex(c => c.SocialPostId);
            entity.HasIndex(c => new { c.SocialPostId, c.CreatedAt });
            entity.Property(c => c.Content).HasMaxLength(2000).IsRequired();
            entity.HasQueryFilter(c => !c.IsDeleted);

            entity.HasOne(c => c.SocialPost)
                  .WithMany(p => p.Comments)
                  .HasForeignKey(c => c.SocialPostId)
                  .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(c => c.User)
                  .WithMany()
                  .HasForeignKey(c => c.UserId)
                  .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(c => c.Vendor)
                  .WithMany()
                  .HasForeignKey(c => c.VendorId)
                  .OnDelete(DeleteBehavior.Restrict);

            // Self-referencing for replies
            entity.HasOne(c => c.ParentComment)
                  .WithMany(c => c.Replies)
                  .HasForeignKey(c => c.ParentCommentId)
                  .OnDelete(DeleteBehavior.Restrict);
        });

        // VendorFollow configuration
        builder.Entity<VendorFollow>(entity =>
        {
            entity.ToTable("VendorFollows");
            entity.HasIndex(f => new { f.FollowerVendorId, f.FollowedVendorId }).IsUnique();
            entity.HasIndex(f => f.FollowerVendorId);
            entity.HasIndex(f => f.FollowedVendorId);
            entity.HasQueryFilter(f => !f.IsDeleted);

            entity.HasOne(f => f.FollowerVendor)
                  .WithMany()
                  .HasForeignKey(f => f.FollowerVendorId)
                  .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(f => f.FollowedVendor)
                  .WithMany()
                  .HasForeignKey(f => f.FollowedVendorId)
                  .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(f => f.FollowedByUser)
                  .WithMany()
                  .HasForeignKey(f => f.FollowedByUserId)
                  .OnDelete(DeleteBehavior.Restrict);
        });

        // SocialPostHashtag configuration
        builder.Entity<SocialPostHashtag>(entity =>
        {
            entity.ToTable("SocialPostHashtags");
            entity.HasIndex(h => h.Tag);
            entity.HasIndex(h => new { h.Tag, h.CreatedAt });
            entity.HasIndex(h => h.SocialPostId);
            entity.Property(h => h.Tag).HasMaxLength(100).IsRequired();
            entity.HasQueryFilter(h => !h.IsDeleted);

            entity.HasOne(h => h.SocialPost)
                  .WithMany(p => p.Hashtags)
                  .HasForeignKey(h => h.SocialPostId)
                  .OnDelete(DeleteBehavior.Cascade);
        });

        // SocialPostReport configuration
        builder.Entity<SocialPostReport>(entity =>
        {
            entity.ToTable("SocialPostReports");
            entity.HasIndex(r => new { r.StatusId, r.CreatedAt });
            entity.HasIndex(r => r.SocialPostId);
            entity.Property(r => r.Description).HasMaxLength(1000);
            entity.Property(r => r.AdminNote).HasMaxLength(2000);
            entity.HasQueryFilter(r => !r.IsDeleted);

            entity.HasOne(r => r.SocialPost)
                  .WithMany()
                  .HasForeignKey(r => r.SocialPostId)
                  .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(r => r.ReporterUser)
                  .WithMany()
                  .HasForeignKey(r => r.ReporterUserId)
                  .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(r => r.ReporterVendor)
                  .WithMany()
                  .HasForeignKey(r => r.ReporterVendorId)
                  .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(r => r.ReviewedByUser)
                  .WithMany()
                  .HasForeignKey(r => r.ReviewedByUserId)
                  .OnDelete(DeleteBehavior.Restrict);
        });

        // SponsoredPost configuration
        builder.Entity<SponsoredPost>(entity =>
        {
            entity.ToTable("SponsoredPosts");
            entity.HasIndex(s => s.VendorId);
            entity.HasIndex(s => s.StatusId);
            entity.HasIndex(s => new { s.StatusId, s.EndDate });
            entity.Property(s => s.Currency).HasMaxLength(10);
            entity.Property(s => s.BudgetAmount).HasColumnType("decimal(18,2)");
            entity.Property(s => s.SpentAmount).HasColumnType("decimal(18,2)");
            entity.HasQueryFilter(s => !s.IsDeleted);

            entity.HasOne(s => s.SocialPost)
                  .WithMany()
                  .HasForeignKey(s => s.SocialPostId)
                  .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(s => s.Vendor)
                  .WithMany()
                  .HasForeignKey(s => s.VendorId)
                  .OnDelete(DeleteBehavior.Cascade);
        });

        // ========== AUCTION ==========
        builder.Entity<Auction>(entity =>
        {
            entity.ToTable("Auctions");
            entity.HasIndex(a => a.VendorId);
            entity.HasIndex(a => a.AuctionStatusId);
            entity.HasIndex(a => a.Slug);
            entity.HasIndex(a => new { a.AuctionStatusId, a.EndAt });
            entity.Property(a => a.Title).HasMaxLength(200).IsRequired();
            entity.Property(a => a.Description).HasMaxLength(4000);
            entity.Property(a => a.Currency).HasMaxLength(10);
            entity.Property(a => a.Slug).HasMaxLength(200);
            entity.Property(a => a.StartingPrice).HasColumnType("decimal(18,2)");
            entity.Property(a => a.ReservePrice).HasColumnType("decimal(18,2)");
            entity.Property(a => a.CurrentHighestBid).HasColumnType("decimal(18,2)");
            entity.Property(a => a.BuyNowPrice).HasColumnType("decimal(18,2)");
            entity.HasQueryFilter(a => !a.IsDeleted);

            entity.HasOne(a => a.Vendor)
                  .WithMany()
                  .HasForeignKey(a => a.VendorId)
                  .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(a => a.Product)
                  .WithMany()
                  .HasForeignKey(a => a.ProductId)
                  .OnDelete(DeleteBehavior.SetNull);

            entity.HasOne(a => a.Category)
                  .WithMany()
                  .HasForeignKey(a => a.CategoryId)
                  .OnDelete(DeleteBehavior.SetNull);
        });

        // AuctionBid configuration
        builder.Entity<AuctionBid>(entity =>
        {
            entity.ToTable("AuctionBids");
            entity.HasIndex(b => b.AuctionId);
            entity.HasIndex(b => new { b.AuctionId, b.BidAmount });
            entity.HasIndex(b => b.BuyerVendorId);
            entity.Property(b => b.BidAmount).HasColumnType("decimal(18,2)");
            entity.Property(b => b.BidderMessage).HasMaxLength(500);
            entity.HasQueryFilter(b => !b.IsDeleted);

            entity.HasOne(b => b.Auction)
                  .WithMany(a => a.Bids)
                  .HasForeignKey(b => b.AuctionId)
                  .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(b => b.BuyerVendor)
                  .WithMany()
                  .HasForeignKey(b => b.BuyerVendorId)
                  .OnDelete(DeleteBehavior.Restrict);
        });

        // AuctionWatcher configuration
        builder.Entity<AuctionWatcher>(entity =>
        {
            entity.ToTable("AuctionWatchers");
            entity.HasIndex(w => new { w.AuctionId, w.WatcherVendorId }).IsUnique();
            entity.HasIndex(w => w.WatcherVendorId);
            entity.HasQueryFilter(w => !w.IsDeleted);

            entity.HasOne(w => w.Auction)
                  .WithMany(a => a.Watchers)
                  .HasForeignKey(w => w.AuctionId)
                  .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(w => w.WatcherVendor)
                  .WithMany()
                  .HasForeignKey(w => w.WatcherVendorId)
                  .OnDelete(DeleteBehavior.Restrict);
        });

        // ========== BACK IN STOCK ==========
        builder.Entity<BackInStockSubscription>(entity =>
        {
            entity.ToTable("BackInStockSubscriptions");
            entity.HasIndex(s => new { s.VendorId, s.ProductId }).IsUnique();
            entity.HasIndex(s => s.ProductId);
            entity.Property(s => s.Email).HasMaxLength(200);
            entity.HasQueryFilter(s => !s.IsDeleted);

            entity.HasOne(s => s.Product)
                  .WithMany()
                  .HasForeignKey(s => s.ProductId)
                  .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(s => s.Vendor)
                  .WithMany()
                  .HasForeignKey(s => s.VendorId)
                  .OnDelete(DeleteBehavior.Restrict);
        });

        // ========== REWARD POINTS ==========
        builder.Entity<RewardPointHistory>(entity =>
        {
            entity.ToTable("RewardPointHistories");
            entity.HasIndex(r => r.VendorId);
            entity.HasIndex(r => new { r.VendorId, r.CreatedAt });
            entity.HasIndex(r => r.ActionTypeId);
            entity.Property(r => r.Description).HasMaxLength(500);
            entity.HasQueryFilter(r => !r.IsDeleted);

            entity.HasOne(r => r.Vendor)
                  .WithMany()
                  .HasForeignKey(r => r.VendorId)
                  .OnDelete(DeleteBehavior.Restrict);
        });
    }

    public override Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        foreach (var entry in ChangeTracker.Entries())
        {
            if (entry.Entity is BaseEntity entity)
            {
                switch (entry.State)
                {
                    case EntityState.Added:
                        entity.CreatedAt = DateTime.UtcNow;
                        break;
                    case EntityState.Modified:
                        entity.UpdatedAt = DateTime.UtcNow;
                        break;
                }
            }
        }

        return base.SaveChangesAsync(cancellationToken);
    }
}
