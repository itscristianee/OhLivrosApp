using System.ComponentModel.DataAnnotations;

namespace OhLivrosApp.Models.DTO.Api
{
    public record LivroListDTO(int Id, string Titulo, string Autor, string Genero, decimal Preco, string? ImagemUrl);

    public class LivroCreateDTO
    {
        [Required] public string Titulo { get; set; } = "";
        [Required] public string Autor { get; set; } = "";
        [Required] public int GeneroFK { get; set; }
        [Range(0, 9999)] public decimal Preco { get; set; }
        public string? Imagem { get; set; } // opcional se quiseres aceitar o nome do ficheiro
    }

    public class LivroUpdateDTO : LivroCreateDTO { }
}
