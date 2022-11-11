using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Mvc;
using YouShallNotPassBackend.DataContracts;
using YouShallNotPassBackend.Security;

namespace YouShallNotPassBackend.Controllers
{
    [Route("security")]
    public class SecurityController : Controller
    {
        private readonly ITokenAuthority tokenAuthority;
        private readonly IAuthenticator authenticator;

        public SecurityController(ITokenAuthority tokenAuthority, IAuthenticator authenticator)
        {
            this.tokenAuthority = tokenAuthority;
            this.authenticator = authenticator;
        }

        [HttpGet]
        [AllowAnonymous]
        [Route("authenticate")]
        public ActionResult<AuthenticationToken> GetSecurityToken([FromQuery] Service service)
        {
            if (authenticator.Authenticate(service))
            {

                return Ok(tokenAuthority.GetToken(service.ServiceName));
            }

            return Unauthorized();
        }
    }
}
