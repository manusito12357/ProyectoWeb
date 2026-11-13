namespace proyectoweb.Models
{
    public class FiltroCatalogo
    {
        public static readonly (string Clave, string Texto)[] Duraciones =
        {
            ("corta", "Menos de 2 h"),
            ("media", "2 a 5 h"),
            ("larga", "Más de 5 h")
        };

        public static readonly (string Clave, string Texto)[] Ordenes =
        {
            ("recientes", "Más recientes"),
            ("titulo", "Título"),
            ("duracion", "Duración")
        };

        public int? CategoriaId { get; set; }
        public string? Buscar { get; set; }
        public List<int> Niveles { get; set; } = new();
        public string? Duracion { get; set; }
        public string Orden { get; set; } = "recientes";

        public bool Activo =>
            CategoriaId != null || !string.IsNullOrEmpty(Buscar) || Niveles.Any() || !string.IsNullOrEmpty(Duracion);

        public void Normalizar()
        {
            Buscar = Buscar?.Trim();
            if (Buscar?.Length > 150)
            {
                Buscar = Buscar[..150];
            }
            Niveles = Niveles.Where(n => Enum.IsDefined(typeof(NivelCurso), n)).Distinct().OrderBy(n => n).ToList();
            if (!Duraciones.Any(d => d.Clave == Duracion))
            {
                Duracion = null;
            }
            if (!Ordenes.Any(o => o.Clave == Orden))
            {
                Orden = "recientes";
            }
        }

        public void AlternarNivel(int nivel)
        {
            if (!Niveles.Remove(nivel))
            {
                Niveles.Add(nivel);
            }
        }

        // Devuelve la parte "?a=b&c=d" de la dirección con los cambios pedidos.
        public string Consulta(Action<FiltroCatalogo>? cambiar = null, int pagina = 1)
        {
            var copia = new FiltroCatalogo
            {
                CategoriaId = CategoriaId,
                Buscar = Buscar,
                Niveles = new List<int>(Niveles),
                Duracion = Duracion,
                Orden = Orden
            };
            cambiar?.Invoke(copia);

            var partes = new List<KeyValuePair<string, string?>>();
            if (copia.CategoriaId != null)
            {
                partes.Add(new("categoriaId", copia.CategoriaId.ToString()));
            }
            if (!string.IsNullOrEmpty(copia.Buscar))
            {
                partes.Add(new("buscar", copia.Buscar));
            }
            foreach (var nivel in copia.Niveles.OrderBy(n => n))
            {
                partes.Add(new("niveles", nivel.ToString()));
            }
            if (!string.IsNullOrEmpty(copia.Duracion))
            {
                partes.Add(new("duracion", copia.Duracion));
            }
            if (copia.Orden != "recientes")
            {
                partes.Add(new("orden", copia.Orden));
            }
            if (pagina > 1)
            {
                partes.Add(new("pagina", pagina.ToString()));
            }
            return QueryString.Create(partes).ToString();
        }
    }

    public class CatalogoViewModel
    {
        public FiltroCatalogo Filtro { get; set; } = new();
        public List<CursoCatalogo> Cursos { get; set; } = new();
        public List<CategoriaConteo> Categorias { get; set; } = new();
        public Dictionary<NivelCurso, int> ConteoNiveles { get; set; } = new();
        public Dictionary<string, int> ConteoDuraciones { get; set; } = new();
        public int Total { get; set; }
        public int Pagina { get; set; }
        public int TotalPaginas { get; set; }
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
