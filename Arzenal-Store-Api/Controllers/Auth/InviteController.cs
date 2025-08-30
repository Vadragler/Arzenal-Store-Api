using ArzenalStoreApi.Data;
using ArzenalStoreApi.Models;
using ArzenalStoreApi.Services.InviteService;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

[Route("api/[controller]")]
[ApiController]
public class InviteController : ControllerBase
{
    private readonly IInviteService _inviteService;

    public InviteController(IInviteService inviteService)
    {
        _inviteService = inviteService;
    }

    [HttpGet("validate")]
    public async Task<IActionResult> ValidateToken([FromQuery] string token)
    {
        await _inviteService.ValidateInviteAsync(token);
        return Ok();
    }

    [HttpPost("generate")]
    public async Task<IActionResult> GenerateToken([FromBody] string email)
    {
        var invitelink = await _inviteService.CreateInviteAsync(email);
        return Ok(new { invitelink });
    }


    [HttpPost("use")]
    public async Task<IActionResult> UseToken([FromQuery] Guid token)
    {
        await _inviteService.UseInviteAsync(token);
        return Ok();
    }
}
