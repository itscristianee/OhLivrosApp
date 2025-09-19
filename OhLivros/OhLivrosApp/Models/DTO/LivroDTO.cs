using Microsoft.AspNetCore.Mvc.Rendering;
using System.ComponentModel.DataAnnotations;

namespace OhLivrosApp.Models.DTO
{
    public class LivroDTO
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "O título é obrigatório.")]
        [MaxLength(40, ErrorMessage = "O título não pode ter mais de 40 caracteres.")]
        [Display(Name = "Título")]
        public string Titulo { get; set; } = string.Empty;

        [Required(ErrorMessage = "O autor é obrigatório.")]
        [MaxLength(40, ErrorMessage = "O autor não pode ter mais de 40 caracteres.")]
        [Display(Name = "Autor")]
        public string Autor { get; set; } = string.Empty;

        [Required(ErrorMessage = "O preço é obrigatório.")]
        [Range(0.01, 999999.99, ErrorMessage = "O preço deve ser maior que zero.")]
        [Display(Name = "Preço")]
        public decimal Preco { get; set; }

        [Display(Name = "Imagem")]
        public string? Imagem { get; set; }

        [Required(ErrorMessage = "O género é obrigatório.")]
        [Display(Name = "Género")]
        public int GeneroFK { get; set; }

        [Display(Name = "Ficheiro de Imagem")]
        public IFormFile? ImagemFicheiro { get; set; }

        // Para dropdown de géneros
        public IEnumerable<SelectListItem>? Generos { get; set; }
    }
}
