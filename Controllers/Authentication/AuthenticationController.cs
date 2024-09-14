
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using WebApiIdentity_security.Model;
using WebApiIdentity_security.Model.Authentication_model;
using WebApiIdentity_security.Model.Entity_model;




namespace WebApiIdentity_security.Controllers.Authentication
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
            _userManager = userManager;
            _emailService = emailService;

        }

        // register user
        [HttpPost]

        public async Task<IActionResult> Register(Signup registeruser, string role)
        {
            try
            {
                var userExist = await _userManager.FindByEmailAsync(registeruser.Email);
                if (userExist != null)
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
                if (await _roleManager.RoleExistsAsync(role))
                {
                    var result = await _userManager.CreateAsync(user, registeruser.password);
                    if (!result.Succeeded)
                    {


                        return StatusCode(StatusCodes.Status500InternalServerError,
                           new Response { Status = "Error", Message = "User Failed Create data" });
                    }

                    // email send data 
                    var status = await _emailService.SendEmail(registeruser.Email, "Account create Success fully !", "Congratulation your account is successfully create");
                    // Add role to user manager
                    await _userManager.AddToRoleAsync(user, role);
                    return StatusCode(StatusCodes.Status201Created,
                        new Response { Status = "Success", Message = "User Create Successfull" });

                }
                else
                {
                    return StatusCode(StatusCodes.Status500InternalServerError,
                          new Response { Status = "Error", Message = $"{role} Role Does not Exist !" });
                }


            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }



        // Login user credencial 


        [HttpPost]
        [Route("Login")]

        public async Task<IActionResult>Login(Login loginModel)
        {
            var user = await _userManager.FindByNameAsync(loginModel.UserName);

            if(user != null && await _userManager.CheckPasswordAsync(user,loginModel.Password))
            {

                var authClaim = new List<Claim>
                {
                    new Claim(ClaimTypes.Name,user.UserName),
                    new Claim(JwtRegisteredClaimNames.Jti,Guid.NewGuid().ToString())
                };
                var userRole = await _userManager.GetRolesAsync(user);
                foreach(var role in userRole)
                {
                    authClaim.Add(new Claim(ClaimTypes.Role,role.ToString()));
                }

                var jwtToken=GetToken(authClaim);

                var tokengenerate = new JwtSecurityTokenHandler().WriteToken(jwtToken);

                return Ok(new
                {
                    token = tokengenerate,
                    expire = jwtToken.ValidTo

                });

            }
            return Unauthorized();
        }


        private JwtSecurityToken GetToken(List<Claim> authClaims)
        {
            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configration["Jwt:Key"]));
            var token = new JwtSecurityToken(
                issuer: _configration["Jwt:Issuer"],
                audience: _configration["Jwt:Audience"],
                expires:DateTime.UtcNow.AddMinutes(1),
                claims:authClaims,
                signingCredentials:new SigningCredentials(key,SecurityAlgorithms.HmacSha256)
                );

            return token;

        }


    }
}
