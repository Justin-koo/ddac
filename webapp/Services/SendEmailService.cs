using MailKit.Net.Smtp;
using MailKit.Security;
using MimeKit;
using MimeKit.Text;
using Microsoft.Extensions.Options;
using System.Threading.Tasks;

namespace webapp.Services
{
	public class SendEmailService
	{
		private readonly SmtpSettings _smtpSettings;

		public SendEmailService(IOptions<SmtpSettings> smtpSettings)
		{
			_smtpSettings = smtpSettings.Value;
		}

		public async Task SendAsync(string recipientEmail, string subject, string htmlMessage, string ccEmail = null)
		{
			var email = new MimeMessage();
			email.From.Add(new MailboxAddress(_smtpSettings.SenderName, _smtpSettings.SenderEmail));
			email.To.Add(MailboxAddress.Parse(recipientEmail));
			email.Subject = subject;
			email.Body = new TextPart(TextFormat.Html) { Text = htmlMessage };
			if (!string.IsNullOrWhiteSpace(ccEmail))
			{
				email.Cc.Add(MailboxAddress.Parse(ccEmail));
			}

			using var smtp = new SmtpClient();
			await smtp.ConnectAsync(_smtpSettings.Server, _smtpSettings.Port, SecureSocketOptions.StartTls);
			await smtp.AuthenticateAsync(_smtpSettings.Username, _smtpSettings.Password);
			await smtp.SendAsync(email);
			await smtp.DisconnectAsync(true);
		}
	}
}
