using System.ComponentModel.DataAnnotations;

namespace OhLivrosApp.Constantes
{
    public enum MetodosPagamento
    {
        [Display(Name = "MBWay")]
        MBWay = 1,

        [Display(Name = "Cartão de Crédito")]
        CartaoCredito,

        [Display(Name = "Multibanco")]
        Multibanco,

        [Display(Name = "Transferência")]
        Transferencia
    }
}
