namespace OhLivrosApp.Models.DTO
{
    /// <summary>
    /// DTO (Data Transfer Object) usado na View Index da Home.
    /// Objetivo:
    /// - Agrupar numa única classe os dados necessários para exibir a página inicial:
    ///   - Lista de livros (já filtrados).
    ///   - Lista de géneros (para preencher o dropdown).
    ///   - Termo de pesquisa e género selecionado (para manter estado da pesquisa).
    /// 
    /// Vantagens:
    /// - Evita usar múltiplos objetos separados na View.
    /// - Centraliza as informações da página num único "pacote".
    /// </summary>
    public class LivroModelDTO
    {
        /// <summary>
        /// Coleção de livros a serem exibidos na tela.
        /// </summary>
        public IEnumerable<Livro> Livros { get; set; }

        /// <summary>
        /// Coleção de géneros disponíveis (dropdown).
        /// </summary>
        public IEnumerable<Genero> Generos { get; set; }

        /// <summary>
        /// Termo de pesquisa digitado pelo utilizador (filtro por título).
        /// </summary>
        public string Termo { get; set; } = "";

        /// <summary>
        /// Género escolhido no filtro (0 significa "todos").
        /// </summary>
        public int GeneroId { get; set; } = 0;
    }
}
