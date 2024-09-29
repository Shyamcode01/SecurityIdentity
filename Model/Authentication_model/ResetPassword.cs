using System.ComponentModel.DataAnnotations;

namespace WebApiIdentity_security.Model.Authentication_model
{
    public class ResetPassword
    {
        [Required]
        public string Password { get; set; } =null!;
    
        [Compare("Password", ErrorMessage ="password is not same")]
        public string ConformPassword { get; set; }=null!;
       
        public string? Token { get; set; }
       
        public string? Email { get; set; }
    }
}
