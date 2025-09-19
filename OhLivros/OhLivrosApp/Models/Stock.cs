using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace OhLivrosApp.Models
{
    [Table("Stock")]
    public class Stock
    {
        public int Id { get; set; }

        [Column("LivroFK")]
        public int LivroFK { get; set; }

        [Range(0, int.MaxValue, ErrorMessage = "A quantidade não pode ser negativa.")]
        public int Quantidade { get; set; }

        [ForeignKey(nameof(LivroFK))]
        public Livro? Livro { get; set; }
    }
}
