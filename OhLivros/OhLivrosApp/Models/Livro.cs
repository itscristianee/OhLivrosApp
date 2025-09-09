using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
namespace OhLivrosApp.Models
{
    public class Livro
    {
        /// <summary>
        /// Identificador do livro
        /// </summary>
        [Key]
        public int Id { get; set; }

        /// <summary>
        /// Título do livro que será associada as compras
        /// </summary>
        [Required(ErrorMessage = "A {0} é de preenchimento obrigatório")]
        [StringLength(20)]
        [Display(Name = "Livro")]
        public string Titulo { get; set; } = ""; // <=> string.Empty;

        /// <summary>
        /// Caminho da imagem de capa do livro
        /// </summary>
        [StringLength(200, ErrorMessage = "O {0} não pode ter mais de {1} caracteres")]
        [Display(Name = "Imagem de Capa")]
        public string? Imagem { get; set; }

        /// <summary>
        /// Nome do autor
        /// </summary>
        [Required(ErrorMessage = "O {0} é de preenchimento obrigatório")]
        [StringLength(100, ErrorMessage = "O {0} não pode ter mais de {1} caracteres")]
        [Display(Name = "Autor")]
        public string? Autor { get; set; }

        /// <summary>
        /// Preço do livro
        /// </summary>
        [Required(ErrorMessage = "O {0} é de preenchimento obrigatório")]
        [Column(TypeName = "decimal(10,2)")]
        [Display(Name = "Preço")]
        public decimal Preco { get; set; }

        // <summary>
        /// FK para a tabela dos Géneros
        /// </summary>
        [ForeignKey(nameof(Genero))]
        [Display(Name = "Género")]
        public int GeneroFK { get; set; }

        /// <summary>
        /// FK para os Géneros
        /// </summary>
        [ValidateNever]
        public Genero Genero { get; set; } = null!;


        /// <summary>
        /// Lista de detalhes associados à encomenda
        /// </summary>
        public List<DetalheEncomenda> DetalhesEncomenda { get; set; } = [];

        /// <summary>
        /// Lista de detalhes associados ao Carrinho
        /// </summary>
        public List<DetalheCarrinho> DetalhesCarrinho { get; set; } = [];
    }
}
