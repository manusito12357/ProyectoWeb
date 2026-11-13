namespace proyectoweb.Models
{
    public class CatalogoViewModel
    {
        public List<CursoCatalogo> Cursos { get; set; } = new();
        public List<CategoriaConteo> Categorias { get; set; } = new();
        public int? CategoriaId { get; set; }
        public string? Buscar { get; set; }
        public int Total { get; set; }
        public int Pagina { get; set; }
        public int TotalPaginas { get; set; }

        public bool Filtrado => CategoriaId != null || !string.IsNullOrEmpty(Buscar);
    }

    public class CursoCatalogo
    {
        private static readonly HashSet<string> PalabrasComunes = new()
        {
            "introducción", "curso", "del", "de", "la", "el", "a", "al", "lenguaje", "y", "en", "con", "para", "desde"
        };

        public Curso Curso { get; set; } = null!;
        public int Lecciones { get; set; }
        public int Completadas { get; set; }
        public bool Inscrito { get; set; }
        public bool Terminado { get; set; }

        public int Porcentaje => Lecciones == 0 ? 0 : Completadas * 100 / Lecciones;

        public string Iniciales
        {
            get
            {
                var palabras = Curso.Titulo.Split(' ', StringSplitOptions.RemoveEmptyEntries);
                var clave = palabras.FirstOrDefault(p => !PalabrasComunes.Contains(p.ToLower())) ?? palabras[0];
                return clave.Length < 2 ? clave.ToUpper() : char.ToUpper(clave[0]) + clave.Substring(1, 1);
            }
        }
    }

    public class CategoriaConteo
    {
        public int Id { get; set; }
        public string Nombre { get; set; } = "";
        public int Cantidad { get; set; }
    }
}
