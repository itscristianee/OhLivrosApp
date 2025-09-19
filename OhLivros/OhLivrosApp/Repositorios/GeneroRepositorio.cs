using Microsoft.EntityFrameworkCore;
using OhLivrosApp.Data;
using OhLivrosApp.Models;

namespace OhLivrosApp.Repositorios;

public interface IGeneroRepositorio
{
    Task AdicionarAsync(Genero genero);
    Task AtualizarAsync(Genero genero);
    Task<Genero?> ObterPorIdAsync(int id);
    Task RemoverAsync(Genero genero);
    Task<IEnumerable<Genero>> ObterTodosAsync();
}

public class GeneroRepositorio : IGeneroRepositorio
{
    private readonly ApplicationDbContext _context;

    public GeneroRepositorio(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task AdicionarAsync(Genero genero)
    {
        _context.Generos.Add(genero);
        await _context.SaveChangesAsync();
    }

    public async Task AtualizarAsync(Genero genero)
    {
        _context.Generos.Update(genero);
        await _context.SaveChangesAsync();
    }

    public async Task RemoverAsync(Genero genero)
    {
        _context.Generos.Remove(genero);
        await _context.SaveChangesAsync();
    }

    public async Task<Genero?> ObterPorIdAsync(int id)
    {
        return await _context.Generos.FindAsync(id);
    }

    public async Task<IEnumerable<Genero>> ObterTodosAsync()
    {
        return await _context.Generos.AsNoTracking().ToListAsync();
    }
}

