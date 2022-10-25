using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Mvc;

namespace YouShallNotPassBackend.Controllers
{
    [Route("isAlive")]
    [EnableCors("AllowAnyOrigin")]
    public class IsAliveController : Controller
    {
        [HttpGet]
        public IActionResult Get()
        {
            return Ok();
        }
    }
}
