using EquipmentBorrowingManagementSystem.Application.Interfaces.Services;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace EquipmentBorrowingManagementSystem.Infrastructure.HostedServices;

public class BorrowRequestExpirationHostedService : BackgroundService
{
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly ILogger<BorrowRequestExpirationHostedService> _logger;

    public BorrowRequestExpirationHostedService(
        IServiceScopeFactory scopeFactory,
        ILogger<BorrowRequestExpirationHostedService> logger)
    {
        _scopeFactory = scopeFactory;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                using var scope = _scopeFactory.CreateScope();
                var borrowService = scope.ServiceProvider.GetRequiredService<IBorrowRequestService>();
                await borrowService.ProcessExpiredApprovalsAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to process expired borrow approvals.");
            }

            await Task.Delay(TimeSpan.FromHours(1), stoppingToken);
        }
    }
}
