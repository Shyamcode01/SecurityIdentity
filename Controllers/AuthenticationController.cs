 
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
 
using WebApiIdentity_security.Model;
using WebApiIdentity_security.Model.Authentication_model;
 



namespace WebApiIdentity_security.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthenticationController : ControllerBase
    {
        private readonly UserManager<IdentityUser> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly IConfiguration _configration;
       private readonly UserManageService.Service.IEmailService _emailService;
        public AuthenticationController(RoleManager<IdentityRole> roleManager,
            UserManager<IdentityUser> userManager, IConfiguration configration, UserManageService.Service.IEmailService emailService)
        {
            _roleManager = roleManager;

            _configration = configration;
            _userManager= userManager;
            _emailService= emailService;

        }

        // register user
        [HttpPost]

        public async Task<IActionResult> Register(Signup registeruser,string role)
        {
            try
            {
                var userExist = await _userManager.FindByEmailAsync(registeruser.Email);
                if(userExist != null)
                {
                    return StatusCode(StatusCodes.Status403Forbidden,
                        new Response { Status = "Error", Message = "User All ready Exist !" }
                        ); ;
                }
                // user data base save
                IdentityUser user = new()
                {
                    Email = registeruser.Email,
                    SecurityStamp = Guid.NewGuid().ToString(),
                    UserName = registeruser.Username
                    
                     
                    
                };

                // check role exist or not 
                if( await _roleManager.RoleExistsAsync(role) )
                {
                    var result = await _userManager.CreateAsync(user, registeruser.password);
                    if (!result.Succeeded)
                    {


                        return StatusCode(StatusCodes.Status500InternalServerError,
                           new Response { Status = "Error", Message = "User Failed Create data" });
                    }

                        // email send data 
                      var status=  await _emailService.SendEmail(registeruser.Email, "Account create Success fully !", "Congratulation your account is successfully create");
                    // Add role to user manager
                    await _userManager.AddToRoleAsync(user,role);
                    return StatusCode(StatusCodes.Status201Created,
                        new Response { Status = "Success", Message = "User Create Successfull" });

                }
                else
                {
                    return StatusCode(StatusCodes.Status500InternalServerError,
                          new Response { Status = "Error", Message = $"{role} Role Does not Exist !" });
                }
              

            }catch (Exception ex)
            {
                return  BadRequest(ex.Message);
            }
        }

 
    }
}
