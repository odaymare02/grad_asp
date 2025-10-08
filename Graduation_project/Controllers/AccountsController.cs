using Graduate.BLL.Services.Interfaces;
using Graduate.DAL.Dto.Request;
using Graduate.DAL.Dto.Response;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
//using Microsoft.AspNetCore.Identity.Data;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace Graduation_project.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AccountsController : ControllerBase
    {
        private readonly IAuthService _authService;

        public AccountsController(IAuthService authService)
        {
            _authService = authService;
        }
        [HttpPost("Register")]
        public async Task<IActionResult> Register(RegisterRequest request)
        {
            var result = await _authService.RegisterAsync(request, Request);
            return result.Success ? Ok(result) : BadRequest(result);
        }

        [HttpGet("ConfirmEmail")]
        public async Task<IActionResult> ConfirmEmail([FromQuery] string token, [FromQuery] string userId)
        {
            var result = await _authService.ConfirmEmail(token, userId);
            return result.Success ? Ok(result) : BadRequest(result);
        }
        [HttpPost("resend-confirmation")]
        public async Task<IActionResult> ResendConfirmation([FromBody] EmailRequest request)
        {
            var message = await _authService.ResendConfirmEmail(request.Email, Request);
            return Ok(message);
        }

        [HttpPost("Login")]
        public async Task<IActionResult> Login(LoginRequest request)
        {
            var result = await _authService.LoginAsync(request);
            return result.Success ? Ok(result) : Unauthorized(result);
        }

        [HttpPost("ForgetPassword")]
        public async Task<IActionResult> ForgetPassword([FromBody] ForgetPasswordRequest request)
        {
            var result = await _authService.ForgetPassword(request);
            return result.Success ? Ok(result) : BadRequest(result);
        }

        [HttpPatch("ResetPassword")]
        public async Task<IActionResult> ResetPassword([FromBody] ResetPasswordRequest request)
        {
            var result = await _authService.ResetPassword(request);
            return result.Success ? Ok(result) : BadRequest(result);
        }
        [HttpPatch("ChangePassword")]
        [Authorize] // لازم يكون المستخدم مسجل دخول
        public async Task<IActionResult> ChangePassword([FromBody] ChangePasswordRequest request)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId))
                return Unauthorized(new ApiResponse<object> { Success = false, Message = "User not authenticated" });

            var result = await _authService.ChangePassword(userId, request);
            return result.Success ? Ok(result) : BadRequest(result);
        }
    }
}
