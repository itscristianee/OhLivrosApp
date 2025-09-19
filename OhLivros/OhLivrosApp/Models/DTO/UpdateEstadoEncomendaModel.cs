// Models/DTO/UpdateEstadoEncomendaModel.cs
using Microsoft.AspNetCore.Mvc.Rendering;
using OhLivrosApp.Constantes;
using System.ComponentModel.DataAnnotations;

namespace OhLivrosApp.Models.DTO
{
    public class UpdateEstadoEncomendaModel
    {
        [Required]
        public int EncomendaId { get; set; }

        [Required]
        [Display(Name = "Estado")]
        public Estados Estado { get; set; }

        public IEnumerable<SelectListItem>? EstadosList { get; set; }
    }
}
