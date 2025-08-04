namespace SwaggerDemo.Interfaces
{
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using SwaggerDemo.Models;

    public interface IMessageRepository
    {
        Task AddAsync(MessageDto message);
        Task UpdateStatusAsync(MessageDto message);
        Task<List<MessageDto>> GetAllAsync();
        Task<List<MessageDto>> GetDeadMessagesAsync();
    }
}
