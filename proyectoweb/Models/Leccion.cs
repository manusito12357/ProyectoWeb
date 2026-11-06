using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace proyectoweb.Models
{
    public class Leccion
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "El campo título es obligatorio")]
        [StringLength(150)]
        [Display(Name = "Título")]
        public string Titulo { get; set; }

        [Required(ErrorMessage = "El contenido es obligatorio")]
        [Display(Name = "Contenido")]
        public string Contenido { get; set; }   

        [Required(ErrorMessage = "El orden es obligatorio")]
        [Range(1, 1000, ErrorMessage = "El orden debe ser mayor a 0")]
        [Display(Name = "Orden")]
        public int Orden { get; set; }

        [Required(ErrorMessage = "El curso es obligatorio")]
        [Display(Name = "Curso")]
        public int CursoId { get; set; }

        [ForeignKey("CursoId")]
        public Curso? Curso { get; set; }

        public ICollection<LeccionCompletada>? Completadas { get; set; }
    }
}
