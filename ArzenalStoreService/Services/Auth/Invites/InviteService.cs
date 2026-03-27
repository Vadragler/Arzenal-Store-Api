using Arzenal.Store.Api.Service.Interfaces;
using Arzenal.Store.Api.Domain.Models;
using Arzenal.Store.Api.Service.Exceptions;
using Microsoft.EntityFrameworkCore;
using Arzenal.Store.Api.Infrastructure.Data;

namespace Arzenal.Store.Api.Service.Services.Auth.Invites
{
    public class InviteService : IInviteService
    {
        private readonly AuthDbContext _context;

        public InviteService(AuthDbContext context)
        {
            _context = context;
        }

        public async Task ValidateInviteAsync(string? token)
        {
            if (string.IsNullOrEmpty(token))
                throw new UnauthorizedAccessException("Token Manqaunt.");

            var invite = await _context.InviteTokens.FirstOrDefaultAsync(i => i.Token == token);

            if (invite == null || invite.Used || invite.ExpiresAt.HasValue && invite.ExpiresAt < DateTime.UtcNow)
            {
                throw new UnauthorizedAccessException("Token invalide ou expiré.");
            }
        }

        public async Task<string> CreateInviteAsync(string email)
        {
            if(string.IsNullOrEmpty(email))
                throw new ValidationException("Email ne peut pas être vide.");

            var inviteToken = new InviteToken
            {
                Id = Guid.NewGuid(),
                Email = email,
                CreatedAt = DateTime.UtcNow,
                ExpiresAt = DateTime.UtcNow.AddDays(7),
                Used = false,
                Token = Guid.NewGuid().ToString()
            };

            _context.InviteTokens.Add(inviteToken);
            await _context.SaveChangesAsync();

            // Crée le lien d'invitation avec le token généré
            var inviteLink = $"http://arzenaldev.duckdns.org/signup?token={inviteToken.Token}";
            return inviteLink;
        }

        public async Task UseInviteAsync(string token)
        {
            if (string.IsNullOrEmpty(token))
                throw new UnauthorizedAccessException("Token Manquant.");

            var invite = await _context.InviteTokens
       .FirstOrDefaultAsync(i => i.Token == token);
            if (invite == null || invite.Used)
            {
                throw new UnauthorizedAccessException("Token invalide ou déjà utilisé.");
            }
            invite.Used = true;
            await _context.SaveChangesAsync();
        }
    }
}
