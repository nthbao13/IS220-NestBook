using Microsoft.AspNetCore.Identity.UI.Services;
using System.Net;
using System.Net.Mail;

namespace BookNest.Helper
{
	public class EmailSender : IEmailSender
	{
		public async Task SendEmailAsync(string email, string subject, string htmlMessage)
		{
            Console.WriteLine(">> Sending email to: " + email);

            var smtpClient = new SmtpClient("smtp.gmail.com")
			{
				Port = 587,
				Credentials = new NetworkCredential("tbaokiemcer@gmail.com", "ijzabfzykpbvdwqw"),
				EnableSsl = true,
			};

            var mailMessage = new MailMessage("tbaokiemcert@gmail.com", email, subject, htmlMessage)
            {
                IsBodyHtml = true
            };

            await smtpClient.SendMailAsync(mailMessage);
        }
	}
}
