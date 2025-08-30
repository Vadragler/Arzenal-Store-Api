using ArzenalStoreApi.Services.Auth;
using ArzenalStoreApi.Services.CookieService;
using ArzenalStoreApi.Services.RequestContextProvider;
using ArzenalStoreApi.Services.RequestInfoProvider;
using ArzenalStoreApi.Services.Token;
using ArzenalStoreSharedDto.DTOs.AuthDto;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace ArzenalStoreApi.Controllers.Auth
{
    [Route("api/auth")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly IJwtService _jwtService;
        private readonly IRefreshTokenService _refreshTokenService;
        private readonly IAuthService _authService;
        private readonly ICookieService _cookieService = new CookieService();
        private readonly IRequestContextProvider _requestContextProvider = new RequestContextProvider();
        private readonly IRequestInfoProvider _requestInfoProvider = new RequestInfoProvider();

        public AuthController(IAuthService authService,IRefreshTokenService refreshTokenService,IJwtService jwtService)
        {
            _jwtService = jwtService;
            _refreshTokenService = refreshTokenService;
            _authService = authService;
        }

        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody] RegisterRequestDto request)
        {
            await _authService.RegisterAsync(request);
            return StatusCode(StatusCodes.Status201Created);
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginRequestDto request)
        {
            var infoProvider = _requestInfoProvider.GetRequestInfo(HttpContext, request);

            var token = await _authService.AuthenticateAsync(request.Email, request.Password, infoProvider);

            // 🔥 Délégué au CookieService
            _cookieService.SetAuthCookies(Response, token.accessToken, token.refreshToken);

            return Ok();
        }

        [HttpPost("refresh")]
        public async Task<IActionResult> Refresh()
        {
            var ctx = _requestContextProvider.Get(HttpContext);
            var refreshToken = _cookieService.GetRefreshToken(Request);

            var newRefreshToken = await _refreshTokenService.RotateRefreshTokenAsync(refreshToken, ctx.IpAddress, ctx.Fingerprint);
            var accessToken = await _jwtService.GenerateJwtTokenAsync(newRefreshToken.User.Email);

            _cookieService.SetAuthCookies(Response, accessToken, newRefreshToken.Token);
            return Ok();
        }

        [Authorize]
        [HttpPost("GenerateRefreshToken")]
        public async Task<IActionResult> GenerateRefreshToken([FromBody] CreateRefreshTokenDto dto)
        {
            var refreshToken = await _refreshTokenService.GenerateRefreshToken(dto);
            return Ok(new { RefreshToken = refreshToken });
        }

        [Authorize]
        [HttpPost("revoke")]
        public async Task<IActionResult> Revoke()
        {
            var userIdStr = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var refreshToken = _cookieService.GetRefreshToken(Request);

            await _refreshTokenService.RevokeRefreshTokenAsync(refreshToken, userIdStr);

            _cookieService.DeleterefreshCookie(Response);
            return Ok(new { Message = "Token révoqué avec succès." });
        }

        [Authorize]
        [HttpGet("logout")]
        public async Task<IActionResult> Logout()
        {
            var userIdStr = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var refreshToken = _cookieService.GetRefreshToken(Request);

            await _authService.LogoutAsync(refreshToken, userIdStr);

            _cookieService.DeleteAuthCookies(Response);

            return Ok(new { Message = "Déconnexion réussie" });
        }

        [Authorize]
        [HttpGet("ping")]
        public IActionResult Ping()
        {
            // Retourne explicitement un objet JSON pour éviter les erreurs de parsing côté client
            return Ok(new { status = "ok" });
        }

        [Authorize]
        [HttpGet("get-token")]
        public IActionResult GetToken()
        {
            // Récupère le JWT depuis le cookie
            var jwt = _cookieService.GetAuthToken(Request);
            return Ok(new { token = jwt });
        }

    }
}
