using Microsoft.AspNetCore.Mvc;
using ArzenalStoreApi.Data;
using Microsoft.AspNetCore.Authorization;
using ArzenalStoreSharedDto.DTOs.AppDto;
using ArzenalStoreApi.Services.AppService;


namespace ArzenalStoreApi.Controllers.Apps
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class AppsController : ControllerBase
    {
        private readonly IAppService _appService;
        public AppsController(IAppService appService)
        {
            _appService = appService;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<ReadAppDto>>> GetAllApps()
        {
            var appDtos = await _appService.GetAllAppsAsync();
            // Retourner les données sous forme de JSON
            return Ok(appDtos);
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<ReadAppDto>> GetAppById(Guid id)
        {
            var appDto = await _appService.GetAppByIdAsync(id);
            return Ok(appDto);
        }

        [HttpPost]
        public async Task<IActionResult> CreateApp([FromBody] CreateAppDto Appdto)
        {
            var AppCreated = await _appService.CreateAppAsync(Appdto);
            return CreatedAtAction(nameof(GetAppById), new { id = AppCreated.Id }, AppCreated);
        }

        [HttpPatch("{id}")]
        public async Task<IActionResult> UpdateApp(Guid id, [FromBody] UpdateAppDto updateAppDto)
        {
            var result = await _appService.UpdateAppAsync(id, updateAppDto);
            return Ok(result);
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteApp(Guid id)
        {
           await _appService.DeleteAppAsync(id);
           return NoContent();
        }
    }
}
