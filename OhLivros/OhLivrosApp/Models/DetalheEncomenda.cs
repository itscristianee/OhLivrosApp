using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace OhLivrosApp.Models
{
    
    public class DetalheEncomenda
    {
        /// <summary>
        /// Identificador do detalhe da encomenda
        /// </summary>
        [Key]
        public int Id { get; set; }

        /// <summary>
        /// Quantidade do livro na encomenda
        /// </summary>
        [Required(ErrorMessage = "A {0} é de preenchimento obrigatório")]
        [Range(1, int.MaxValue, ErrorMessage = "A {0} deve ser pelo menos 1")]
        [Display(Name = "Quantidade")]
        public int Quantidade { get; set; }

        /// <summary>
        /// Preço unitário do livro
        /// </summary>
        [Required(ErrorMessage = "O {0} é de preenchimento obrigatório")]
        [Column(TypeName = "decimal(10,2)")]
        [Display(Name = "Preço Unitário")]
        public decimal PrecoUnitario { get; set; }

        /// <summary>
        /// FK para a encomenda associada
        /// </summary>
        [ForeignKey(nameof(Encomenda))]
        [Required(ErrorMessage = "A {0} é de preenchimento obrigatório")]
        [Display(Name = "Encomenda")]
        public int EncomendaFK { get; set; }

        /// <summary>
        /// Encomenda associada
        /// </summary>
        [ValidateNever]
        public Encomenda Encomenda { get; set; } = null!;

        /// <summary>
        /// FK para o livro associado
        /// </summary>
        [ForeignKey(nameof(Livro))]
        [Required(ErrorMessage = "O {0} é de preenchimento obrigatório")]
        [Display(Name = "Livro")]
        public int LivroFK { get; set; }

        /// <summary>
        /// Livro associado
        /// </summary>
        [ValidateNever]
        public Livro Livro { get; set; } = null!;
    }
}
