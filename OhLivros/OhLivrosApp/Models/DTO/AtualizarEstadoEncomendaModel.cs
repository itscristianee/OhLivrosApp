using OhLivrosApp.Constantes;

namespace OhLivrosApp.Models.DTO
{
    public class AtualizarEstadoEncomendaModel
    {
        public int EncomendaId { get; set; }
        public Estados Estado { get; set; }
    }
}
