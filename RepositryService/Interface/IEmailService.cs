

using WebApiIdentity_security.Model;

namespace UserManageService.Service
{
    public interface IEmailService
    {

    public  Task<Response> SendEmail(string email,string subject, string message);
    }
}
