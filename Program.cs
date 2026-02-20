using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Serilog;
using Serilog.Events;
using Serilog.Sinks.PostgreSQL;
using Serilog.Sinks.PostgreSQL.ColumnWriters;
using NpgsqlTypes;
using Bridgo.Data;
using Bridgo.Models.Identity;
using Bridgo.Models.Entities;
using Bridgo.Services;
using Bridgo.Services.Interfaces;
using Bridgo.Repositories;
using Bridgo.Middleware;
using Bridgo.Handlers;
using Bridgo.Configuration;

// Serilog Bootstrap Logger (uygulama baslamadan once)
Log.Logger = new LoggerConfiguration()
    .MinimumLevel.Information()
    .WriteTo.Console()
    .CreateBootstrapLogger();

try
{
    Log.Information("Bridgo Platform baslatiliyor...");

var builder = WebApplication.CreateBuilder(args);

// Serilog yapilandirmasi
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");

// PostgreSQL kolon eslestirmesi
IDictionary<string, ColumnWriterBase> columnWriters = new Dictionary<string, ColumnWriterBase>
{
    { "Timestamp", new TimestampColumnWriter(NpgsqlDbType.TimestampTz) },
    { "Level", new LevelColumnWriter(true, NpgsqlDbType.Varchar) },
    { "Message", new RenderedMessageColumnWriter(NpgsqlDbType.Text) },
    { "MessageTemplate", new MessageTemplateColumnWriter(NpgsqlDbType.Text) },
    { "Exception", new ExceptionColumnWriter(NpgsqlDbType.Text) },
    { "Properties", new PropertiesColumnWriter(NpgsqlDbType.Jsonb) },
    { "MachineName", new SinglePropertyColumnWriter("MachineName", PropertyWriteMethod.ToString, NpgsqlDbType.Varchar) },
    { "UserName", new SinglePropertyColumnWriter("UserName", PropertyWriteMethod.ToString, NpgsqlDbType.Varchar) },
    { "RequestPath", new SinglePropertyColumnWriter("RequestPath", PropertyWriteMethod.ToString, NpgsqlDbType.Varchar) },
    { "RequestMethod", new SinglePropertyColumnWriter("RequestMethod", PropertyWriteMethod.ToString, NpgsqlDbType.Varchar) },
    { "SourceContext", new SinglePropertyColumnWriter("SourceContext", PropertyWriteMethod.ToString, NpgsqlDbType.Varchar) },
    { "ActionName", new SinglePropertyColumnWriter("ActionName", PropertyWriteMethod.ToString, NpgsqlDbType.Varchar) }
};

builder.Host.UseSerilog((context, services, configuration) =>
{
    configuration
        .ReadFrom.Configuration(context.Configuration)
        .ReadFrom.Services(services)
        .Enrich.FromLogContext()
        .Enrich.WithMachineName()
        .MinimumLevel.Information()
        .MinimumLevel.Override("Microsoft", LogEventLevel.Warning)
        .MinimumLevel.Override("Microsoft.EntityFrameworkCore", LogEventLevel.Warning)
        .MinimumLevel.Override("Microsoft.AspNetCore", LogEventLevel.Warning)
        .WriteTo.Console(outputTemplate: "[{Timestamp:HH:mm:ss} {Level:u3}] {Message:lj}{NewLine}{Exception}");

    // PostgreSQL log sink sadece Development'ta (Cloud'da AppLogs tablosu olmayabilir)
    if (!context.HostingEnvironment.IsProduction())
    {
        configuration.WriteTo.PostgreSQL(
            connectionString: connectionString!,
            tableName: "AppLogs",
            columnOptions: columnWriters,
            needAutoCreateTable: false,
            restrictedToMinimumLevel: LogEventLevel.Information
        );
    }
});

// PostgreSQL DbContext
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));

// Identity (int ID)
builder.Services.AddIdentity<ApplicationUser, ApplicationRole>(options =>
{
    // Password settings
    options.Password.RequireDigit = true;
    options.Password.RequireLowercase = true;
    options.Password.RequireUppercase = true;
    options.Password.RequireNonAlphanumeric = false;
    options.Password.RequiredLength = 6;

    // Lockout settings
    options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(5);
    options.Lockout.MaxFailedAccessAttempts = 5;

    // User settings
    options.User.RequireUniqueEmail = true;
})
.AddEntityFrameworkStores<ApplicationDbContext>()
.AddDefaultTokenProviders()
.AddClaimsPrincipalFactory<CustomUserClaimsPrincipalFactory>();

// Cookie Authentication - Identity default olarak cookie kullaniyor
builder.Services.ConfigureApplicationCookie(options =>
{
    options.LoginPath = "/Account/Login";
    options.LogoutPath = "/Account/Logout";
    options.AccessDeniedPath = "/Account/AccessDenied";
    options.ExpireTimeSpan = TimeSpan.FromHours(24);
    options.SlidingExpiration = true;

    // API isteklerinde 401 donmesi icin (redirect yerine)
    options.Events.OnRedirectToLogin = context =>
    {
        if (context.Request.Path.StartsWithSegments("/api"))
        {
            context.Response.StatusCode = StatusCodes.Status401Unauthorized;
            return Task.CompletedTask;
        }
        context.Response.Redirect(context.RedirectUri);
        return Task.CompletedTask;
    };
    options.Events.OnRedirectToAccessDenied = context =>
    {
        if (context.Request.Path.StartsWithSegments("/api"))
        {
            context.Response.StatusCode = StatusCodes.Status403Forbidden;
            return Task.CompletedTask;
        }
        context.Response.Redirect(context.RedirectUri);
        return Task.CompletedTask;
    };
});

// JWT Bearer Authentication (mobil, desktop, 3. parti API erisimi icin)
var jwtSettings = builder.Configuration.GetSection("JwtSettings");
var jwtSecretKey = jwtSettings["SecretKey"] ?? throw new InvalidOperationException("JWT SecretKey not configured");

builder.Services.AddAuthentication()
    .AddJwtBearer(JwtBearerDefaults.AuthenticationScheme, options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSecretKey)),
            ValidateIssuer = true,
            ValidIssuer = jwtSettings["Issuer"],
            ValidateAudience = true,
            ValidAudience = jwtSettings["Audience"],
            ValidateLifetime = true,
            ClockSkew = TimeSpan.Zero
        };
    });

// Services
builder.Services.AddHttpContextAccessor();
builder.Services.AddMemoryCache();

// Repository & Unit of Work
builder.Services.AddScoped<IUnitOfWork, UnitOfWork>();
builder.Services.AddScoped(typeof(IRepository<>), typeof(Repository<>));

// Application Services
builder.Services.AddScoped<IJwtService, JwtService>();
builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddScoped<ILocalizationService, LocalizationService>();
builder.Services.AddScoped<IBranchService, BranchService>();
// Type'lar artik static class olarak tanimli: AddressTypes, VendorStatuses, etc.
builder.Services.AddScoped<IVendorService, VendorService>();
builder.Services.AddScoped<ITeamService, TeamService>();
builder.Services.AddScoped<ICompanyService, CompanyService>();
builder.Services.AddScoped<ICapabilityRequestService, CapabilityRequestService>();
builder.Services.AddScoped<ICapabilityProfileService, CapabilityProfileService>();
builder.Services.AddScoped<IRegistrationService, RegistrationService>();
builder.Services.AddScoped<IAdminService, AdminService>();
builder.Services.AddScoped<IProductService, ProductService>();
builder.Services.AddScoped<IWarehouseService, WarehouseService>();
builder.Services.AddScoped<ICategoryRequestService, CategoryRequestService>();
builder.Services.AddScoped<ICategorySeedService, CategorySeedService>();
builder.Services.AddScoped<GeographySeedService>();
builder.Services.AddScoped<IDemandService, DemandService>();
builder.Services.AddScoped<INegotiationService, NegotiationService>();
builder.Services.AddScoped<ISupplierProfileService, SupplierProfileService>();
builder.Services.AddScoped<INotificationService, NotificationService>();
builder.Services.AddScoped<IProductInquiryService, ProductInquiryService>();
builder.Services.AddScoped<IOrderService, OrderService>();
builder.Services.AddScoped<IStripeService, StripeService>();
builder.Services.AddScoped<IProposalService, ProposalService>();
builder.Services.AddScoped<ICatalogService, CatalogService>();
builder.Services.AddScoped<IBuyerService, BuyerService>();
builder.Services.AddScoped<IStockService, StockService>();
builder.Services.AddScoped<IMessageService, MessageService>();
builder.Services.AddScoped<IReportService, ReportService>();
builder.Services.AddScoped<IAccountService, AccountService>();
builder.Services.AddScoped<ICartService, CartService>();
builder.Services.AddScoped<ICheckoutService, CheckoutService>();
builder.Services.AddScoped<IDashboardService, DashboardService>();
builder.Services.AddScoped<IProductReviewService, ProductReviewService>();
builder.Services.AddScoped<IOrderOrchestrationService, OrderOrchestrationService>();
builder.Services.AddScoped<IRiskScoringService, RiskScoringService>();
builder.Services.AddScoped<IWaitlistService, WaitlistService>();

// Evrim API Configuration
builder.Services.Configure<EvrimApiSettings>(
    builder.Configuration.GetSection("EvrimApi"));
builder.Services.AddScoped<IEvrimApiService, EvrimApiService>();
builder.Services.AddScoped<ISocialFeedService, SocialFeedService>();
builder.Services.AddScoped<ISponsoredPostService, SponsoredPostService>();
builder.Services.AddScoped<IAuctionService, AuctionService>();
builder.Services.AddScoped<IRewardPointService, RewardPointService>();
builder.Services.AddScoped<IBackInStockService, BackInStockService>();

// Evrim API HttpClient
builder.Services.AddHttpClient("EvrimApi", client =>
{
    client.Timeout = TimeSpan.FromSeconds(30);
    client.DefaultRequestHeaders.Add("Accept", "application/json");
});

// Background Services
builder.Services.AddHostedService<CustomsDeclarationStatusCheckerService>();

// HttpClient for external API calls
builder.Services.AddHttpClient("ExternalApi", client =>
{
    client.Timeout = TimeSpan.FromMinutes(10);
    client.DefaultRequestHeaders.Add("User-Agent", "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36");
    client.DefaultRequestHeaders.Add("Accept", "*/*");
}).ConfigurePrimaryHttpMessageHandler(() => new HttpClientHandler
{
    AllowAutoRedirect = true,
    MaxAutomaticRedirections = 5
});

builder.Services.AddHttpClient();

// Route Constraints
builder.Services.AddRouting(options =>
{
    options.ConstraintMap.Add("companySlug", typeof(Bridgo.Middleware.CompanySlugRouteConstraint));
});

// MVC
builder.Services.AddControllersWithViews();

// SignalR
builder.Services.AddSignalR();

// Authorization Policies - Dual auth: Cookie + JWT Bearer
builder.Services.AddAuthorization(options =>
{
    // Default policy: Cookie veya Bearer ile auth olabilir
    options.DefaultPolicy = new AuthorizationPolicyBuilder(
        IdentityConstants.ApplicationScheme,
        JwtBearerDefaults.AuthenticationScheme)
        .RequireAuthenticatedUser()
        .Build();

    options.AddPolicy("SystemAdmin", policy =>
    {
        policy.AddAuthenticationSchemes(
            IdentityConstants.ApplicationScheme,
            JwtBearerDefaults.AuthenticationScheme);
        policy.RequireAssertion(context =>
        {
            var isSystemAdminClaim = context.User.FindFirst("IsSystemAdmin");
            return isSystemAdminClaim?.Value == "true";
        });
    });
});

// Capability-based Authorization
builder.Services.AddSingleton<IAuthorizationHandler, Bridgo.Authorization.CapabilityAuthorizationHandler>();
builder.Services.AddSingleton<IAuthorizationPolicyProvider, Bridgo.Authorization.CapabilityPolicyProvider>();

// CORS - Landing page (corplynk.com) icin
builder.Services.AddCors(options =>
{
    options.AddPolicy("LandingPage", policy =>
    {
        policy.WithOrigins(
                "https://corplynk.com",
                "https://www.corplynk.com",
                "https://corplynk-proxy.corplynkcmon.workers.dev",
                "https://storage.googleapis.com")
              .AllowAnyHeader()
              .WithMethods("POST", "GET");
    });
});

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseRouting();
app.UseCors("LandingPage");


app.UseAuthentication();
app.UseAuthorization();

// VendorId gereklilik kontrolu
// Admin haric tum kullanicilar VendorId olmadan sisteme giremez
app.UseVendorRequired();

app.MapStaticAssets();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}")
    .WithStaticAssets();

// SignalR Hub
app.MapHub<Bridgo.Hubs.NotificationHub>("/hubs/notifications");

// Seed data
using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    try
    {
        var context = services.GetRequiredService<ApplicationDbContext>();
        var userManager = services.GetRequiredService<UserManager<ApplicationUser>>();
        var roleManager = services.GetRequiredService<RoleManager<ApplicationRole>>();

        // Migration'lari otomatik uygula (development icin)
        if (app.Environment.IsDevelopment())
        {
            await context.Database.MigrateAsync();
        }

        // Default roller
        if (!await roleManager.RoleExistsAsync("Admin"))
        {
            await roleManager.CreateAsync(new ApplicationRole
            {
                Name = "Admin",
                Description = "Sistem yoneticisi"
            });
        }

        // User rolu - tum kayitli kullanicilar icin
        if (!await roleManager.RoleExistsAsync("User"))
        {
            await roleManager.CreateAsync(new ApplicationRole
            {
                Name = "User",
                Description = "Kayitli kullanici"
            });
        }

        // Localization seed (Diller + Ceviri kaynaklari)
        // XML dosyalarindan import edilir (App_Data/Localization/*.xml)
        var contentRoot = app.Environment.ContentRootPath;
        await LocalizationSeed.SeedLocalizationAsync(context, contentRoot);

        // Type'lar artik code-based (ITypeResolver.cs): AddressTypes, VendorStatuses, etc.

        // RBAC seed (Capability, Module, Role, Permission)
        await RbacSeeder.SeedAsync(context);

        // Default admin kullanici (VendorId olmadan - sistem admini)
        var adminEmail = "admin@bridgo.com";
        var adminUser = await userManager.FindByEmailAsync(adminEmail);
        if (adminUser == null)
        {
            Log.Information("Admin kullanici bulunamadi, yeni olusturuluyor...");
            adminUser = new ApplicationUser
            {
                UserName = adminEmail,
                Email = adminEmail,
                FirstName = "Admin",
                LastName = "User",
                EmailConfirmed = true,
                IsActive = true,
                IsSystemAdmin = true,
                VendorId = null  // Sistem admini - vendor'a bagli degil
            };

            var result = await userManager.CreateAsync(adminUser, "Admin123!");
            if (result.Succeeded)
            {
                await userManager.AddToRoleAsync(adminUser, "User");
                await userManager.AddToRoleAsync(adminUser, "Admin");
                Log.Information("Admin kullanici basariyla olusturuldu.");
            }
            else
            {
                Log.Error("Admin kullanici olusturulamadi: {Errors}", string.Join(", ", result.Errors.Select(e => e.Description)));
            }
        }
        else
        {
            Log.Information("Admin kullanici bulundu (Id: {Id}), sifre resetleniyor...", adminUser.Id);
            // Admin varsa şifreyi resetle
            var token = await userManager.GeneratePasswordResetTokenAsync(adminUser);
            var resetResult = await userManager.ResetPasswordAsync(adminUser, token, "Admin123!");
            if (resetResult.Succeeded)
            {
                adminUser.IsActive = true;
                adminUser.IsSystemAdmin = true;
                await userManager.UpdateAsync(adminUser);
                Log.Information("Admin sifre basariyla resetlendi.");
            }
            else
            {
                Log.Error("Admin sifre resetlenemedi: {Errors}", string.Join(", ", resetResult.Errors.Select(e => e.Description)));
            }
        }
    }
    catch (Exception ex)
    {
        Log.Error(ex, "Seed islemi sirasinda bir hata olustu.");
    }
}

app.Run();
}
catch (Exception ex)
{
    Log.Fatal(ex, "Uygulama beklenmedik bir hata ile sonlandi.");
}
finally
{
    Log.CloseAndFlush();
}
