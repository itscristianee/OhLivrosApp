// Data/Seed/DbInitializer.cs
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using OhLivrosApp.Constantes;
using OhLivrosApp.Models;

namespace OhLivrosApp.Data.Seed
{
    internal static class DbInitializer
    {
        // NÃO usar async void. Use Task.
        internal static async Task InitializeAsync(IServiceProvider services)
        {
            using var scope = services.CreateScope();

            var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
            var userManager = scope.ServiceProvider.GetRequiredService<UserManager<IdentityUser>>();
            var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>();
            var config = scope.ServiceProvider.GetRequiredService<IConfiguration>();

            // 0) Aplicar migrations pendentes
            await db.Database.MigrateAsync();

            // 1) Garantir roles
            var roles = new[] { nameof(Perfis.Utilizador), nameof(Perfis.Administrador) };
            foreach (var role in roles)
                if (!await roleManager.RoleExistsAsync(role))
                    _ = await roleManager.CreateAsync(new IdentityRole(role));

            // 2) Criar/garantir utilizador Admin (Identity)
            var adminEmail = config["Admin:Email"] ?? "admin@admin.com";
            var adminPass = config["Admin:Password"] ?? "Admin#12345";
            var adminNome = config["Admin:Nome"] ?? "Administrador";

            var admin = await userManager.FindByEmailAsync(adminEmail);
            if (admin is null)
            {
                admin = new IdentityUser
                {
                    UserName = adminEmail,
                    Email = adminEmail,
                    EmailConfirmed = true
                };

                var create = await userManager.CreateAsync(admin, adminPass);
                if (!create.Succeeded)
                    throw new InvalidOperationException(
                        "Falha a criar utilizador admin: " +
                        string.Join("; ", create.Errors.Select(e => e.Description)));
            }

            if (!await userManager.IsInRoleAsync(admin, nameof(Perfis.Administrador)))
                await userManager.AddToRoleAsync(admin, nameof(Perfis.Administrador));

            // 3) Registo correspondente na tua tabela Utilizadores (ponte via UserName = IdentityId)
            if (!await db.Utilizadores.AnyAsync(u => u.UserName == admin.Id))
            {
                db.Utilizadores.Add(new Utilizador
                {
                    UserName = admin.Id,   // guarda o Id do Identity
                    Nome = adminNome,
                    Morada = "—",
                    CodPostal = "0000-000",
                    Pais = "Portugal",
                    Telemovel = "000000000",
                    NIF = "123456789"
                });
                await db.SaveChangesAsync();
            }
        }
    }
}
