using System.Text.Json;
using Karandash.Authentication.DataAccess.Contexts;
using Karandash.Shared.Utils.Infrastructure;
using Karandash.Shared.Utils.Methods;
using Microsoft.EntityFrameworkCore;

namespace Karandash.Authentication.API.BackgroundServices;

public class OutboxProcessorHostedService(
    IServiceProvider serviceProvider,
    ILogger<OutboxProcessorHostedService> logger)
    : BackgroundService
{
    private readonly IServiceProvider _serviceProvider = serviceProvider;
    private readonly ILogger<OutboxProcessorHostedService> _logger = logger;

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                using IServiceScope scope = _serviceProvider.CreateScope();
                AuthenticationDbContext dbContext = scope.ServiceProvider.GetRequiredService<AuthenticationDbContext>();
                EmailService emailService = scope.ServiceProvider.GetRequiredService<EmailService>();

                List<OutboxEvent> pendingEvents = await dbContext.OutboxEvents
                    .Where(e => e.ProcessedAt == null && e.RetryCount < 5)
                    .OrderBy(e => e.CreatedAt)
                    .Take(10)
                    .ToListAsync(stoppingToken);

                foreach (OutboxEvent evt in pendingEvents)
                {
                    try
                    {
                        if (evt.Type == "UserRegisteredEvent")
                        {
                            UserRegisteredPayload payload =
                                JsonSerializer.Deserialize<UserRegisteredPayload>(evt.Payload)!;
                            emailService.SendRegistrationEmail(payload.Email, payload.FullName, payload.Language);
                        }

                        evt.ProcessedAt = DateTime.UtcNow;
                        _logger.LogInformation("Processed event {EventId}", evt.Id);
                    }
                    catch (Exception ex)
                    {
                        evt.RetryCount++;
                        _logger.LogError(ex, "Error processing event {EventId}", evt.Id);
                    }
                }

                await dbContext.SaveChangesAsync(stoppingToken);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Outbox processing cycle failed");
            }

            await Task.Delay(TimeSpan.FromSeconds(10), stoppingToken);
        }
    }

    private record UserRegisteredPayload(string Email, string FullName, string Language);
}