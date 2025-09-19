// Models/ViewModels/AtualizarEstadoEncomendaModel.cs
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc.Rendering;
using OhLivrosApp.Constantes;

public class AtualizarEstadoEncomendaModel
{
    public int EncomendaId { get; set; }

    [Required(ErrorMessage = "Selecione um estado.")]
    [Display(Name = "Estado da encomenda")]
    public Estados Estado { get; set; }

    public IEnumerable<SelectListItem> ListaEstados { get; set; } = new List<SelectListItem>();
}
