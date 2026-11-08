namespace proyectoweb.Models
{
    public class InscritoCurso
    {
        public string? Email { get; set; }
        public DateTime FechaInscripcion { get; set; }
        public DateTime? FechaFinalizacion { get; set; }
        public int Completadas { get; set; }
    }
}
