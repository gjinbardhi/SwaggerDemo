using System;
using System.Collections.Generic;
using System.Reflection;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Moq;
using SwaggerDemo.Controllers;
using SwaggerDemo.Interfaces;
using SwaggerDemo.Models;
using Xunit;

namespace SwaggerDemo.Tests.Controllers
{
    public class MessagesControllerTests
    {
        private readonly Mock<IMessageRepository> _repo;
        private readonly Mock<ILogger<MessagesController>> _log;
        private readonly MessagesController _ctrl;

        public MessagesControllerTests()
        {
            _repo = new Mock<IMessageRepository>();
            _log  = new Mock<ILogger<MessagesController>>();
            _ctrl = new MessagesController(_repo.Object, _log.Object);
        }

        [Fact]
        public async Task SendMessage_ReturnsOk_WithNewMessage()
        {
            // Arrange
            var dto = new MessageDto { Type = "SMS", Recipient = "123", Content = "hi", MaxRetries = 2 };

            // Act
            var result = await _ctrl.SendMessage(dto);

            // Assert
            var ok      = Assert.IsType<OkObjectResult>(result);
            var wrapper = ok.Value;
            // grab the "data" property
            var dataProp = wrapper.GetType().GetProperty("data", BindingFlags.Public | BindingFlags.Instance);
            Assert.NotNull(dataProp);
            var msg = Assert.IsType<MessageDto>(dataProp.GetValue(wrapper)!);
            Assert.Equal("Pending", msg.Status);
            _repo.Verify(r => r.AddAsync(It.IsAny<MessageDto>()), Times.Once);
        }

        [Fact]
        public async Task GetStatus_MessageNotFound_Returns404()
        {
            _repo.Setup(r => r.GetAllAsync()).ReturnsAsync(new List<MessageDto>());

            var result = await _ctrl.GetStatus("no-such-id");
            Assert.IsType<NotFoundObjectResult>(result);
        }

        [Fact]
        public async Task GetStatus_MessageExists_Returns200WithStatus()
        {
            // Arrange
            var msgDto = new MessageDto { Id = "42", Status = "Sent" };
            _repo.Setup(r => r.GetAllAsync()).ReturnsAsync(new List<MessageDto> { msgDto });

            // Act
            var result = await _ctrl.GetStatus("42");

            // Assert
            var ok      = Assert.IsType<OkObjectResult>(result);
            var wrapper = ok.Value;
            var statusProp = wrapper.GetType().GetProperty("status", BindingFlags.Public | BindingFlags.Instance);
            Assert.NotNull(statusProp);
            var status = Assert.IsType<string>(statusProp.GetValue(wrapper)!);
            Assert.Equal("Sent", status);
        }

        [Fact]
        public async Task GetStats_ReturnsCountsGroupedByStatus()
        {
            var list = new List<MessageDto>
            {
                new MessageDto { Status = "Sent" },
                new MessageDto { Status = "Failed" },
                new MessageDto { Status = "Sent" }
            };
            _repo.Setup(r => r.GetAllAsync()).ReturnsAsync(list);

            var result = await _ctrl.GetStats();

            var ok   = Assert.IsType<OkObjectResult>(result);
            var dict = Assert.IsType<Dictionary<string,int>>(ok.Value);
            Assert.Equal(2, dict["Sent"]);
            Assert.Equal(1, dict["Failed"]);
        }

        [Fact]
        public async Task GetDeadMessages_UsesRepository_AndReturnsOk()
        {
            var dead = new List<MessageDto> { new() { Id = "x", Status = "Dead" } };
            _repo.Setup(r => r.GetDeadMessagesAsync()).ReturnsAsync(dead);

            var result = await _ctrl.GetDeadMessages();

            var ok       = Assert.IsType<OkObjectResult>(result);
            var returned = Assert.IsType<List<MessageDto>>(ok.Value);
            Assert.Single(returned);
            Assert.Equal("Dead", returned[0].Status);
        }
    }
}
