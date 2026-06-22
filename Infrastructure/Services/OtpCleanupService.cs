using Application.Interfaces;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace Infrastructure.Services;

public class OtpCleanupService : BackgroundService
{
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<OtpCleanupService> _logger;

    public OtpCleanupService(IServiceProvider serviceProvider, ILogger<OtpCleanupService> logger)
    {
        _serviceProvider = serviceProvider;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("OTP Cleanup Service is starting.");

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                using (var scope = _serviceProvider.CreateScope())
                {
                    var otpRepository = scope.ServiceProvider.GetRequiredService<IOtpRepository>();
                    await otpRepository.DeleteExpiredOtpsAsync();
                    _logger.LogInformation("Expired OTPs cleaned up at: {time}", DateTimeOffset.Now);

                    var userRepository = scope.ServiceProvider.GetRequiredService<IUserRepository>();
                    await userRepository.DeleteExpiredGuestsAsync(DateTime.UtcNow.AddHours(-2));
                    _logger.LogInformation("Expired guest users cleaned up at: {time}", DateTimeOffset.Now);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while cleaning up expired OTPs and guest users.");
            }

            // Run every hour
            await Task.Delay(TimeSpan.FromHours(1), stoppingToken);
        }

        _logger.LogInformation("OTP Cleanup Service is stopping.");
    }
}
