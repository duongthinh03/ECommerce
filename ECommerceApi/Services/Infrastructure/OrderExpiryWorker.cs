using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace ECommerceApi.Services;

// Job nền: mỗi phút quét & tự hủy đơn chuyển khoản quá hạn chưa thanh toán (hoàn kho + coupon).
public class OrderExpiryWorker(IServiceScopeFactory scopeFactory, ILogger<OrderExpiryWorker> logger)
    : BackgroundService
{
    private static readonly TimeSpan Interval = TimeSpan.FromMinutes(1);

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        using var timer = new PeriodicTimer(Interval);
        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                // OrderService là Scoped → tạo scope riêng mỗi lần chạy.
                // CreateAsyncScope + await using: UnitOfWork chỉ có IAsyncDisposable nên phải dispose async.
                await using var scope = scopeFactory.CreateAsyncScope();
                var orderService = scope.ServiceProvider.GetRequiredService<IOrderService>();
                await orderService.CancelExpiredUnpaidOrdersAsync();
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Lỗi khi tự hủy đơn quá hạn");
            }

            try { await timer.WaitForNextTickAsync(stoppingToken); }
            catch (OperationCanceledException) { break; }   // app tắt → thoát êm
        }
    }
}
