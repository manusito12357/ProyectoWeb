using System.ComponentModel.DataAnnotations;

namespace proyectoweb.Models
{
    public class Categoria
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "El campo nombre es obligatorio")]
        [StringLength(80)]
        [Display(Name = "Nombre")]
        public string Nombre { get; set; }

        public ICollection<Curso>? Cursos { get; set; }
    }
}
