using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace WebApiIdentity_security.Controllers.User
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class AdminController : ControllerBase
    {


        public AdminController()
        {
            
        }


        [HttpGet("GetList")]
        [Authorize(Roles ="Admin")]
        public IActionResult GetDetails()
        {
                List<string> errors = new List<string>()
                {
                   "adamin", "karan","mohan"
                };

            return Ok(errors);

        }


        
    }
}
