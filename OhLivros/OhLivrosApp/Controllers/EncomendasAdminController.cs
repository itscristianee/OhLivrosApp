// Controllers/EncomendasAdminController.cs
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using OhLivrosApp.Constantes;         // Estados
using OhLivrosApp.Repositorios;      // IEncUtilizadorRepositorio
using OhLivrosApp.Models.DTO;        

namespace OhLivrosApp.Controllers;

[Authorize(Roles = "Admin")]
public class EncomendasAdminController : Controller
{
    private readonly IEncUtilizadorRepositorio _repo;

    public EncomendasAdminController(IEncUtilizadorRepositorio repo)
    {
        _repo = repo;
    }

    // GET: /EncomendasAdmin/Todas
    public async Task<IActionResult> Todas()
    {
        var encomendas = await _repo.EncomendasDoUtilizadorAsync(obterTodas: true);
        return View(encomendas);
    }

    // POST: /EncomendasAdmin/AlternarPagamento/5
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> AlternarPagamento(int id)
    {
        try
        {
            await _repo.AlternarPagamentoAsync(id);
            TempData["msg"] = "Estado de pagamento atualizado.";
        }
        catch
        {
            TempData["msg"] = "Falha ao atualizar pagamento.";
        }
        return RedirectToAction(nameof(Todas));
    }

    // GET: /EncomendasAdmin/AtualizarEstado/5
    public async Task<IActionResult> AtualizarEstado(int id)
    {
        var enc = await _repo.ObterPorIdAsync(id, incluirDetalhes: false);
        if (enc == null) return NotFound();

        var vm = new UpdateEstadoEncomendaModel
        {
            EncomendaId = id,
            Estado = enc.Estado,
            EstadosList = Enum.GetValues(typeof(Estados))
                              .Cast<Estados>()
                              .Select(e => new SelectListItem
                              {
                                  Value = ((int)e).ToString(),
                                  Text = e.ToString()
                              })
        };
        return View(vm);
    }

    // POST: /EncomendasAdmin/AtualizarEstado
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> AtualizarEstado(UpdateEstadoEncomendaModel data)
    {
        if (!ModelState.IsValid)
        {
            data.EstadosList = Enum.GetValues(typeof(Estados))
                                   .Cast<Estados>()
                                   .Select(e => new SelectListItem
                                   {
                                       Value = ((int)e).ToString(),
                                       Text = e.ToString(),
                                       Selected = e == data.Estado
                                   });
            return View(data);
        }

        try
        {
            await _repo.AtualizarEstadoAsync(data.EncomendaId, data.Estado);
            TempData["msg"] = "Estado da encomenda atualizado.";
        }
        catch
        {
            TempData["msg"] = "Falha ao atualizar o estado.";
        }

        return RedirectToAction(nameof(AtualizarEstado), new { id = data.EncomendaId });
    }
}
