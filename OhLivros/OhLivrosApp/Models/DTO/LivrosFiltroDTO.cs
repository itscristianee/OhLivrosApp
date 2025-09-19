namespace OhLivrosApp.Models.DTO
{
    public class LivrosFiltroDTO
    {
        public IEnumerable<Livro> Livros { get; set; } = Enumerable.Empty<Livro>();
        public IEnumerable<Genero> Generos { get; set; } = Enumerable.Empty<Genero>();

        // termo de pesquisa
        public string Termo { get; set; } = string.Empty;

        // filtro de género (0 = todos)
        public int GeneroFK { get; set; } = 0;
    }
}
