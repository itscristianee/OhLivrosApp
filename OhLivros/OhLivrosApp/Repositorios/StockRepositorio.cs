// Repositorios/StockRepositorio.cs
using Microsoft.EntityFrameworkCore;
using OhLivrosApp.Data;
using OhLivrosApp.Models;
using OhLivrosApp.Models.DTO;

namespace OhLivrosApp.Repositorios
{
    public class StockRepositorio : IStockRepositorio
    {
        private readonly ApplicationDbContext _context;

        public StockRepositorio(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<Stock?> ObterStockPorLivroIdAsync(int livroId)
            => await _context.Stocks.FirstOrDefaultAsync(s => s.LivroFK == livroId);

        public async Task GerirStockAsync(StockDTO StockGestao)
        {
            // se não existir stock para o livro, cria; caso exista, atualiza a quantidade
            var existente = await ObterStockPorLivroIdAsync(StockGestao.LivroFK);
            if (existente is null)
            {
                var novo = new Stock { LivroFK = StockGestao.LivroFK, Quantidade = StockGestao.Quantidade };
                _context.Stocks.Add(novo);
            }
            else
            {
                existente.Quantidade = StockGestao.Quantidade;
            }

            await _context.SaveChangesAsync();
        }

        public async Task<IEnumerable<StockListagemDTO>> ObterStocksAsync(string termo = "")
        {
            var query = _context.Livros
                                .AsNoTracking()
                                .Include(l => l.Stock)
                                .AsQueryable();

            if (!string.IsNullOrWhiteSpace(termo))
            {
                // Em SQL Server a collation costuma ser case-insensitive; não preciso ToLower.
                query = query.Where(l => l.Titulo.StartsWith(termo));
            }

            return await query
                .Select(l => new StockListagemDTO
                {
                    Id = l.Stock != null ? l.Stock.Id : 0,
                    LivroFK = l.Id,
                    Quantidade = l.Stock != null ? l.Stock.Quantidade : 0,
                    TituloLivro = l.Titulo
                })
                .ToListAsync();
        }
    }

    public interface IStockRepositorio
    {
        Task<IEnumerable<StockListagemDTO>> ObterStocksAsync(string termo = "");
        Task<Stock?> ObterStockPorLivroIdAsync(int livroId);
        Task GerirStockAsync(StockDTO stock);
    }
}
