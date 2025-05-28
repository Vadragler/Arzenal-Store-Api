using System.ComponentModel.DataAnnotations;

namespace ArzenalStoreApi.Models
{
    public class InviteToken
    {
        public Guid Id { get; set; }  // Utilisation d'un GUID
        public string Email { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? ExpiresAt { get; set; }
        public bool Used { get; set; }
        public string Token { get; set; }  // Le token est généré dans le code
    }


}
