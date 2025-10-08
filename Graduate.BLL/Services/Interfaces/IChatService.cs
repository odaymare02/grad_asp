using Graduate.DAL.Dto.Request;
using Graduate.DAL.Dto.Response;
using Graduate.DAL.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Graduate.BLL.Services.Interfaces
{
    public interface IChatService
    {
        Task<ChatSessionDto> CreateSessionAsync(string userId);
        Task<ChatMessageDto> AddMessageAsync(ChatRequest request);
        Task<IEnumerable<ChatSessionDto>> GetUserSessionsAsync(string userId);
        Task<ChatSessionDto?> GetSessionMessagesAsync(int sessionId,string userId);
        Task<bool> RenameSessionAsync(int sessionId, string newTitle, string userId);
        Task<bool> DeleteSessionAsync(int sessionId, string userId);
        Task<IEnumerable<ChatMessageDto>> SearchMessagesAsync(string userId, string query, string? major);

        //Task<byte[]> ExportSessionAsync(int sessionId, string userId, string format = "pdf");
        Task<byte[]> ExportSessionAsync(int sessionId, string userId);
        Task<ChatStatsDto> GetUserStatsAsync(string userId);



    }
}
