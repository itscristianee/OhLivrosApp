using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace OhLivrosApp.Models
{
    public class DetalheCarrinho
    {
        /// <summary>
        /// Identificador do detalhe do carrinho
        /// </summary>
        [Key]
        public int Id { get; set; }

        /// <summary>
        /// Quantidade do livro no carrinho
        /// </summary>
        [Required(ErrorMessage = "A {0} é de preenchimento obrigatório")]
        [Display(Name = "Quantidade")]
        public int Quantidade { get; set; }

        /// <summary>
        /// Preço unitário do livro
        /// </summary>
        [Required(ErrorMessage = "O {0} é de preenchimento obrigatório")]
        [Display(Name = "Preço Unitário")]
        [DataType(DataType.Currency)]
        public double PrecoUnitario { get; set; }

        /// <summary>
        /// FK para o livro associado
        /// </summary>
        [ForeignKey(nameof(Livro))]
        public int LivroFK { get; set; }
      
        /// <summary>
        /// Livro associado ao detalhe
        /// </summary>
        [ValidateNever]
        public Livro Livro { get; set; } = null!;

        /// <summary>
        /// FK para o carrinho associado
        /// </summary>
        [ForeignKey(nameof(Carrinho))]
        public int CarrinhoFK { get; set; }
       
        /// <summary>
        /// Carrinho associado ao detalhe
        /// </summary>
        [ValidateNever]
        public Carrinho Carrinho { get; set; } = null!;
    }
}
