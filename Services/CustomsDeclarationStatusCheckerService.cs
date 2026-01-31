using Microsoft.Extensions.Options;
using Bridgo.Configuration;
using Bridgo.Services.Interfaces;

namespace Bridgo.Services;

/// <summary>
/// Gumruk beyannamesi durum kontrolu icin background service
/// Belirli araliklarla aktif beyannamelerin durumunu Evrim'den sorgular
/// </summary>
public class CustomsDeclarationStatusCheckerService : BackgroundService
{
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<CustomsDeclarationStatusCheckerService> _logger;
    private readonly EvrimApiSettings _settings;

    public CustomsDeclarationStatusCheckerService(
        IServiceProvider serviceProvider,
        IOptions<EvrimApiSettings> settings,
        ILogger<CustomsDeclarationStatusCheckerService> logger)
    {
        _serviceProvider = serviceProvider;
        _settings = settings.Value;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("CustomsDeclarationStatusChecker servisi baslatildi");

        // Polling aktif degilse hic calisma
        if (!_settings.EnableStatusPolling)
        {
            _logger.LogInformation("Beyanname durum polling devre disi - EnableStatusPolling: false");
            return;
        }

        // Ilk calismadan once biraz bekle (uygulama tam ayaga kalksin)
        await Task.Delay(TimeSpan.FromMinutes(2), stoppingToken);

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                _logger.LogInformation("Beyanname durum senkronizasyonu basliyor...");

                using (var scope = _serviceProvider.CreateScope())
                {
                    var evrimService = scope.ServiceProvider.GetRequiredService<IEvrimApiService>();
                    await evrimService.SyncAllPendingDeclarationsAsync();
                }

                _logger.LogInformation("Beyanname durum senkronizasyonu tamamlandi");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Beyanname durum senkronizasyonu sirasinda hata olustu");
            }

            // Sonraki calisma icin bekle
            await Task.Delay(TimeSpan.FromMinutes(_settings.StatusPollingIntervalMinutes), stoppingToken);
        }

        _logger.LogInformation("CustomsDeclarationStatusChecker servisi durduruluyor");
    }

    public override Task StopAsync(CancellationToken cancellationToken)
    {
        _logger.LogInformation("CustomsDeclarationStatusChecker servisi durduruluyor");
        return base.StopAsync(cancellationToken);
    }
}
