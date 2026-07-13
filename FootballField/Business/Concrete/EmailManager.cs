using Business.Abstract;
using Business.BusinessAspects.Autofac;
using Entities.DTOs;
using MailKit.Net.Smtp;
using MailKit.Security;
using Microsoft.Extensions.Options;
using MimeKit;
using System;
using System.Collections.Generic;
using System.Text;

namespace Business.Concrete
{
    public class EmailManager : IEmailService
    {
        private readonly EmailSettings _emailSettings;


        
        public EmailManager(IOptions<EmailSettings> emailSettings)
        {
           _emailSettings = emailSettings.Value;
        }


        public async Task SendEmailAsync(string toEmail, string subject, string htmlMessage)
        {
            var email = new MimeMessage();

            // Gönderen bilgisi
            email.From.Add(new MailboxAddress(_emailSettings.SenderName, _emailSettings.SenderEmail));

            // Alıcı bilgisi
            email.To.Add(MailboxAddress.Parse(toEmail));

            // Konu
            email.Subject = subject;

            // HTML içerikli gövde (Body)
            var builder = new BodyBuilder
            {
                HtmlBody = htmlMessage
            };
            email.Body = builder.ToMessageBody();

            // SMTP sunucusuna bağlanma ve gönderme
            using var smtp = new SmtpClient();
            try
            {
                // StartTls ile güvenli bağlantı açıyoruz
                await smtp.ConnectAsync(_emailSettings.Server, _emailSettings.Port, SecureSocketOptions.StartTls);
                await smtp.AuthenticateAsync(_emailSettings.Username, _emailSettings.Password);
                await smtp.SendAsync(email);
            }
            catch (Exception ex)
            {
                // Burada loglama mekanizması (Serilog vb.) devreye girebilir
                throw new InvalidOperationException($"Mail gönderimi sırasında bir hata oluştu: {ex.Message}");
            }
            finally
            {
                await smtp.DisconnectAsync(true);
            }
        }
    }
}
