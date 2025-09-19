using OhLivrosApp.Constantes;
using OhLivrosApp.Models;

namespace OhLivrosApp.Repositorios
{
    public interface IEncUtilizadorRepositorio
    {
        Task<IEnumerable<Encomenda>> EncomendasDoUtilizadorAsync(bool obterTodas = false);
        Task<Encomenda?> ObterPorIdAsync(int id, bool incluirDetalhes = true);
        Task AlternarPagamentoAsync(int encomendaId);
        Task AtualizarEstadoAsync(int encomendaId, Estados novoEstado);
    }
}