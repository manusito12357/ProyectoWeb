using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace proyectoweb.Models
{
    public class Inscripcion
    {
        public int Id { get; set; }

        [Display(Name = "Fecha de inscripción")]
        public DateTime FechaInscripcion { get; set; } = DateTime.UtcNow;

        [Display(Name = "Fecha de finalización")]
        public DateTime? FechaFinalizacion { get; set; }

        [Required]
        public string UserId { get; set; }

        [ForeignKey("UserId")]
        public IdentityUser? User { get; set; }

        [Required]
        public int CursoId { get; set; }

        [ForeignKey("CursoId")]
        public Curso? Curso { get; set; }
    }
}
