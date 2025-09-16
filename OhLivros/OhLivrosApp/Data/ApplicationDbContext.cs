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

        // especificar as tabelas associadas à BD

        /// <summary>
        /// tabela Generos na BD
        /// </summary>
        public DbSet<Genero> Generos { get; set; }

        /// <summary>
        /// tabela Livros na BD
        /// </summary>
        public DbSet<Livro> Livros { get; set; }

        /// <summary>
        /// tabela Utilizadores na BD
        /// </summary>
        public DbSet<Utilizador> Utilizadores { get; set; }

        /// <summary>
        /// tabela Carrinhos na BD
        /// </summary>
        public DbSet<Carrinho> Carrinhos { get; set; }

        /// <summary>
        /// tabela DetalhesCarrinho na BD
        /// </summary>
        public DbSet<DetalheCarrinho> DetalhesCarrinho { get; set; }

        /// <summary>
        /// tabela Encomendas na BD
        /// </summary>
        public DbSet<Encomenda> Encomendas { get; set; }

        /// <summary>
        /// tabela DetalhesEncomenda na BD
        /// </summary>
        public DbSet<DetalheEncomenda> DetalhesEncomenda { get; set; }
    }
}
