using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace OhLivrosApp.Models
{
    
    public class Encomenda
    {
        /// <summary>
        /// Identificador da encomenda
        /// </summary>
        [Key]
        public int Id { get; set; }

        /// <summary>
        /// Data de criação da encomenda
        /// </summary>
        [Display(Name = "Data de Criação")]
        public DateTime DataCriacao { get; set; } = DateTime.UtcNow;

        /// <summary>
        /// Estado lógico (eliminado ou não)
        /// </summary>
        [Display(Name = "Eliminado")]
        public bool Eliminado { get; set; } = false;

       
        /// <summary>
        /// Método de pagamento escolhido
        /// </summary>
        [Required(ErrorMessage = "O {0} é de preenchimento obrigatório")]
        [MaxLength(30, ErrorMessage = "O {0} não pode ter mais de {1} caracteres")]
        [Display(Name = "Método de Pagamento")]
        public string? MetodoPagamento { get; set; }

        /// <summary>
        /// Indica se a encomenda já foi paga
        /// </summary>
        [Display(Name = "Pago")]
        public bool Pago { get; set; }

        /// <summary>
        /// Estado atual da encomenda
        /// </summary>
        [Display(Name = "Estado da Encomenda")]
        public Estados Estado { get; set; }

        /// <summary>
        /// Estado do pagamento (texto calculado, não mapeado na BD)
        /// </summary>
        [NotMapped]
        [Display(Name = "Estado do Pagamento")]
        public string EstadoPagamento => Pago ? "Pago" : "Não Pago";

        // Relacionamentos 1-N

        /// <summary>
        /// FK para referenciar o comprador da Encomenda
        /// </summary>
        [ForeignKey(nameof(Comprador))]
        public int CompradorFK { get; set; }
        /// <summary>
        /// FK para referenciar o Comprador da Encomenda
        /// </summary>
        [ValidateNever]
        public Utilizador Comprador { get; set; } = null!;

        /// <summary>
        /// Lista de detalhes associados à encomenda
        /// </summary>
        public List<DetalheEncomenda> DetalhesEncomenda { get; set; } = [];
    }

    

}
