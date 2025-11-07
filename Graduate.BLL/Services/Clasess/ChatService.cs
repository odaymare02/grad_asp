using Graduate.BLL.Services.Interfaces;
using Graduate.DAL.Data;
using Graduate.DAL.Dto.Request;
using Graduate.DAL.Models;
using iTextSharp.text;
using iTextSharp.text.pdf;
using Microsoft.EntityFrameworkCore;
using MigraDoc.DocumentObjectModel;
using MigraDoc.Rendering;
using System.Drawing.Text;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using Graduate.DAL.Dto.Response;

namespace Graduate.BLL.Services.Clasess
{
    public class ChatService : IChatService
    {
        private readonly ApplicationDbContext _context;
        private readonly RagService _ragService;

        public ChatService(ApplicationDbContext context,RagService ragService)
        {
            _context = context;
            _ragService = ragService;
        }
        public async Task<ChatMessageDto> AddMessageAsync(ChatRequest request)
        {
           
            var userMessage = new ChatMessage
            {
                ChatSessionId = request.SessionId,
                Role = request.Role,
                Content = request.Content,
                CreatedAt = DateTime.UtcNow,
                Major=request.Major
            };
            _context.ChatMessages.Add(userMessage);

            var session = await _context.ChatSessions.FindAsync(request.SessionId);
            if (session != null && session.Title == "محادثة جديدة" && request.Role == "user")
            {
                session.Title = request.Content.Length > 30
                    ? request.Content.Substring(0, 30) + "..."
                    : request.Content;
            }

            await _context.SaveChangesAsync();

            
            ChatMessage botMessage = null;
            if (request.Role == "user")
            {
                if (string.IsNullOrEmpty(request.Major))
                    request.Major = "عام";
                var completedCourses = request.CompletedCourses ?? new List<string>();

                var ragResponse = await _ragService.GetRagResponseAsync(request.Content, request.Major,completedCourses);
                botMessage = new ChatMessage
                {
                    ChatSessionId = request.SessionId,
                    Role = "bot",
                    Content = ragResponse?.Result ?? "⚠️ لم يتمكن النظام من توليد رد.",
                    Major = request.Major
                };

                _context.ChatMessages.Add(botMessage);
                await _context.SaveChangesAsync();

                return new ChatMessageDto
                {
                    Id = botMessage.ChatMessageId,
                    Role = botMessage.Role,
                    ChatSessionId = botMessage.ChatSessionId,
                    Content = botMessage.Content,
                    BotResponse = ragResponse,
                    CreatedAt = botMessage.CreatedAt,
                    Major = botMessage.Major
                };
            }

            return new ChatMessageDto
            {
                Id = userMessage.ChatMessageId,
                Role = userMessage.Role,
                ChatSessionId = userMessage.ChatSessionId,
                Content = userMessage.Content
            };
        }

        public async Task<ChatSessionDto> CreateSessionAsync(string userId)
        {
            var s = new ChatSession()
            {
                UserId = userId,
                Title = "محادثة جديدة",
                CreatedAt = DateTime.UtcNow
            };

            _context.ChatSessions.Add(s);
            await _context.SaveChangesAsync();

            return new ChatSessionDto
            {
                Id = s.ChatSessionId,
                Title = s.Title,
                CreatedAt = s.CreatedAt,
                Messages = new List<ChatMessageDto>()
            };
        }

        public async Task<ChatSessionDto?> GetSessionMessagesAsync(int sessionId, string userId)
        {
            var session = await _context.ChatSessions
            .Include(s => s.Messages)
            .FirstOrDefaultAsync(s => s.ChatSessionId == sessionId && s.UserId == userId);

            if (session == null)
                return null;

            return new ChatSessionDto
            {
                Id = session.ChatSessionId,
                Title = session.Title,
                CreatedAt = session.CreatedAt,
                Messages = session.Messages
           .OrderBy(m => m.CreatedAt)
           .Select(m => new ChatMessageDto
           {
               Id = m.ChatMessageId,
               Role = m.Role,
               Content = m.Content,
               ChatSessionId=session.ChatSessionId,
               CreatedAt = m.CreatedAt,
               Major=m.Major
           }).ToList()
            };
        }

        public async Task<IEnumerable<ChatSessionDto>> GetUserSessionsAsync(string userId)
        {
           var sessions= await _context.ChatSessions
                .Where(s => s.UserId == userId)
                .OrderByDescending(s=>s.CreatedAt)
                .ToListAsync();
            return sessions.Select(s => new ChatSessionDto
            {
                Id = s.ChatSessionId,
                Title = s.Title,
                CreatedAt = s.CreatedAt,
            }).ToList();

        }
        public async Task<bool> RenameSessionAsync(int sessionId, string newTitle, string userId)
        {
            var session = await _context.ChatSessions
                .FirstOrDefaultAsync(s => s.ChatSessionId == sessionId && s.UserId == userId);

            if (session == null) return false;

            session.Title = newTitle;
            await _context.SaveChangesAsync();
            return true;
        }
        public async Task<bool> DeleteSessionAsync(int sessionId, string userId)
        {
            var session = await _context.ChatSessions
                .Include(s => s.Messages)
                .FirstOrDefaultAsync(s => s.ChatSessionId == sessionId && s.UserId == userId);

            if (session == null) return false;

            _context.ChatSessions.Remove(session);
            _context.ChatSessions.Remove(session);
            await _context.SaveChangesAsync();
            return true;
        }
        public async Task<IEnumerable<ChatMessageDto>> SearchMessagesAsync(string userId, string query, string? major)
        {
            var messages = _context.ChatMessages
        .Include(m => m.ChatSession)
        .Where(m => m.ChatSession.UserId == userId &&
                    m.Content.Contains(query));

            if (!string.IsNullOrEmpty(major))
                messages = messages.Where(m => m.Content.Contains(major));

            return await messages
                .Select(m => new ChatMessageDto
                {
                    Id = m.ChatMessageId,
                    Content = m.Content,
                    CreatedAt = m.CreatedAt,
                    ChatSessionId = m.ChatSessionId,
                    Role=m.Role
                })
                .ToListAsync();
        }

        public async Task<byte[]> ExportSessionAsync(int sessionId, string userId)
        {
            var session = await _context.ChatSessions
                .Include(s => s.Messages)
                .FirstOrDefaultAsync(s => s.ChatSessionId == sessionId && s.UserId == userId);

            if (session == null) return Array.Empty<byte>();

            var document = new MigraDoc.DocumentObjectModel.Document();
            var section = document.AddSection();

            
            string fontName = "Scheherazade";

            var titleStyle = document.Styles.AddStyle("TitleStyle", "Normal");
            titleStyle.Font.Name = fontName;
            titleStyle.Font.Size = 16;
            titleStyle.Font.Bold = true;

            var textStyle = document.Styles.AddStyle("TextStyle", "Normal");
            textStyle.Font.Name = fontName;
            textStyle.Font.Size = 12;

            var titleParagraph = section.AddParagraph($"جلسة المحادثة: {session.Title}");
            titleParagraph.Style = "TitleStyle";
            titleParagraph.Format.Alignment = ParagraphAlignment.Center;

            var createdParagraph = section.AddParagraph($"Created At: {session.CreatedAt}");
            createdParagraph.Style = "TextStyle";
            createdParagraph.Format.Alignment = ParagraphAlignment.Left;

            section.AddParagraph("الرسائل / Messages:").Style = "TitleStyle";

            foreach (var msg in session.Messages.OrderBy(m => m.CreatedAt))
            {
                bool isArabic = msg.Content.Any(c => c >= 0x0600 && c <= 0x06FF);

                var rolePara = section.AddParagraph($"{msg.Role.ToUpper()} ({msg.CreatedAt}):");
                rolePara.Style = "TitleStyle";
                rolePara.Format.Alignment = isArabic ? ParagraphAlignment.Right : ParagraphAlignment.Left;

                var contentPara = section.AddParagraph(msg.Content);
                contentPara.Style = "TextStyle";
                contentPara.Format.Alignment = isArabic ? ParagraphAlignment.Right : ParagraphAlignment.Left;

                section.AddParagraph(" ");
            }

            var pdfRenderer = new PdfDocumentRenderer(true); 
            pdfRenderer.Document = document;
            pdfRenderer.RenderDocument();

            using var ms = new MemoryStream();
            pdfRenderer.PdfDocument.Save(ms, false);
            return ms.ToArray();
        }

        public async Task<ChatStatsDto> GetUserStatsAsync(string userId)
        {
            var sessions = await _context.ChatSessions
                .Where(s => s.UserId == userId)
                .Include(s => s.Messages)
                .ToListAsync();

            var totalSessions = sessions.Count;
            var totalMessages = sessions.Sum(s => s.Messages.Count);
            var lastActivity = sessions
                .SelectMany(s => s.Messages)
                .OrderByDescending(m => m.CreatedAt)
                .FirstOrDefault()?.CreatedAt;

            return new ChatStatsDto
            {
                TotalSessions = totalSessions,
                TotalMessages = totalMessages,
                LastActivity = lastActivity ?? DateTime.MinValue
            };
        }
    }
}
