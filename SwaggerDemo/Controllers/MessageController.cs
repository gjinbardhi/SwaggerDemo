using Microsoft.AspNetCore.Mvc;
using System;
using System.Linq;
using System.Threading.Tasks;
using SwaggerDemo.Services;
using System.Collections.Generic;
using SwaggerDemo.Models;
using Microsoft.Extensions.Logging;
using SwaggerDemo.Services;
using SwaggerDemo.Interfaces;

namespace SwaggerDemo.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class MessagesController : ControllerBase
    {
        private readonly IMessageRepository _repository;
        private readonly ILogger<MessagesController> _logger;

        public MessagesController(
            IMessageRepository repository,     // <-- interface here
            ILogger<MessagesController> logger)
        {
            _repository = repository;
            _logger = logger;
        }

        // POST /api/messages
        [HttpPost]
        public async Task<IActionResult> SendMessage([FromBody] MessageDto message)
        {
            try
            {
                message.Id = Guid.NewGuid().ToString();
                message.Status = "Pending";
                message.RetryCount = 0;
                message.MaxRetries = message.MaxRetries == 0 ? 3 : message.MaxRetries;

                await _repository.AddAsync(message);

                _logger.LogInformation("📨 Received new message for {Recipient} of type {Type}. ID: {Id}", message.Recipient, message.Type, message.Id);

                return Ok(new { message = "Message received", data = message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "❌ Failed to add message.");
                return StatusCode(500, new { error = "Failed to add message." });
            }
        }

        // GET /api/messages/status?id={guid}
        [HttpGet("status")]
        public async Task<IActionResult> GetStatus([FromQuery] string id)
        {
            try
            {
                var allMessages = await _repository.GetAllAsync();
                var message = allMessages.FirstOrDefault(m => m.Id == id);

                if (message == null)
                {
                    _logger.LogWarning("🔍 Status check failed. No message found with ID {Id}", id);
                    return NotFound(new { message = "Message not found" });
                }

                _logger.LogInformation("📊 Status for message {Id}: {Status}", message.Id, message.Status);
                return Ok(new { id = message.Id, status = message.Status });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "❌ Failed to fetch status for message {Id}", id);
                return StatusCode(500, new { error = "Failed to fetch message status." });
            }
        }

        // GET /api/messages/stats
        [HttpGet("stats")]
        public async Task<IActionResult> GetStats()
        {
            try
            {
                var allMessages = await _repository.GetAllAsync();

                var stats = allMessages
                    .GroupBy(m => m.Status)
                    .ToDictionary(g => g.Key, g => g.Count());

                _logger.LogInformation("📈 Message stats: {@Stats}", stats);
                return Ok(stats);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "❌ Failed to generate message stats.");
                return StatusCode(500, new { error = "Failed to generate message stats." });
            }
        }

        // GET /api/messages/dead
        [HttpGet("dead")]
        public async Task<IActionResult> GetDeadMessages()
        {
            try
            {
                var dead = await _repository.GetDeadMessagesAsync();
                _logger.LogInformation("💀 Found {Count} dead messages", dead.Count);
                return Ok(dead);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "❌ Failed to retrieve dead messages.");
                return StatusCode(500, new { error = "Failed to retrieve dead messages." });
            }
        }
    }
}
