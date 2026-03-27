using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;

namespace ArzenalStoreApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [AllowAnonymous]
    [SwaggerTag("Status")]
    public class HealthController : ControllerBase
    {
        /// <summary>
        /// Renvoie le statut de santé de l'API
        /// </summary>
        /// <returns>
        /// 200 OK avec un objet contenant:
        /// - <c>Status</c> : "Healthy"
        /// </returns>
        [HttpGet]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public IActionResult Get() => Ok(new { Status = "Healthy" });

    }
}
