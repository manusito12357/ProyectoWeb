namespace proyectoweb.Models
{
    public class ResumenPanel
    {
        public int Cursos { get; set; }
        public int Publicados { get; set; }
        public int Categorias { get; set; }
        public int Lecciones { get; set; }
        public int Suscriptores { get; set; }
        public int Inscripciones { get; set; }
        public int Terminados { get; set; }
        public int LeccionesCompletadas { get; set; }

        public int Borradores => Cursos - Publicados;

        public List<CursoInscritos> Populares { get; set; } = new();
    }

    public class CursoInscritos
    {
        public int Id { get; set; }
        public string Titulo { get; set; } = "";
        public int Inscritos { get; set; }
    }
}
