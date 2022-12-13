using Microsoft.AspNetCore.Mvc;
using System.ComponentModel.DataAnnotations;
using YouShallNotPassBackend.DataContracts;
using YouShallNotPassBackend.Exceptions;
using YouShallNotPassBackend.Storage;
using Authorize = YouShallNotPassBackend.Security.AuthorizeAttribute;

namespace YouShallNotPassBackend.Controllers
{
    /// <summary>
    /// CRUD for secret data (data is encrypted at rest)
    /// </summary>
    [Route("vault")]
    public class VaultController : Controller
    {
        private readonly ILogger logger;
        private readonly IStorageManager storageManager;

        public VaultController(ILogger<VaultController> logger, IStorageManager storageManager)
        {
            this.logger = logger;
            this.storageManager = storageManager;
        }

        /// <summary>
        /// Retrieve secret data
        /// </summary>
        /// <remarks>
        /// Sample request
        /// 
        ///     GET /vault?Id=8114b83b-cd09-4ebe-a962-936a206f4feb&amp;Key=B3F545E58849C0C78DFA0094F02E48CE
        ///     
        /// </remarks>
        /// <returns>
        /// 
        ///     {
        ///        "contentType": 2,
        ///        "label": "my super secret password",
        ///        "expirationDate": "2022-11-10T15:28:24.858242",
        ///        "maxAccessCount": 5,
        ///        "timesAccessed": 2,
        ///        "data": "cGFzc3dvcmQ="
        ///     }
        ///     
        /// </returns>
        [HttpGet]
        public ActionResult<Content> Get([FromQuery()] ContentKey contentKey)
        {
            string path = $"[GET] {Request.Path.Value}";

            if (contentKey == null)
            {
                logger.LogInformation("{path}: Bad request because of null content key parameter", path);
                return BadRequest("Improperly formatted content key parameter");
            }

            if (contentKey.Key == null)
            {
                logger.LogInformation("{path}: Bad request because of null key parameter", path);
                return BadRequest("Improperly formatted key in content key parameter");
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
            catch (Exception e) when (e is InvalidSecurityQuestionAnswerException)
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

        /// <summary>
        ///     Get the security question that is required to get data with id
        /// </summary>
        /// <param name="id"> UUID-formatted string </param>
        /// <remarks>
        /// Sample request
        /// 
        ///     GET /vault/securityQuestion?Id=8114b83b-cd09-4ebe-a962-936a206f4feb
        ///     
        /// </remarks>
        [HttpGet]
        [Route("securityQuestion")]
        public ActionResult<string?> GetSecurityQuestion([FromQuery] [Required] Guid id)
        {
            string path = $"[GET] {Request.Path.Value}";

            try
            {
                logger.LogInformation("{path}: Received request with id {id}", path, id);
                return storageManager.GetSecurityQuestion(id);
            }
            catch (Exception e) when (e is EntryExpiredException || e is EntryNotFoundException)
            {
                ActionResult error = NotFound();
                logger.LogInformation("{path}: Unsuccsesfully processed request with id {id}, error {error}", path, id, error);
                return error;
            }
            catch (Exception e)
            {
                logger.LogError(e, "{path}: id {id}", path, id);
                return Problem();
            }
        }

        /// <summary>
        /// Upload secret data to be encryped at rest
        /// </summary>
        /// <remarks>
        /// Sample request
        ///     
        ///     POST /vault
        ///     {
        ///        "contentType": 2,  
        ///        "label": "my super secret password",
        ///        "expirationDate": "2022-11-10T15:28:24.858242",
        ///        "maxAccessCount": 5,
        ///        "data": "cGFzc3dvcmQ="
        ///     }
        ///     
        /// To generate an expiration date one week from now in python
        ///     
        ///     from datetime import datetime, timedelta
        ///     (datetime.now() + timedelta(days=1)).isoformat()
        ///
        /// </remarks>
        /// <returns>
        ///     The key required to retrieve the secret data:
        ///     
        ///     {
        ///        "id": "8fa2c316-6380-4f57-b80d-48a9545e9b0f",
        ///        "key": "4772F324327F569605BB970A4496BEE5"
        ///     }
        /// </returns>
        [HttpPost]
        [Authorize]
        public ActionResult<ContentKey> Post([FromBody] Content content)
        {
            string path = $"[POST] {Request.Path.Value}";

            if (content == null)
            {
                logger.LogInformation("{path}: Bad request because of null content parameter, label {label}", path, content?.Label);
                return BadRequest("Improperly formated json for content parameter");
            }

            if (content.Label == null)
            {
                logger.LogInformation("{path}: Bad request because of null label parameter, label {label}", path, content?.Label);
                return BadRequest("Improperly formated label in content parameter");
            }

            if (content.Data == null)
            {
                logger.LogInformation("{path}: Bad request because of null data parameter, label {label}", path, content?.Label);
                return BadRequest("Improperly formated data in content parameter");
            }

            if (content.Data.Length > 10 * 1024)
            {
                logger.LogInformation("{path}: Bad request because data exceeds 10kB, label {label}", path, content?.Label);
                return BadRequest("Data must be less then 10kB");
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

        /// <summary>
        ///     Delete secret data
        /// </summary>
        /// <param name="id"> UUID-formatted string </param>
        /// <remarks>
        /// Sample request
        /// 
        ///     DELETE /vault?Id=8114b83b-cd09-4ebe-a962-936a206f4feb
        ///     
        /// </remarks>
        [HttpDelete]
        public IActionResult Delete([FromQuery] [Required] Guid id)
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
