
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http.Metadata;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using Org.BouncyCastle.Tsp;
using System.Data;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using UserManageService.Model;
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
        private readonly SignInManager<IdentityUser> _signInManager;
        public AuthenticationController(RoleManager<IdentityRole> roleManager,
            UserManager<IdentityUser> userManager, IConfiguration configration,
            UserManageService.Service.IEmailService emailService, SignInManager<IdentityUser> signInManager)
        {
            _roleManager = roleManager;

            _configration = configration;
            _userManager = userManager;
            _emailService = emailService;
            _signInManager = signInManager;
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
                        new Response { Status = "Error", Message = $"{registeruser.Email}: User All ready Exist !" }
                        ); ;
                }
                // user data base save
                IdentityUser user = new()
                {
                    Email = registeruser.Email,
                    SecurityStamp = Guid.NewGuid().ToString(),
                    UserName = registeruser.Username,
                    TwoFactorEnabled=true
                    



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
                    var status = await _emailService.SendEmail(registeruser.Email, "New User Added  !", "Congratulation your account is successfully create");
                    // Add role to user manager
                    await _userManager.AddToRoleAsync(user, role);


                    // Add tokent to verify the email
                    var token=await _userManager.GenerateEmailConfirmationTokenAsync(user);
                    var confirmationLink = Url.Action(nameof(ConfirmEmail), "Authentication", new {token,email=user.Email }, Request.Scheme);
                    
                    await _emailService.SendEmail(registeruser.Email,"Confirm your email id link",confirmationLink);

                    return StatusCode(StatusCodes.Status201Created,
                        new Response { Status = "Success", Message = $"{registeruser.Email}User Create Successfull" });
                    

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

        public async Task<IActionResult> Login(Login loginModel)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest( new Response { Status = "error", Message = "envalid data" });
            }
            var user = await _userManager.FindByNameAsync(loginModel.UserName);

            // VERIFY TWOFACTOR 
            if (user.TwoFactorEnabled)
            {
                await _signInManager.SignOutAsync();
                await _signInManager.PasswordSignInAsync(user, loginModel.Password, false, true);

                var tokenotp = await _userManager.GenerateTwoFactorTokenAsync(user, "Email");

                await _emailService.SendEmail(user.Email!, "Otp conformation ", tokenotp);
                return StatusCode(StatusCodes.Status200OK,

                    new Response { Status = "success", Message = $"you have send otp seuccessfully in your email {user.Email}" }
                    );
            }
            if (user != null && await _userManager.CheckPasswordAsync(user, loginModel.Password))
            {

                var authClaim = new List<Claim>
         {
             new Claim(ClaimTypes.Name,user.UserName),
             new Claim(JwtRegisteredClaimNames.Jti,Guid.NewGuid().ToString())
         };
                var userRole = await _userManager.GetRolesAsync(user);
                foreach (var role in userRole)
                {
                    authClaim.Add(new Claim(ClaimTypes.Role, role.ToString()));
                }




                var jwtToken = GetToken(authClaim);

                var tokengenerate = new JwtSecurityTokenHandler().WriteToken(jwtToken);


                return Ok(
                    new
                    {
                        token = tokengenerate,
                        expire = jwtToken.ValidTo

                    });
            }

            return BadRequest(new Response
            {
                Status = "error",
                Message = $"{loginModel.UserName} user credential faild  or user name or password not exists!"
            });

        }

        // login 2factor user

        [HttpPost]
        [Route("Login-2FA")]
        public async Task<IActionResult>LoginWithOtp(string otp,string username)
        {
            var user=await _userManager.FindByNameAsync(username);
            if (user == null)
            {
                return NotFound(new Response { Status = "error", Message = "User not found" });
            }
            var signIg = await _signInManager.TwoFactorSignInAsync("Email", otp, isPersistent: false,rememberClient: false);

            if(signIg.Succeeded)
            {
                 

                    var authClaim = new List<Claim>
                {
                    new Claim(ClaimTypes.Name,user.UserName),
                    new Claim(JwtRegisteredClaimNames.Jti,Guid.NewGuid().ToString())
                };
                    var userRole = await _userManager.GetRolesAsync(user);
                    foreach (var role in userRole)
                    {
                        authClaim.Add(new Claim(ClaimTypes.Role, role.ToString()));
                    }
                     



                    var jwtToken = GetToken(authClaim);

                    var tokengenerate = new JwtSecurityTokenHandler().WriteToken(jwtToken);


                    return Ok(
                        new
                        {
                            token = tokengenerate,
                            expire = jwtToken.ValidTo

                        });
           

            }else if (signIg.IsLockedOut)
            {
                return StatusCode(StatusCodes.Status423Locked, new Response { Status = "error", Message = "user account locked out" });

            }
            return StatusCode(StatusCodes.Status404NotFound,
                        new Response { Status = "Error", Message = $" invalid otp   !" });



        }

        // token generate
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

        // send token in your email 


        [HttpGet("ConfirmEmail")]
        public async Task<IActionResult>ConfirmEmail(string token,string email)
        {


            var user=await _userManager.FindByEmailAsync(email);
            if (user != null)
            {
                var result=await _userManager.ConfirmEmailAsync(user,token);
                if (result.Succeeded)
                {
                    return StatusCode(StatusCodes.Status200OK,

                        new Response { Status="success",Message="Email verify successfully!"}
                        );
                  
                }
               
                

            }
            
                return StatusCode(StatusCodes.Status500InternalServerError,

                        new Response { Status = "error", Message = "This user dous not exist " });
 
        }

        //  send password link here get token reset password

        [HttpPost("Forgot-password")]
        [AllowAnonymous]

        public async Task<IActionResult> ForgotPassword(string email)
        {
            var user = await _userManager.FindByEmailAsync(email);
            if (user != null)
            {
                var token=await _userManager.GeneratePasswordResetTokenAsync(user);
                var forgotPasswordLink = Url.Action(nameof(ResetPassword), "Authentication", new { token, email = user.Email }, Request.Scheme);
                _emailService.SendEmail(user.Email, "Reset password link ", forgotPasswordLink);
                return StatusCode(StatusCodes.Status200OK,

                    new Response { Status = "success", Message = $"user password reset link send {user.Email} Verify it " });
            }
            return StatusCode(StatusCodes.Status400BadRequest,
                new Response { Status = "error", Message = "Coud not send link email is not right" });
        }


        // reset password link post  change password post mentod

        [HttpPost("reset-password-Change")]
        [AllowAnonymous]
        
        public async Task<IActionResult>ResetPassword(ResetPassword resetPassword)
        {
            var user= await _userManager.FindByEmailAsync(resetPassword.Email);
            if (user != null)
            {
                var result = await _userManager.ResetPasswordAsync(user, resetPassword.Token, resetPassword.Password);
                if (!result.Succeeded) 
                {
                    foreach(var erro in result.Errors)
                    {
                        ModelState.AddModelError(erro.Code, erro.Description);
                    }
                    return Ok(ModelState);
                }
                return StatusCode(StatusCodes.Status200OK,
                 new Response { Status="success",Message="password has veen changed "}   
                    );
            }
            return StatusCode(StatusCodes.Status400BadRequest,
                 new Response { Status = "error", Message = "could not send link  " }
                    );

        }

        [HttpGet("Resetpassword")]
        public async Task<IActionResult> ResetPassword(string token, string email)
        {
            var model = new ResetPassword { Token = token, Email = email };

            return Ok(new { model });

        }
    }
}
