 
using UserManageService.Model;
using WebApiIdentity_security.Model;
using UserManageService.Service;
using Microsoft.Extensions.Options;
using MimeKit;
using MailKit.Net.Smtp;

namespace UserManageService.Service
{
    public class EmailService : IEmailService
    {
        private readonly IConfiguration _configuration;
        private EmailConfigration _eMAILcONFIG;

        public EmailService(IConfiguration configuration, IOptions< EmailConfigration> eMAILcONFIG)
        {
            _configuration = configuration;
            _eMAILcONFIG = eMAILcONFIG.Value;
        }

        public async Task<Response> SendEmail(string email, string subject, string message)
        {
            try
            {
                var mail = new MimeMessage();
                mail.From.Add(new MailboxAddress(_eMAILcONFIG.UserName, _eMAILcONFIG.From));
                mail.To.Add(MailboxAddress.Parse(email));
                mail.Subject = subject;


                var builder = new BodyBuilder
                {
                    HtmlBody = message // or PlainTextBody if you need plain text emails
                };
                mail.Body=builder.ToMessageBody();

                using(var client=new MailKit.Net.Smtp.SmtpClient())
                {
                    await client.ConnectAsync(_eMAILcONFIG.SmtpServer,_eMAILcONFIG.Port,_eMAILcONFIG.EnableSSL);
                    await client.AuthenticateAsync(_eMAILcONFIG.UserName,_eMAILcONFIG.Password);
                    await client.SendAsync(mail);
                    await client.DisconnectAsync(true);
                }




                // Return success response
                return new Response { Status = "success", Message = "Email sent successfully" };
            }
            catch (Exception ex)
            {
                // Capture and log the exception
                Console.WriteLine($"Failed to send email: {ex.Message}");

                // Return failure response
                return new Response { Status = "error", Message = $"Failed to send email: {ex.Message}" };
            }
        }
    }
}
