using Microsoft.EntityFrameworkCore;
using OhLivrosApp.Data;
using OhLivrosApp.Models;
using System.IO;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory;
using static System.Reflection.Metadata.BlobBuilder;

namespace OhLivrosApp.Repositorios
{
    
    /*
     * Objetivo deste repositório:
     * ---------------------------
     * - Obter a lista de livros existentes na base de dados.
     * - Permitir ao utilizador aplicar filtros:
     *    - procurar livros por título (prefixo introduzido numa caixa de pesquisa)
     *    - restringir resultados a um género específico
     * - Devolver os dados já preparados para a camada de apresentação (View).
     */

    /// <summary>
    /// Repositório de leitura da Home (lista livros e géneros).
    /// </summary>
    public class HomeRepositorio : IHomeRepositorio
    {
        private readonly ApplicationDbContext _context;
        public HomeRepositorio(ApplicationDbContext context)
        {
            _context = context;
        }

        // <summary>
        /// Lista livros filtrando por título e género.
        /// </summary>
        /// <param name="termo">Prefixo do título (case-insensitive).</param>
        /// <param name="generoId">Id do género; 0 para todos.</param>
        /// <returns>Lista de livros com o género carregado.</returns>
        public async Task<IEnumerable<Livro>> GetLivros(string termo = "", int generoId = 0) 
        {
            // Criar query base → livros + respetivo género (join com tabela Generos)

            var query = _context.Livros
                   .AsNoTracking()               // leitura mais rápida (não altera objetos)
                   .Include(l => l.Genero)      // inclui dados do género de cada livro
                   .AsQueryable();

            // Se foi fornecido um termo → filtrar por título que começa por esse texto
            if (!string.IsNullOrWhiteSpace(termo))
            {
                var t = termo.Trim();
                // mais eficiente que ToLower(): usa LIKE
                query = query.Where(l => EF.Functions.Like(l.Titulo, t + "%"));
            }

            // Se foi escolhido um género → filtrar apenas os livros desse género
            if (generoId > 0)
            {
                query = query.Where(l => l.GeneroFK == generoId);
            }

            // Executar a query → devolver lista de livros com os campos necessários
            var livros = await query
                .Select(livro => new Livro
                {
                    Id = livro.Id,
                    Imagem = livro.Imagem,
                    Autor = livro.Autor,
                    Titulo = livro.Titulo,
                    GeneroFK = livro.GeneroFK,
                    Preco = livro.Preco
                }).ToListAsync();

            return livros;
        }


        /// <summary>
        /// Devolve a lista de géneros (ordenada por nome) para preencher o dropdown.
        /// </summary>
        public async Task<IEnumerable<Genero>> GetGeneros()
        {
            return await _context.Generos
                             .AsNoTracking()
                             .OrderBy(g => g.Nome)
                             .ToListAsync();;
        }
}
}