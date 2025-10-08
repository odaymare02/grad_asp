using Graduate.BLL.Services.Interfaces;
using Graduate.DAL.Dto.Request;
using Graduate.DAL.Dto.Response;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace Graduation_project.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class ChatsController : ControllerBase
    {
        private readonly IChatService _chatService;

        public ChatsController(IChatService chatService)
        {
            _chatService = chatService;
        }
        [HttpPost("create-session")]
        public async Task<IActionResult> CreateSession()
        {
            try
            {
                var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
                var session = await _chatService.CreateSessionAsync(userId);
                return Ok(ApiResponse<ChatSessionDto>.SuccessResponse(session, "تم إنشاء المحادثة بنجاح"));
            }
            catch (Exception ex)
            {
                return StatusCode(500, ApiResponse<ChatSessionDto>.FailResponse($"حدث خطأ أثناء إنشاء المحادثة: {ex.Message}"));
            }
        }
        [HttpGet("sessions/{id}")]
        public async Task<IActionResult> GetSession(int id)
        {
            try
            {
                var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
                var session = await _chatService.GetSessionMessagesAsync(id, userId);
                if (session == null)
                    return NotFound(ApiResponse<ChatSessionDto>.FailResponse("المحادثة غير موجودة"));

                return Ok(ApiResponse<ChatSessionDto>.SuccessResponse(session));
            }
            catch (Exception ex)
            {
                return StatusCode(500, ApiResponse<ChatSessionDto>.FailResponse($"حدث خطأ أثناء جلب المحادثة: {ex.Message}"));
            }
        }
        [HttpPut("sessions/{id}/rename")]
        public async Task<IActionResult> RenameSession(int id, [FromBody] RenameSession dto)
        {
            try
            {
                var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
                var result = await _chatService.RenameSessionAsync(id, dto.NewTitle, userId);
                if (!result)
                    return NotFound(ApiResponse<object>.FailResponse("المحادثة غير موجودة أو لا يمكنك تعديلها"));

                return Ok(ApiResponse<object>.SuccessResponse(null, "تم تعديل اسم المحادثة بنجاح"));
            }
            catch (Exception ex)
            {
                return StatusCode(500, ApiResponse<object>.FailResponse($"حدث خطأ أثناء تعديل المحادثة: {ex.Message}"));
            }
        }

        [HttpDelete("sessions/{id}")]
        public async Task<IActionResult> DeleteSession(int id)
        {
            try
            {
                var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
                var result = await _chatService.DeleteSessionAsync(id, userId);
                if (!result)
                    return NotFound(ApiResponse<object>.FailResponse("المحادثة غير موجودة أو لا يمكنك حذفها"));

                return Ok(ApiResponse<object>.SuccessResponse(null, "تم حذف المحادثة بنجاح"));
            }
            catch (Exception ex)
            {
                return StatusCode(500, ApiResponse<object>.FailResponse($"حدث خطأ أثناء حذف المحادثة: {ex.Message}"));
            }
        }
        [HttpPost("send-message")]
        public async Task<IActionResult> AddMessage([FromBody] ChatRequest request)
        {
            try
            {
                var message = await _chatService.AddMessageAsync(request);  
                return Ok(ApiResponse<ChatMessageDto>.SuccessResponse(message));
            }catch(Exception ex)
            {
                return StatusCode(500, ApiResponse<ChatMessageDto>.FailResponse($"حدث خطأ: {ex.Message}"));
            }
        }
        [HttpGet("sessions")]
        public async Task<IActionResult> GetUserSessions()
        {
            try
            {
                var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
                var sessions = await _chatService.GetUserSessionsAsync(userId);
                return Ok(ApiResponse<IEnumerable<ChatSessionDto>>.SuccessResponse(sessions));
            }
            catch (Exception ex)
            {
                return StatusCode(500, ApiResponse<IEnumerable<ChatSessionDto>>.FailResponse($"حدث خطأ أثناء جلب المحادثات: {ex.Message}"));
            }
        }
        [HttpGet("messages/{sessionId}")]
        public async Task<IActionResult> GetSessionMessages(int sessionId)
        {
            try
            {
                var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
                var messages = await _chatService.GetSessionMessagesAsync(sessionId, userId);
                if (messages == null)
                    return NotFound(ApiResponse<ChatSessionDto>.FailResponse("المحادثة غير موجودة"));

                return Ok(ApiResponse<ChatSessionDto>.SuccessResponse(messages));
            }
            catch (Exception ex)
            {
                return StatusCode(500, ApiResponse<ChatSessionDto>.FailResponse($"حدث خطأ أثناء جلب الرسائل: {ex.Message}"));
            }
        }
        [HttpGet("messages/search")]
        public async Task<IActionResult> SearchMessages([FromQuery] string query, [FromQuery] string? major)
        {
            try
            {
                var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
                var messages = await _chatService.SearchMessagesAsync(userId, query, major);
                return Ok(ApiResponse<IEnumerable<ChatMessageDto>>.SuccessResponse(messages));
            }
            catch (Exception ex)
            {
                return StatusCode(500, ApiResponse<IEnumerable<ChatMessageDto>>.FailResponse($"حدث خطأ أثناء البحث عن الرسائل: {ex.Message}"));
            }
        }
        [HttpGet("{sessionId}/export")]
        public async Task<IActionResult> ExportSession(int sessionId)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;
            var fileBytes = await _chatService.ExportSessionAsync(sessionId, userId);

            if (fileBytes.Length == 0)
                return NotFound("Session not found.");

            return File(fileBytes, "application/pdf", $"session_{sessionId}.pdf");
        }
        [HttpGet("user-stats")]
        public async Task<IActionResult> GetUserStats()
        {
            // Get the currently logged-in user's ID
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId))
            {
                return Unauthorized();
            }

            var stats = await _chatService.GetUserStatsAsync(userId);
            return Ok(stats);
        }
    }
}
