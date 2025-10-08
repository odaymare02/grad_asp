using Microsoft.AspNetCore.Identity.UI.Services;
using System.Net;
using System.Net.Mail;

namespace Graduation_project.utalities
{
    public class SendEmail : IEmailSender
    {
        private readonly IConfiguration _configuration;

        public SendEmail(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        public Task SendEmailAsync(string email, string subject, string htmlMessage)
        {
            var client = new SmtpClient("smtp.gmail.com", 587)//ها الخدمة المجانية الي بتقدمها هاي النورية لارسال الايميل
            {
              

                EnableSsl = true,
                UseDefaultCredentials = false,
                Credentials = new NetworkCredential("projectgraduate004@gmail.com", _configuration["emailPassword:pass"])//وبنجيب كلمة مرور للتطبيقات من داخل الحساب مشان احطها هان 
            };

            return client.SendMailAsync(
                new MailMessage(from: "projectgraduate004@gmail.com",
                                to: email,
                                subject,
                                htmlMessage
                                )
                { IsBodyHtml = true }
                );
        }
    }
}
