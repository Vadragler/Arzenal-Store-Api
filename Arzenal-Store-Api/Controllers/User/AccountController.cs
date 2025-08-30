using ArzenalStoreApi.Services.Auth;
using ArzenalStoreApi.Services.UserService;
using ArzenalStoreSharedDto.DTOs.AccountDto;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace ArzenalStoreApi.Controllers.User
{
    [Route("api/[controller]")]
    [ApiController]
    public class AccountController : ControllerBase
    {
        private readonly IUserService _userService;

        public AccountController(IUserService userService)
        {
            _userService = userService;
        }

        // GET: api/<AccountController>
        [Authorize]
        [HttpGet("me")]
        public async Task<IActionResult> GetCurrentUser()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var user = await _userService.FindByIdAsync(userId);
            return Ok(user);
        }

        [Authorize]
        [HttpDelete("me")]
        public async Task<IActionResult> DeleteCurrentUser()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            await _userService.DeleteAsync(userId);
            return NoContent();
        }

        [Authorize]
        [HttpPatch("me")]
        public async Task<IActionResult> PatchCurrentUser([FromBody] UpdateAccountDto updateaccountdto)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            await _userService.PatchAsync(userId, updateaccountdto);
            return NoContent();
        }
    }
}
