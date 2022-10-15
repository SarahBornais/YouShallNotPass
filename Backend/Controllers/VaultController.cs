using Microsoft.AspNetCore.Mvc;
using System;

namespace YouShallNotPassBackend.Controllers
{
    [Route("vault")]
    public class VaultController : Controller
    {
        [HttpGet]
        public ActionResult<Content> Get([FromQuery()] ContentKey contentKey)
        {
            return Ok(new Content());
        }

        [HttpPost] 
        public ActionResult<ContentKey> Post([FromBody] Content content)
        {
            return Ok(new ContentKey());
        }

        [HttpDelete]
        public IActionResult Delete([FromQuery(Name = "id")] Guid id)
        {
            return Ok();
        }
    }
}
