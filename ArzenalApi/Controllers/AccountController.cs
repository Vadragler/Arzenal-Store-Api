using Arzenal.Shared.Dtos.DTOs.AccountDto;
using ArzenalApi.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace ArzenalApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AccountController : ControllerBase
    {
        private readonly IAuthService _authService;

        public AccountController(IAuthService authService)
        {
            _authService = authService;
        }

        // GET: api/<AccountController>
        [Authorize]
        [HttpGet("me")]
        public async Task<IActionResult> GetCurrentUser()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var user = await _authService.FindByIdAsync(userId);

            // Fix: Check if user fields are null or empty
            if (string.IsNullOrEmpty(user.username) || string.IsNullOrEmpty(user.email))
            {
                return BadRequest("User data is invalid.");
            }

            return Ok(new
            {
                user.email,
                user.username,
                // ajoute d'autres champs si nécessaire
            });
        }

        [Authorize]
        [HttpDelete("me")]
        public async Task<IActionResult> DeleteCurrentUser()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var Success = await _authService.DeleteAsync(userId);
            if(Success)
            return Ok();
            else
            {
                return BadRequest();
            }
        }

        [Authorize]
        [HttpPatch("me")]
        public async Task<IActionResult> PatchCurrentUser([FromBody] UpdateAccountDto updateaccountdto)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var Success = await _authService.PatchAsync(userId, updateaccountdto.Username, updateaccountdto.Email, updateaccountdto.ActualPassword, updateaccountdto.NewPassword);
            if (Success)
                return Ok();
            else
            {
                return BadRequest();
            }
        }

        // Other methods remain unchanged
    }
}
