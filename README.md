# Portal de microcursos

Portal web para publicar microcursos y tutoriales técnicos. Cada curso tiene varias lecciones escritas en texto o HTML sencillo.

- Cualquier persona puede ver el catálogo (título, descripción, nivel y duración), pero no puede leer las lecciones.
- El suscriptor se registra, se inscribe gratis en un curso, lee sus lecciones, las marca como completadas y ve su progreso.
- El administrador crea categorías, cursos y lecciones, los publica o los retira del catálogo y consulta quiénes se inscribieron en cada curso.

El acceso a las lecciones se comprueba en el servidor: si el usuario no está inscrito, no puede abrirlas aunque conozca la dirección.

## Requisitos

- .NET 8 SDK
- SQL Server (instancia local)
- La herramienta `dotnet-ef`: `dotnet tool install --global dotnet-ef`

## Cómo ejecutarlo

1. Clona el repositorio y entra a la carpeta del proyecto:

   ```
   cd proyectoweb
   ```

2. Configura tu conexión y el administrador en `appsettings.Development.json`:

   ```json
   {
     "ConnectionStrings": {
       "DefaultConnection": "Server=localhost;Database=WebProyecto;Trusted_Connection=True;TrustServerCertificate=True;"
     },
     "Admin": {
       "Email": "admin@ejemplo.com",
       "Password": "una contraseña con mayúscula, minúscula, número y símbolo"
     }
   }
   ```

   Cambia el servidor si tu instancia es otra, por ejemplo `.\SQLEXPRESS`. No subas tus datos reales al repositorio.

3. Crea las tablas:

   ```
   dotnet ef database update
   ```

4. Ejecuta la aplicación:

   ```
   dotnet run
   ```

Al arrancar se crean los roles `Administrador` y `Suscriptor`, y el usuario administrador si la sección `Admin` está completa.

## Roles

| Rol | Qué puede hacer |
|---|---|
| Invitado | Ver el catálogo y la página de presentación de cada curso |
| Suscriptor | Inscribirse, ver Mis cursos, leer lecciones, marcarlas como completadas y ver su progreso |
| Administrador | Gestionar categorías, cursos y lecciones, publicar y despublicar, y ver los inscritos por curso |

Quien se registra recibe el rol Suscriptor automáticamente.

## Estructura

- `Controllers/`: controladores MVC
- `Models/`: modelos y el enum `NivelCurso`
- `Views/`: vistas Razor
- `Data/`: `AppDbContext` y la inicialización de roles
- `Areas/Identity/`: páginas de inicio de sesión y registro
- `Migrations/`: migraciones de Entity Framework
