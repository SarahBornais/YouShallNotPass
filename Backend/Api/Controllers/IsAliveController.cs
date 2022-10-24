using Microsoft.AspNetCore.Mvc;

namespace YouShallNotPassBackend.Controllers
{
    [Route("isAlive")]
    public class IsAliveController : Controller
    {
        [HttpGet]
        public IActionResult Get()
        {
            return Ok();
        }
    }
}
