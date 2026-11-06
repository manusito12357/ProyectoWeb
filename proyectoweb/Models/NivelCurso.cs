using System.ComponentModel.DataAnnotations;

namespace proyectoweb.Models
{
    public enum NivelCurso
    {
        [Display(Name = "Básico")] Basico = 1,
        [Display(Name = "Intermedio")] Intermedio = 2,
        [Display(Name = "Avanzado")] Avanzado = 3
    }
}
