using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using SwaggerDemo.Models;
using SwaggerDemo.Interfaces;

namespace SwaggerDemo.Services
{
    public class MessageProcessorService : BackgroundService
    {
        private readonly IMessageRepository _repository;
        private readonly ILogger<MessageProcessorService> _logger;
        private readonly int _delayMs;
        private readonly int _batchSize;

        public MessageProcessorService(
            IMessageRepository repository,
            IConfiguration configuration,
            ILogger<MessageProcessorService> logger)
        {
            _repository = repository;
            _logger = logger;
            _delayMs = configuration.GetValue<int>("Throttling:DelayBetweenMessagesMs", 1000);
            _batchSize = configuration.GetValue<int>("Throttling:BatchSize", 5);
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("📡 Message processor started.");

            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    var messagesToProcess = (await _repository.GetAllAsync())
                        .Where(m => (m.Status == "Pending" || m.Status == "Failed") && m.RetryCount < m.MaxRetries)
                        .Take(_batchSize)
                        .ToList();

                    foreach (var message in messagesToProcess)
                    {
                        await Task.Delay(_delayMs, stoppingToken);

                        try
                        {
                            if (message.RetryCount >= message.MaxRetries)
                            {
                                message.Status = "Dead";
                            }
                            else
                            {
                                bool success = Random.Shared.Next(0, 100) < 80;
                                message.Status = success ? "Sent" : "Failed";
                                message.RetryCount++;
                            }

                            await _repository.UpdateStatusAsync(message);
                            _logger.LogInformation(
                                "🔁 Processed: {Id} → {Status} (Retry {RetryCount})",
                                message.Id, message.Status, message.RetryCount
                            );
                        }
                        catch (Exception ex)
                        {
                            _logger.LogError(ex, "❌ Error processing message {Id}", message.Id);
                        }
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "❌ Error retrieving messages from repository.");
                }

                await Task.Delay(3000, stoppingToken); // Wait before next scan
            }

            _logger.LogInformation("🛑 Message processor stopped.");
        }
    }
}
