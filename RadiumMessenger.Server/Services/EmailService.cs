using Microsoft.Extensions.Configuration;
using Resend;

namespace RadiumMessenger.Server.Services
{
    public class EmailService
    {
        private readonly IResend _resend;

        public EmailService(IResend resend)
        {
            _resend = resend;
        }

        public async Task SendVerificationCodeAsync(
            string email,
            string code)
        {
            var message = new EmailMessage
            {
                From = "Radium Messenger <onboarding@resend.dev>",
                To = new[] { email },
                Subject = "Подтверждение электронной почты",
                HtmlBody = $@"
                    <h2>Radium Messenger</h2>
                    <p>Ваш код подтверждения:</p>
                    <h1>{code}</h1>
                    <p>Код действителен в течение 10 минут.</p>"
            };

            await _resend.EmailSendAsync(message);
        }
    }
}