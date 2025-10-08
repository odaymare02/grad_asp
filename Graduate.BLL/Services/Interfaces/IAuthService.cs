using Graduate.DAL.Dto.Request;
using Graduate.DAL.Dto.Response;
using Microsoft.AspNetCore.Http;
//using Microsoft.AspNetCore.Identity.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Graduate.BLL.Services.Interfaces
{
    public interface IAuthService
    {
        Task<ApiResponse<UserResponse>> RegisterAsync(RegisterRequest request,HttpRequest Req);
        Task<ApiResponse<UserResponse>> ConfirmEmail(string token,string userId);
        Task<ApiResponse<UserResponse>> ResendConfirmEmail(string email, HttpRequest httpRequest);

        Task<ApiResponse<UserResponse>> LoginAsync(LoginRequest request);
        Task<ApiResponse<object>> ForgetPassword(ForgetPasswordRequest request);
        Task<ApiResponse<object>> ResetPassword(ResetPasswordRequest request);
        Task<ApiResponse<object>> ChangePassword(string userId, ChangePasswordRequest request);






    }
}
