using System.ComponentModel.DataAnnotations;

namespace WebApiIdentity_security.Model.Authentication_model
{
    public class Signup
    {
        [Required]
        public string? Username { get; set;}
        [Required]
        public string? Email { get; set; }
        [Required]
        public string? password { get; set; }
    }
}
