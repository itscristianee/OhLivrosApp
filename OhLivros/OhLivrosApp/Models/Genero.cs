using System.ComponentModel.DataAnnotations;

namespace OhLivrosApp.Models
{
    public class Genero
    {
        /// <summary>
        /// Identificador do Género
        /// </summary>
        [Key]
        public int Id { get; set; }

        /// <summary>
        /// Nome do Género que será associada aos livros
        /// </summary>
        [Required(ErrorMessage = "A {0} é de preenchimento obrigatório")]
        [StringLength(20)]
        [Display(Name = "Género")]
        public string Nome { get; set; } = ""; // <=> string.Empty;

        /* *************************
      * Definção dos relacionamentos
      * ************************** 
      */

        /// <summary>
        /// Lista dos Livros associadas a um Género
        /// </summary>
        public ICollection<Livro> ListaLivros { get; set; } = [];  // <=> new HashSet<Livro>();  

    }

}
