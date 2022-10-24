using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Mvc;
using YouShallNotPassBackend.DataContracts;
using YouShallNotPassBackend.Exceptions;
using YouShallNotPassBackend.Storage;

namespace YouShallNotPassBackend.Controllers
{
    [Route("vault")]
    [EnableCors("AllowAnyOrigin")]
    public class VaultController : Controller
    {
        private readonly ILogger logger;
        private readonly IStorageManager storageManager;

        public VaultController(ILogger<VaultController> logger, IStorageManager storageManager)
        {
            this.logger = logger;
            this.storageManager = storageManager;
        }

        [HttpGet]
        public ActionResult<Content> Get([FromQuery()] ContentKey contentKey)
        {
            string path = $"[GET] {Request.Path.Value}";

            if (contentKey == null)
            {
                logger.LogInformation("{path}: Bad request because of null parameter", path);
                return BadRequest();
            }

            try
            {
                logger.LogInformation("{path}: Received request with id {id}", path, contentKey.Id);

                Content content = storageManager.GetEntry(contentKey);

                logger.LogInformation("{path}: Successfully processed request with id {id}", path, contentKey.Id);

                return content;
            } 
            catch (Exception e) when (e is EntryExpiredException || e is EntryNotFoundException)
            {
                ActionResult error = NotFound();
                logger.LogInformation("{path}: Unsuccsesfully processed request with id {id}, error {error}", path, contentKey.Id, error);
                return error;
            }
            catch (Exception e) when (e is InvalidKeyException)
            {
                ActionResult error = Unauthorized();
                logger.LogInformation("{path}: Unsuccsesfully processed request with id {id}, error {error}", path, contentKey.Id, error);
                return error;
            }
            catch (Exception e)
            {
                logger.LogError(e, "{path}: id {id}", path, contentKey.Id);
                return Problem();
            }
        }

        [HttpPost] 
        public ActionResult<ContentKey> Post([FromBody] Content content)
        {
            string path = $"[POST] {Request.Path.Value}";

            if (content == null || content.Label == null || content.Data == null)
            {
                logger.LogInformation("{path}: Bad request because of null parameter, label {label}", path, content?.Label);
                return BadRequest();
            }

            try
            {
                logger.LogInformation("{path}: Received request with label {label}", path, content.Label);

                ContentKey contentKey = storageManager.AddEntry(content);

                logger.LogInformation("{path}: Successfully processed request with label {label}", path, content.Label);

                return contentKey;
            }
            catch (Exception e)
            {
                logger.LogError(e, "{path}: label {label}", path, content.Label);
                return Problem();
            }
        }

        [HttpDelete]
        public IActionResult Delete([FromQuery(Name = "id")] Guid id)
        {
            string path = $"[DELETE] {Request.Path.Value}";

            try
            {
                logger.LogInformation("{path}: Received request with id {id}", path, id);

                
                if (storageManager.DeleteEntry(id))
                {
                    logger.LogInformation("{path}: Successfully processed request with id {id}", path, id);
                    return Ok();
                }
                else
                {
                    ActionResult error = NotFound();
                    logger.LogInformation("{path}: Unsuccsesfully processed request with id {id}, error {error}", path, id, error);
                    return error;
                }
            }
            catch (Exception e)
            {
                logger.LogError(e, "{path}: id {id}", path, id);
                return Problem();
            }
        }
    }
}
