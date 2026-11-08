namespace proyectoweb.Models
{
    public class ProgresoCurso
    {
        public Curso Curso { get; set; } = null!;
        public int Total { get; set; }
        public int Completadas { get; set; }

        public int Porcentaje => Total == 0 ? 0 : Completadas * 100 / Total;
    }
}
