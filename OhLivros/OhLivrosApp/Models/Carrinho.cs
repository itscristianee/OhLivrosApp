using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace OhLivrosApp.Models
{
    public class Carrinho
    {
        /// <summary>
        /// Identificador do Carrinho
        /// </summary>
        [Key]
        public int Id { get; set; }

        /// <summary>
        /// Estado lógico do carrinho (se foi eliminado ou não)
        /// </summary>
        [Display(Name = "Eliminado")]
        public bool Eliminado { get; set; } = false;


        /// <summary>
        /// Chave estrangeira para o Utilizador dono do carrinho
        /// </summary>
        [ForeignKey(nameof(Utilizador))]
        [Display(Name = "Utilizador")]
        [Required(ErrorMessage = "O {0} é de preenchimento obrigatório")]
        public int DonoFK { get; set; }

        /// <summary>
        /// Navegação para o Utilizador Dono associado ao carrinho
        /// </summary>
        [ValidateNever]
        public Utilizador Dono { get; set; } = null!;

        /// <summary>
        /// Lista de detalhes associados ao carrinho
        /// </summary>
        public List<DetalheCarrinho> DetalhesCarrinho { get; set; } = [];


    }
}
