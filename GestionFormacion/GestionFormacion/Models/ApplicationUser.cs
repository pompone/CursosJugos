using Microsoft.AspNetCore.Identity;

namespace GestionFormacion.Models
{
    public class ApplicationUser : IdentityUser
    {
        public bool DebeCambiarPassword { get; set; } = false;

        public bool SolicitoResetPassword { get; set; } = false;

        public DateTime? FechaSolicitudReset { get; set; }
    }
}