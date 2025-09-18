using System.ComponentModel.DataAnnotations;

namespace OhLivrosApp.Models.DTO
{
    public class CheckoutModelDTO
    {
        // este campo é o ÚNICO para submeter no form
        [Required(ErrorMessage = "O {0} é obrigatório")]
        [MaxLength(30)]
        [Display(Name = "Método de Pagamento")]
        public string? MetodoPagamento { get; set; }

        public Utilizador Utilizador { get; set; } = null!;
        public List<ResumoCarrinhoItem> Itens { get; set; } = new();

        public decimal Total => Itens.Sum(i => i.Subtotal);
    }

    public class ResumoCarrinhoItem
    {
        public string Titulo { get; set; } = "";
        public int Quantidade { get; set; }
        public decimal PrecoUnitario { get; set; }
        public decimal Subtotal => Quantidade * PrecoUnitario;
    }
}