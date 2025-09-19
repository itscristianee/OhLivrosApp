using System.ComponentModel.DataAnnotations;

namespace OhLivrosApp.Models.DTO
{
    public class GeneroDTO
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "O nome do género é obrigatório.")]
        [MaxLength(40, ErrorMessage = "O nome do género não pode ter mais de 40 caracteres.")]
        [Display(Name = "Género")]
        public string Nome { get; set; } = string.Empty;
    }
}
