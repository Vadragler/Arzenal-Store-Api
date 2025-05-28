using Arzenal.Shared.Dtos.DTOs;
using Arzenal.Shared.Dtos.DTOs.AuthDto;
using Arzenal.Shared.Dtos.DTOs.RegisterDto;
using ArzenalStoreApi.Services;
using Microsoft.AspNetCore.Mvc;

namespace ArzenalStoreApi.Controllers
{
    [Route("api/auth")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly IAuthService _authService;

        public AuthController(IAuthService authService)
        {
            _authService = authService;
        }


        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody] RegisterRequestDto request)
        {
            try
            {
                Console.WriteLine($"DTO: {request.Username}, {request.Email}, {request.Password}, {request.Token}");

                var result = await _authService.RegisterAsync(request);

            if (!result.Success)
            {
                return BadRequest(result.ErrorMessage);
            }

                var response = new RegisterResponseDto { Message = "Inscription réussie" };
                return Ok(response);
            }
            catch (Exception ex)
            {
                // Ajoute ceci pour savoir ce qui plante :
                Console.WriteLine($"ERREUR REGISTER: {ex.Message} - {ex.StackTrace}");
                return StatusCode(500, "Erreur interne du serveur.");
            }
        }



        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginRequestDto request)
        {
            var token = await _authService.AuthenticateAsync(request.Email, request.Password);

            if (token == null)
            {
                return Unauthorized("Invalid email or password");
            }

            return Ok(new LoginResponseDto { Token = token });

        }
    }
}
