using Microsoft.EntityFrameworkCore;
using OhLivrosApp.Data;
using OhLivrosApp.Models;

namespace OhLivrosApp.Repositorios
{
    public interface ILivroRepositorio
    {
        Task AdicionarAsync(Livro livro);
        Task AtualizarAsync(Livro livro);
        Task RemoverAsync(Livro livro);
        Task<Livro?> ObterPorIdAsync(int id);
        Task<IEnumerable<Livro>> ObterTodosAsync();
    }

    public class LivroRepositorio : ILivroRepositorio
    {
        private readonly ApplicationDbContext _context;

        public LivroRepositorio(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task AdicionarAsync(Livro livro)
        {
            _context.Livros.Add(livro);
            await _context.SaveChangesAsync();
        }

        public async Task AtualizarAsync(Livro livro)
        {
            _context.Livros.Update(livro);
            await _context.SaveChangesAsync();
        }

        public async Task RemoverAsync(Livro livro)
        {
            _context.Livros.Remove(livro);
            await _context.SaveChangesAsync();
        }

        public async Task<Livro?> ObterPorIdAsync(int id)
        {
            return await _context.Livros
                                 .Include(l => l.Genero)
                                 .FirstOrDefaultAsync(l => l.Id == id);
        }

        public async Task<IEnumerable<Livro>> ObterTodosAsync()
        {
            return await _context.Livros
                                 .Include(l => l.Genero)
                                 .AsNoTracking()
                                 .ToListAsync();
        }
    }
}
