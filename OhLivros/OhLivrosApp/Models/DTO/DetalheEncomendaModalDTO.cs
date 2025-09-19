using System.Collections.Generic;
using System.Linq;

namespace OhLivrosApp.Models.DTOs
{
    public class DetalheEncomendaModalDTO
    {
        public string IdDiv { get; set; } = string.Empty;
        public IEnumerable<DetalheEncomenda> Detalhes { get; set; } = Enumerable.Empty<DetalheEncomenda>();
    }
}
