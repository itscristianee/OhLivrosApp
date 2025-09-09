using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using OhLivrosApp.Models;

namespace OhLivrosApp.Data
{
    public class ApplicationDbContext : IdentityDbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        public DbSet<Genero> Generos { get; set; }
        public DbSet<Livro> Livros { get; set; }
        public DbSet<Utilizador> Utilizadores { get; set; }
        public DbSet<Carrinho> Carrinhos { get; set; }
        public DbSet<DetalheCarrinho> DetalhesCarrinho { get; set; }
        public DbSet<Encomenda> Encomendas { get; set; }
        public DbSet<DetalheEncomenda> DetalhesEncomenda { get; set; }
    }
}
