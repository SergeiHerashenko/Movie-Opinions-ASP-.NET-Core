using MailKit.Net.Smtp;
using MailKit.Security;
using MimeKit;
using MimeKit.Text;
using Notification.Models.Enum;
using Notification.Models.Notification;
using Notification.Models.Responses.SenderResponses;
using Notification.Services.Interfaces;

namespace Notification.Services.Senders
{
    public class EmailSender : ISender
    {
        public NotificationChannel Channel => NotificationChannel.Email;

        public async Task<SenderResponses> SendAsync(string destination, NotificationContent message)
        {
            var email = new MimeMessage();

            // 1. Від кого
            email.From.Add(new MailboxAddress("Movie Opinions", "your-email@gmail.com"));

            // 2. Кому
            email.To.Add(MailboxAddress.Parse(destination));

            // 3. Тема та вміст (використовуємо TextFormat.Html для красивої верстки)
            email.Subject = message.Subject;
            email.Body = new TextPart(TextFormat.Html) { Text = message.Body };

            using var smtp = new SmtpClient();
            try
            {
                // 4. Підключення до сервера (SecureSocketOptions.StartTls для безпеки)
                await smtp.ConnectAsync("smtp.gmail.com", 587, SecureSocketOptions.StartTls);

                // 5. Авторизація
                await smtp.AuthenticateAsync("", "");

                // 6. Відправка
                await smtp.SendAsync(email);
            }
            catch (Exception ex)
            {
                
            }
            finally
            {
                // 7. Обов'язково відключаємося
                await smtp.DisconnectAsync(true);
            }
            return new SenderResponses
            {
                IsSuccess= true,
            };
        }
    }
}
