using System.Net;
using System.Net.Mail;
using UserManageService.Model;
using WebApiIdentity_security.Model;
using UserManageService.Service;

namespace UserManageService.Service
{
    public class EmailService : IEmailService
    {
        private readonly IConfiguration _configuration;

        public EmailService(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        public async Task<Response> SendEmail(string email, string subject, string message)
        {
            try
            {
                // Corrected configuration fetching with proper keys
                EmailConfigration getEmailSetting = new EmailConfigration()
                {
                    From = _configuration.GetValue<string>("AppSettings:EmailConfig:From"),
                    Password = _configuration.GetValue<string>("AppSettings:Password"),
                    SmtpServer = _configuration.GetValue<string>("AppSettings:EmailConfig:SmtpServer"),
                    Port = _configuration.GetValue<int>("AppSettings:EmailConfig:Port"),
                    EnableSSL = _configuration.GetValue<bool>("AppSettings:EmailConfig:EnableSSL"),
                    UserName = _configuration.GetValue<string>("AppSettings:EmailConfig:UserName")
                };

                // Create the email message
                MailMessage mailMessage = new MailMessage()
                {
                    From = new MailAddress(getEmailSetting.From),
                    Subject = subject,
                    Body = message,
                    IsBodyHtml = true // Option to send HTML content
                };

                mailMessage.To.Add(email);

                // Set up SMTP client
                var smtpClient = new SmtpClient(getEmailSetting.SmtpServer)
                {
                    Port = getEmailSetting.Port,
                    Credentials = new NetworkCredential(getEmailSetting.From, getEmailSetting.Password),
                    EnableSsl = getEmailSetting.EnableSSL
                };

                // Send the email
                await smtpClient.SendMailAsync(mailMessage);

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
