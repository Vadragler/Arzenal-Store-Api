using ArzenalStoreApi.Data;
using ArzenalStoreApi.Models;
using Microsoft.EntityFrameworkCore;

namespace ArzenalStoreApi.Services.InviteService
{
    public class InviteService : IInviteService
    {
        private readonly AuthDbContext _context;

        public InviteService(AuthDbContext context)
        {
            _context = context;
        }
        public async Task ValidateInviteAsync(string token)
        {
            var invite = await _context.InviteTokens.FirstOrDefaultAsync(i => i.Token == token);

            if (invite == null || invite.Used || (invite.ExpiresAt.HasValue && invite.ExpiresAt < DateTime.UtcNow))
            {
                throw new UnauthorizedAccessException("Token invalide ou expiré.");
            }
        }
        public async Task<string> CreateInviteAsync(string email)
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
            var inviteLink = $"http://localhost:56525/signup?token={inviteToken.Token}";
            return inviteLink;
        }
        public async Task UseInviteAsync(Guid token)
        {
            //le token est un GUID dans ce cas
            var invite = await _context.InviteTokens.FindAsync(token);
            if (invite == null || invite.Used)
            {
                throw new UnauthorizedAccessException("Token invalide ou déjà utilisé.");
            }
            invite.Used = true;
            await _context.SaveChangesAsync();
        }
    }
}
