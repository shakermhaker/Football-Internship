using System;
using System.Collections.Generic;
using System.Text;

namespace Business.Abstract
{
    public interface IEmailService
    {
        Task SendEmailAsync(string toEmail, string subject, string htmlMessage);
    }
}
