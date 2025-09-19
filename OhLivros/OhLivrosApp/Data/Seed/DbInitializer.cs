// Data/Seed/DbInitializer.cs
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using OhLivrosApp.Models;

namespace OhLivrosApp.Data.Seed
{
    internal static class DbInitializer
    {
        internal static async Task InitializeAsync(IServiceProvider services)
        {
            using var scope = services.CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
            var userManager = scope.ServiceProvider.GetRequiredService<UserManager<IdentityUser>>();
            var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>();
            var config = scope.ServiceProvider.GetRequiredService<IConfiguration>();

            //1) Roles e Migrations
            await db.Database.MigrateAsync();

            
            var roles = new[] { "Utilizador", "Administrador" };
            foreach (var role in roles)
                if (!await roleManager.RoleExistsAsync(role))
                    _ = await roleManager.CreateAsync(new IdentityRole(role));

            // 2) Utilizador admin (Identity)
            var adminEmail = config["Admin:Email"] ?? "admin@ohlivros.pt";
            var adminPass = config["Admin:Password"] ?? "Admin#12345";

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
                        "Falha ao criar admin: " +
                        string.Join("; ", create.Errors.Select(e => e.Description)));
            }
            if (!await userManager.IsInRoleAsync(admin, "Administrador"))
                await userManager.AddToRoleAsync(admin, "Administrador");

            // 2b) Utilizador normal (Identity)
            var userEmail = config["User:Email"] ?? "user@ohlivros.pt";
            var userPass = config["User:Password"] ?? "User#12345";

            var user = await userManager.FindByEmailAsync(userEmail);
            if (user is null)
            {
                user = new IdentityUser
                {
                    UserName = userEmail,
                    Email = userEmail,
                    EmailConfirmed = true
                };
                var createUser = await userManager.CreateAsync(user, userPass);
                if (!createUser.Succeeded)
                    throw new InvalidOperationException(
                        "Falha ao criar utilizador normal: " +
                        string.Join("; ", createUser.Errors.Select(e => e.Description)));
            }
            if (!await userManager.IsInRoleAsync(user, "Utilizador"))
                await userManager.AddToRoleAsync(user, "Utilizador");


            // 3) Géneros (somente Nome)
            if (!await db.Generos.AnyAsync())
            {
                db.Generos.AddRange(
                    new Genero { Nome = "Ficção" },
                    new Genero { Nome = "Fantasia" },
                    new Genero { Nome = "Romance" },
                    new Genero { Nome = "Tecnologia" },
                    new Genero { Nome = "Negócios" },
                    new Genero { Nome = "Ciência" },
                    new Genero { Nome = "História" },
                    new Genero { Nome = "Mistério" },
                    new Genero { Nome = "Infantil" }
                );
                await db.SaveChangesAsync();
            }

            // Mapa de géneros por nome
            var gen = await db.Generos.ToDictionaryAsync(g => g.Nome);

            // 4) Livros (somente campos existentes)
            if (!await db.Livros.AnyAsync())
            {
                var livros = new List<Livro>
                {
                    new Livro {
                        Titulo = "Clean Code",
                        Autor  = "Robert C. Martin",
                        Preco  = 39.90m,
                        Quantidade = 12,
                        Imagem = null,                // opcional
                        GeneroFK = gen["Tecnologia"].Id
                    },
                    new Livro {
                        Titulo = "O Senhor dos Anéis",
                        Autor  = "J. R. R. Tolkien",
                        Preco  = 29.90m,
                        Quantidade = 7,
                        Imagem = null,
                        GeneroFK = gen["Fantasia"].Id
                    },
                    new Livro {
                        Titulo = "Sapiens",
                        Autor  = "Yuval N. Harari",
                        Preco  = 24.90m,
                        Quantidade = 9,
                        Imagem = null,
                        GeneroFK = gen["História"].Id
                    },
                    new Livro {
                        Titulo = "Pai Rico Pai Pobre",
                        Autor  = "Robert Kiyosaki",
                        Preco  = 18.90m,
                        Quantidade = 15,
                        Imagem = null,
                        GeneroFK = gen["Negócios"].Id
                    }
                };

                // Respeita o [StringLength(20)] do Titulo
                foreach (var l in livros)
                    if (l.Titulo.Length > 20)
                        l.Titulo = l.Titulo.Substring(0, 20);

                db.Livros.AddRange(livros);
                await db.SaveChangesAsync();
            }
        }
    }
}
