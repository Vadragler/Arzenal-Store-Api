using Arzenal.Store.Api.Service.Interfaces;
using Arzenal.Dto.DTOs.AccountDto;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;

namespace ArzenalStoreApi.Controllers.User
{
    [Authorize(Policy = "Web")]
    [Route("api/[controller]")]
    [ApiController]
    [SwaggerTag("Devices")]
    public class DevicesController : ControllerBase
    {
        private readonly IDeviceService _deviceService;
        public DevicesController(IDeviceService deviceService)
        {
            _deviceService = deviceService;
        }

        /// <summary>
        /// Récupère tous les appareils associés à l'utilisateur actuellement connecté
        /// </summary>
        /// <returns>
        /// 200 OK avec une liste d'objets <c>ReadDeviceDto</c> représentant les appareils de l'utilisateur connecté.
        /// </returns>
        [HttpGet]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> GetAllDevice()
        {
            List<ReadDeviceDto> devices = await _deviceService.GetAllDevice(Request);
            return Ok(devices);
        }

        /// <summary>
        /// Révoque un appareil spécifique associé à l'utilisateur actuellement connecté
        /// </summary>
        /// <param name="deviceId">Identifiant de l'appareil de l'utilisateur à déconnecter.</param>
        /// <returns>
        /// 200 OK avec un object contenant:
        /// - <c>success</c> : indique si la révocation a réussi.
        /// </returns>
        [HttpDelete("revoke/{deviceId}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> RevokeDevice(Guid deviceId)
        {
            var result = await _deviceService.RevokeDevice(deviceId);
            return Ok(new { success = result });
        }
    }
}
