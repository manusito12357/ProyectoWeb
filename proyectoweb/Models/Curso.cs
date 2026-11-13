using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using static System.Collections.Specialized.BitVector32;

namespace proyectoweb.Models
{
    public class Curso
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "El campo título es obligatorio")]
        [StringLength(150)]
        [Display(Name = "Título")]
        public string Titulo { get; set; }

        [Required(ErrorMessage = "El campo descripción es obligatorio")]
        [StringLength(2000)]
        [Display(Name = "Descripción")]
        public string Descripcion { get; set; }

        [Required(ErrorMessage = "El nivel es obligatorio")]
        [Display(Name = "Nivel")]
        public NivelCurso Nivel { get; set; }

        [Required(ErrorMessage = "La duración es obligatoria")]
        [Range(1, 10000, ErrorMessage = "La duración debe estar entre 1 y 10000 minutos")]
        [Display(Name = "Duración (minutos)")]
        public int DuracionMinutos { get; set; }

        [RegularExpression(@"^https?://\S+$", ErrorMessage = "Escribe una dirección web válida que empiece por http:// o https://")]
        [Display(Name = "Imagen")]
        public string? ImagenUrl { get; set; }

        [Display(Name = "Publicado")]
        public bool Publicado { get; set; } = false;

        [Display(Name = "Fecha de creación")]
        public DateTime FechaCreacion { get; set; } = DateTime.UtcNow;

        [Required(ErrorMessage = "La categoría es obligatoria")]
        [Display(Name = "Categoría")]
        public int CategoriaId { get; set; }

        [ForeignKey("CategoriaId")]
        public Categoria? Categoria { get; set; }

        public ICollection<Leccion>? Lecciones { get; set; }
        public ICollection<Inscripcion>? Inscripciones { get; set; }

        private static readonly HashSet<string> PalabrasComunes = new()
        {
            "introducción", "curso", "del", "de", "la", "el", "a", "al", "lenguaje", "y", "en", "con", "para", "desde"
        };

        [NotMapped]
        public string Iniciales
        {
            get
            {
                var palabras = Titulo.Split(' ', StringSplitOptions.RemoveEmptyEntries);
                var clave = palabras.FirstOrDefault(p => !PalabrasComunes.Contains(p.ToLower())) ?? palabras[0];
                return clave.Length < 2 ? clave.ToUpper() : char.ToUpper(clave[0]) + clave.Substring(1, 1);
            }
        }
    }
}
