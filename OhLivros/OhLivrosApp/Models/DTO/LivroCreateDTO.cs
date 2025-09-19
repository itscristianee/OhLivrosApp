namespace OhLivrosApp.DTO;

public record LivroListItemDTO(int Id, string Titulo, string Autor, string Genero, decimal Preco, string? Imagem);
public record LivroDetailsDTO(int Id, string Titulo, string Autor, int GeneroFK, string Genero, decimal Preco, string? Imagem);

public class LivroCreateDTO
{
    public string Titulo { get; set; } = default!;
    public string Autor { get; set; } = default!;
    public int GeneroFK { get; set; }
    public decimal Preco { get; set; }
}

public class LivroUpdateDTO : LivroCreateDTO
{
    public int Id { get; set; }
}

public record GeneroDTO(int Id, string Nome);
