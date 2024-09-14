using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace WebApiIdentity_security.Controllers.User
{
    [Route("api/[controller]")]
    [ApiController]
    public class HRController : ControllerBase
    {



        [HttpGet("GetList")]
        [Authorize(Roles = "HR")]
        public IActionResult GetDetailsHr()
        {
            List<string> errors = new List<string>()
                {
                   "HR", "KIRAN","SUMAN"
                };

            return Ok(errors);

        }
    }
}
