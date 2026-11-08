using Microsoft.AspNetCore.Identity;

namespace proyectoweb.Data
{
    public static class DbInicializador
    {
        private static readonly string[] Roles = { "Administrador", "Suscriptor" };

        public static async Task InicializarAsync(IServiceProvider servicios, IConfiguration config)
        {
            var roleManager = servicios.GetRequiredService<RoleManager<IdentityRole>>();
            var userManager = servicios.GetRequiredService<UserManager<IdentityUser>>();

            foreach (var rol in Roles)
            {
                if (!await roleManager.RoleExistsAsync(rol))
                {
                    await roleManager.CreateAsync(new IdentityRole(rol));
                }
            }

            var email = config["Admin:Email"];
            var password = config["Admin:Password"];
            if (string.IsNullOrEmpty(email) || string.IsNullOrEmpty(password))
            {
                return;
            }

            var admin = await userManager.FindByEmailAsync(email);
            if (admin == null)
            {
                admin = new IdentityUser { UserName = email, Email = email, EmailConfirmed = true };
                var resultado = await userManager.CreateAsync(admin, password);
                if (!resultado.Succeeded)
                {
                    var errores = string.Join(", ", resultado.Errors.Select(e => e.Description));
                    throw new InvalidOperationException("No se pudo crear el administrador: " + errores);
                }
            }

            if (!await userManager.IsInRoleAsync(admin, "Administrador"))
            {
                await userManager.AddToRoleAsync(admin, "Administrador");
            }
        }
    }
}
