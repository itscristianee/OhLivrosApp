using OhLivrosApp.Models;

namespace OhLivrosApp
{
    public interface IHomeRepositorio
    {
        Task<IEnumerable<Livro>> GetLivros(string termo = "", int generoId = 0);
        Task<IEnumerable<Genero>> GetGeneros();
    }
}