using System.ComponentModel.DataAnnotations;

namespace OhLivrosApp.Models.DTO
{
    public class StockDTO
    {
        [Required(ErrorMessage = "O livro é obrigatório.")]
        public int LivroFK { get; set; }

        [Range(0, int.MaxValue, ErrorMessage = "A quantidade tem de ser um valor não negativo.")]
        public int Quantidade { get; set; }
    }
}
