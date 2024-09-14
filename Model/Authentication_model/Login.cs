using System.ComponentModel.DataAnnotations;

namespace WebApiIdentity_security.Model.Entity_model
{
    public class Login
    {


        [Required(ErrorMessage ="User name is require ")]
        public string? UserName { get; set; }
        [Required(ErrorMessage ="Password name is requre ")]
        public string? Password { get; set; }

    }
}
