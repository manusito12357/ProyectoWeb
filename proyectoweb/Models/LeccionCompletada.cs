using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using static System.Collections.Specialized.BitVector32;

namespace proyectoweb.Models
{
    public class LeccionCompletada
    {
        public int Id { get; set; }

        [Display(Name = "Fecha")]
        public DateTime Fecha { get; set; } = DateTime.UtcNow;

        [Required]
        public string UserId { get; set; }

        [ForeignKey("UserId")]
        public IdentityUser? User { get; set; }

        [Required]
        public int LeccionId { get; set; }

        [ForeignKey("LeccionId")]
        public Leccion? Leccion { get; set; }
    }
}
