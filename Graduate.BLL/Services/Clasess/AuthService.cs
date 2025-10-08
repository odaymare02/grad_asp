using Graduate.BLL.Services.Interfaces;
using Graduate.DAL.Dto.Request;
using Graduate.DAL.Dto.Response;
using Graduate.DAL.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;

namespace Graduate.BLL.Services.Clasess
{
    public class AuthService : IAuthService
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IEmailSender _emailSender;
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly IConfiguration _configuration;

        public AuthService(UserManager<ApplicationUser> userManager,
            IEmailSender emailSender,
            SignInManager<ApplicationUser> signInManager,
            IConfiguration configuration)
        {
            _userManager = userManager;
            _emailSender = emailSender;
            _signInManager = signInManager;
            _configuration = configuration;
        }

        public async Task<ApiResponse<UserResponse>> ConfirmEmail(string token, string userId)
        {
            var user = await _userManager.FindByIdAsync(userId);
            if(user is null)
            {
                return new ApiResponse<UserResponse> { Success = false, Message = "user not found" };
            }
            var result = await _userManager.ConfirmEmailAsync(user, token);
            if (!result.Succeeded)
                return new ApiResponse<UserResponse> { Success = false, Message = "failed to confirm email" };

            return new ApiResponse<UserResponse>
            {
                Success = true,
                Message = "تم تاكيد الايميل بنجاح✅",
                Data = new UserResponse { Id = user.Id, Email = user.Email, Username = user.UserName ,Success=true}
            };
        }

        public async Task<ApiResponse<UserResponse>> LoginAsync(LoginRequest request)
        {
            if (!request.Email.EndsWith("@stu.najah.edu"))
                return new ApiResponse<UserResponse> { Success = false, Message = "invalid email format!" };
            var user = await _userManager.FindByEmailAsync(request.Email);
            if (user is null)
                return new ApiResponse<UserResponse> { Success = false, Message = "invalid email or password" };
            var result = await _signInManager.CheckPasswordSignInAsync(user, request.Password, true);

            if (result.IsNotAllowed) { return new ApiResponse<UserResponse> { Success = false, Message = "please confirm your email" }; }
            //if (!result.Succeeded) { return new ApiResponse<UserResponse> { Success = false, Message = result.IsLockedOut ? "your account is temporarily blocked" : "invalid email or password" }; }

            if (result.IsLockedOut)
            {
                var lockoutEnd = await _userManager.GetLockoutEndDateAsync(user);
                var lockoutMsg = lockoutEnd.HasValue
                    ? $"Your account is temporarily blocked until {lockoutEnd.Value.UtcDateTime.ToLocalTime().AddHours(1):f}"
                    : "Your account is temporarily blocked.";
                return new ApiResponse<UserResponse>
                {
                    Success = false,
                    Message = lockoutMsg
                };
            }

            if (!result.Succeeded)
            {
                return new ApiResponse<UserResponse>
                {
                    Success = false,
                    Message = "Invalid email or password"
                };
            }
            return new ApiResponse<UserResponse>
            {
                Success = true,
                Message = "login success",
                Data = new UserResponse
                {
                    Id = user.Id,
                    Email = user.Email,
                    Username = user.UserName,
                    Token = await generateToken(user),
                    Message= "login success✅",
                    Success=true
                }
            };

        }
        private async Task<string> generateToken(ApplicationUser user)
        {
            var Userclaims = new List<Claim>()
            {
                new Claim(ClaimTypes.Email,user.Email),
                new Claim(ClaimTypes.Name,user.UserName),
                new Claim(ClaimTypes.NameIdentifier,user.Id)
            };
            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration["Jwt:SecretKey"]));
            var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
            var token = new JwtSecurityToken(
                claims: Userclaims,
                expires: DateTime.Now.AddDays(1),
                signingCredentials: credentials
                );
            return new JwtSecurityTokenHandler().WriteToken(token);
        }

        public async Task<ApiResponse<UserResponse>> RegisterAsync(RegisterRequest request,HttpRequest httpRequest)
        {
            if (!request.Email.EndsWith("@stu.najah.edu"))
            {
                return new ApiResponse<UserResponse>
                {
                    Success = false,
                    Message = "invalid email format"
                };
            }
            if (String.IsNullOrWhiteSpace(request.UserName))
            {
                var random = new Random();
                var username = $"{request.FirstName.ToLower()}_{request.LastName.ToLower()}{random.Next(0, 9999)}";
                request.UserName = username;
            }
            var user = new ApplicationUser
            {
                Email = request.Email,
                FullName = string.Join(" ", request.FirstName, request.LastName),
                UserName = request.UserName
            };

            var result = await _userManager.CreateAsync(user, request.Password);
            if (!result.Succeeded)
            {
                return new ApiResponse<UserResponse>
                {
                    Success = false,
                    Message = "faliled to create account",
                    Errors = result.Errors.Select(e => e.Description).ToList()
                };
            }

            var tokenEmail = await _userManager.GenerateEmailConfirmationTokenAsync(user);
            var escapedToken = Uri.EscapeDataString(tokenEmail);
            var emailUrl = $"{httpRequest.Scheme}://{httpRequest.Host}/api/Accounts/ConfirmEmail?token={escapedToken}&userId={user.Id}";
            var emailHtml = $@"
<!DOCTYPE html>
<html lang=""ar"">
<head>
    <meta charset=""UTF-8"">
    <meta name=""viewport"" content=""width=device-width, initial-scale=1.0"">
</head>
<body dir=""rtl"" style=""margin:0; padding:0; background-color:#f4f4f4; font-family: 'Segoe UI', Tahoma, Geneva, Verdana, sans-serif; color:#333;"">
    <div style='font-family: Arial, sans-serif; max-width: 600px; margin: auto; padding: 20px; border: 1px solid #ddd; border-radius: 10px; background-color: #f9f9f9; text-align: center;'>
    
    <!-- العنوان -->
    <h2 style='color: #4a90e2;'>تأكيد البريد الإلكتروني</h2>
    
    <!-- الترحيب -->
    <p style='font-size: 16px; color: #333;'>مرحبًا {user.UserName} 👋،</p>
    <p style='font-size: 16px; color: #333;'>شكرًا لانضمامك إلى <b>Askly</b> 🎉</p>
    <p style='font-size: 16px; color: #333;'>لتفعيل حسابك والبدء في استخدام خدماتنا، يرجى الضغط على الزر أدناه:</p>

    <!-- زر التأكيد -->
    <a href='{emailUrl}' 
       style='display:inline-block; padding:12px 28px; margin:20px 0; background-color:#4a90e2; color:#fff; font-size:16px; font-weight:bold; text-decoration:none; border-radius:6px;'>
       ✅ تأكيد البريد الإلكتروني
    </a>
    <!-- الفوتر -->
    <p style='font-size: 12px; color: #999; line-height:1.6;'>
        © 2025 <b>Askly</b> – جميع الحقوق محفوظة.<br>
        في حال وجود أي استفسار أو دعم، راسلنا عبر البريد: 
        <a href='mailto:projectgraduate004@gmail.com' style='color:#4a90e2; text-decoration:none;'>
            projectgraduate004@gmail.com
        </a>
    </p>
</div>
</body>
</html>
";


            try
            {
                await _emailSender.SendEmailAsync(user.Email, "📧 تأكيد البريد الإلكتروني ", emailHtml);
            }
            catch (Exception ex)
            {
              
                Console.WriteLine("Email failed: " + ex.Message);
            }
            //await _emailSender.SendEmailAsync(user.Email, "Confirm your email", emailHtml);

            return new ApiResponse<UserResponse>
            {
                Success = true,
                Message = "The account confirmation has been sent to your email.",
                Data = new UserResponse { Id = user.Id, Email = user.Email, Username = user.UserName }
            };

        }

        public async Task<ApiResponse<object>> ForgetPassword(ForgetPasswordRequest request)
        {
            var user = await _userManager.FindByEmailAsync(request.Email);
            if (user is null)
                return new ApiResponse<object> { Success = false, Message = "user not found" };
            var random = new Random();
            var code = random.Next(1000, 9999).ToString();
            user.CodeResetPassword = code;
            user.CodeResetPasswordExpire= DateTime.UtcNow.AddMinutes(15);
            await _userManager.UpdateAsync(user);
            await _emailSender.SendEmailAsync(user.Email, "🔑 إعادة تعيين كلمة المرور",
    $@"
    <div style='font-family: Arial, sans-serif; max-width: 600px; margin: auto; padding: 20px; border: 1px solid #ddd; border-radius: 10px; background-color: #f9f9f9; text-align: center;'>
        <h2 style='color: #4a90e2;'>رمز إعادة تعيين كلمة المرور</h2>
        <p style='font-size: 16px; color: #333;'>مرحبًا،</p>
        <p style='font-size: 16px; color: #333;'>:رمز إعادة تعيين كلمة المرور الخاص بك هو</p>
        <p style='font-size: 32px; font-weight: bold; color: #e94e77; margin: 20px 0;'>{code}</p>
        <p style='font-size: 14px; color: #777;'>⚠️ سينتهي صلاحية هذا الرمز بعد 15 دقيقة.</p>
        <hr style='margin: 20px 0;'/>
        <p style='font-size: 12px; color: #aaa;'>إذا لم تطلب إعادة تعيين كلمة المرور، تجاهل هذه الرسالة.</p>
<p style='font-size: 12px; color: #999; line-height:1.6;'>
        © 2025 <b>Askly</b> – جميع الحقوق محفوظة.<br>
        في حال وجود أي استفسار أو دعم، راسلنا عبر البريد: 
        <a href='mailto:projectgraduate004@gmail.com' style='color:#4a90e2; text-decoration:none;'>
            projectgraduate004@gmail.com
        </a>
    </p>
    </div>"
                );
            return new ApiResponse<object>
            {
                Success = true,
                Message = "A verification code has been sent to your email address."
            };
        }

        public async Task<ApiResponse<object>> ResetPassword(ResetPasswordRequest request)
        {
            var user = await _userManager.FindByEmailAsync(request.Email);
            if (user is null)
                return new ApiResponse<object> { Success = false, Message = "user not found" };

            if (user.CodeResetPassword != request.Code)
                return new ApiResponse<object> { Success = false, Message = "invalid code" };

            if (user.CodeResetPasswordExpire < DateTime.UtcNow)
                return new ApiResponse<object> { Success = false, Message = "expired this code" };

            var token = await _userManager.GeneratePasswordResetTokenAsync(user);
            var result = await _userManager.ResetPasswordAsync(user, token, request.NewPassword);
            if (!result.Succeeded)
                return new ApiResponse<object>
                {
                    Success = false,
                    Message = "failed to reset your password",
                    Errors = result.Errors.Select(e => e.Description).ToList()
                };
            await _emailSender.SendEmailAsync(user.Email, "✅ تم تغيير كلمة المرور بنجاح",
    @"
    <div style='font-family: Arial, sans-serif; max-width: 600px; margin: auto; padding: 20px; border: 1px solid #ddd; border-radius: 10px; background-color: #e6ffed; text-align: center;'>
        <h2 style='color: #28a745;'>🎉 تم تغيير كلمة المرور بنجاح!</h2>
        <p style='font-size: 16px; color: #333;'>يمكنك الآن تسجيل الدخول باستخدام كلمة المرور الجديدة الخاصة بك.</p>
        <p style='font-size: 16px; color: #333;'>شكراً لاستخدامك خدمتنا 💖</p>
        <hr style='margin: 20px 0;'/>
        <p style='font-size: 12px; color: #555;'>إذا لم تقم بتغيير كلمة المرور، يرجى التواصل معنا فورًا.</p>
    </div>");
            return new ApiResponse<object>
            {
                Success = true,
                Message = "sucess to reset your password"
            };

        }

        public async Task<ApiResponse<object>> ChangePassword(string userId, ChangePasswordRequest request)
        {
            var user = await _userManager.FindByIdAsync(userId);
            if (user is null)
                return new ApiResponse<object> { Success = false, Message = "user not found" };
            var passwordCheck = await _userManager.CheckPasswordAsync(user, request.CurrentPassword);
            if (!passwordCheck)
                return new ApiResponse<object> { Success = false, Message = "wrong current password " };
            var result = await _userManager.ChangePasswordAsync(user, request.CurrentPassword, request.NewPassword);
            if (!result.Succeeded)
                return new ApiResponse<object>
                {
                    Success = false,
                    Message = "failed change the password",
                    Errors = result.Errors.Select(e => e.Description).ToList()
                };

            await _emailSender.SendEmailAsync(user.Email, "تغيير كلمة المرور", "<h1>تم تغيير كبمة المرور بنجاح✅</h1>");

            return new ApiResponse<object>
            {
                Success = true,
                Message = "تم تغيير كلمة المرور بنجاح✅"
            };
        }

        public async Task<ApiResponse<UserResponse>> ResendConfirmEmail(string email,HttpRequest httpRequest)
        {
            var user = await _userManager.FindByEmailAsync(email);
            if (user is null)
            {
                return new ApiResponse<UserResponse>
                {
                    Success = false,
                    Message = "user not found"
                };
            }
            if(await _userManager.IsEmailConfirmedAsync(user))
            {
                return new ApiResponse<UserResponse>
                {
                    Success = false,
                    Message = "you are already confirmed your email"
                };
            }
            var token = await _userManager.GenerateEmailConfirmationTokenAsync(user);
            var escapedToken = Uri.EscapeDataString(token);

            var emailUrl = $"{httpRequest.Scheme}://{httpRequest.Host}/api/Accounts/ConfirmEmail?token={escapedToken}&userId={user.Id}";

            var emailHtml = $@"
<!DOCTYPE html>
<html lang=""ar"">
<head>
    <meta charset=""UTF-8"">
    <meta name=""viewport"" content=""width=device-width, initial-scale=1.0"">
</head>
<body dir=""rtl"" style=""margin:0; padding:0; background-color:#f4f4f4; font-family: 'Segoe UI', Tahoma, Geneva, Verdana, sans-serif; color:#333;"">
    <div style='font-family: Arial, sans-serif; max-width: 600px; margin: auto; padding: 20px; border: 1px solid #ddd; border-radius: 10px; background-color: #f9f9f9; text-align: center;'>
    
    <!-- العنوان -->
    <h2 style='color: #4a90e2;'>تأكيد البريد الإلكتروني</h2>
    
    <!-- الترحيب -->
    <p style='font-size: 16px; color: #333;'>مرحبًا {user.UserName} 👋،</p>
    <p style='font-size: 16px; color: #333;'>شكرًا لانضمامك إلى <b>Askly</b> 🎉</p>
    <p style='font-size: 16px; color: #333;'>لتفعيل حسابك والبدء في استخدام خدماتنا، يرجى الضغط على الزر أدناه:</p>

    <!-- زر التأكيد -->
    <a href='{emailUrl}' 
       style='display:inline-block; padding:12px 28px; margin:20px 0; background-color:#4a90e2; color:#fff; font-size:16px; font-weight:bold; text-decoration:none; border-radius:6px;'>
       ✅ تأكيد البريد الإلكتروني
    </a>
    <!-- الفوتر -->
    <p style='font-size: 12px; color: #999; line-height:1.6;'>
        © 2025 <b>Askly</b> – جميع الحقوق محفوظة.<br>
        في حال وجود أي استفسار أو دعم، راسلنا عبر البريد: 
        <a href='mailto:projectgraduate004@gmail.com' style='color:#4a90e2; text-decoration:none;'>
            projectgraduate004@gmail.com
        </a>
    </p>
</div>
</body>
</html>
";

            await _emailSender.SendEmailAsync(user.Email, "تأكيد البريد الإلكتروني", emailHtml);

            return new ApiResponse<UserResponse>
            {
                Success = true,
                Message = "new confirm email send"
            };
        }
    }
}
