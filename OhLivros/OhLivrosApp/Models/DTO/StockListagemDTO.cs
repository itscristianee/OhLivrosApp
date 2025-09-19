namespace OhLivrosApp.Models.DTO
{
    public class StockListagemDTO
    {
        public int Id { get; set; }
        public int LivroFK { get; set; }
        public int Quantidade { get; set; }
        public string? TituloLivro { get; set; }
    }
}
