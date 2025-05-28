using ArzenalStoreApi.Data;
using ArzenalStoreApi.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

[Route("api/[controller]")]
[ApiController]
public class InviteController : ControllerBase
{
    private readonly AuthDbContext _context;

    public InviteController(AuthDbContext context)
    {
        _context = context;
    }

    [HttpGet("validate")]
    public async Task<IActionResult> ValidateToken([FromQuery] string token)
    {
        var invite = await _context.InviteTokens.FirstOrDefaultAsync(i => i.Token == token);

        if (invite == null || invite.Used || (invite.ExpiresAt.HasValue && invite.ExpiresAt < DateTime.UtcNow))
        {
            return BadRequest("Token invalide ou expiré.");
        }

        return Ok();
    }

    [HttpPost("generate")]
    public async Task<IActionResult> GenerateToken([FromBody] string email)
    {
        var inviteToken = new InviteToken
        {
            Id = Guid.NewGuid(),
            Email = email,  // Utilisation de 'email' directement
            CreatedAt = DateTime.UtcNow,
            ExpiresAt = DateTime.UtcNow.AddDays(7),  // Par exemple, expire dans 7 jours
            Used = false,
            Token = Guid.NewGuid().ToString()  // Génère un UUID pour le token
        };

        _context.InviteTokens.Add(inviteToken);
        await _context.SaveChangesAsync();

        // Crée le lien d'invitation avec le token généré
        var inviteLink = $"https://ton-site.fr/signup?token={inviteToken.Token}"; // Utilise le Token pour créer le lien

        return Ok(new { inviteLink });
    }


    [HttpPost("use")]
    public async Task<IActionResult> UseToken([FromQuery] Guid token)
    {
        var invite = await _context.InviteTokens.FindAsync(token);

        if (invite == null || invite.Used)
            return BadRequest("Token invalide ou déjà utilisé.");

        invite.Used = true;
        await _context.SaveChangesAsync();

        return Ok();
    }
}
