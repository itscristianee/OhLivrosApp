using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using OhLivrosApp.Repositorios;
using OhLivrosApp.Models.DTO;
using Microsoft.Extensions.Logging;

namespace OhLivrosApp.Controllers
{
    [Authorize]
    public class EncomendasController : Controller
    {
        private readonly IEncUtilizadorRepositorio _repo;
        private readonly ILogger<EncomendasController> _logger;

        public EncomendasController(IEncUtilizadorRepositorio repo, ILogger<EncomendasController> logger)
        {
            _repo = repo;
            _logger = logger;
        }

        // GET /Encomendas/Minhas  (equivalente ao GetEncomendas)
        public async Task<IActionResult> Minhas()
        {
            var encomendas = await _repo.EncomendasDoUtilizadorAsync();
            return View(encomendas); 
        }

        // GET /Encomendas/Detalhe/5
        public async Task<IActionResult> Detalhe(int id)
        {
            var enc = await _repo.ObterPorIdAsync(id);
            if (enc == null) return NotFound();
            return View(enc); 
        }

        // POST /Encomendas/AlternarPagamento/5  (igual ao TogglePaymentStatus)
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AlternarPagamento(int id)
        {
            try
            {
                await _repo.AlternarPagamentoAsync(id);
                TempData["successMessage"] = "Estado de pagamento alterado.";
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao alternar pagamento da encomenda {Id}", id);
                TempData["errorMessage"] = "Não foi possível alterar o estado de pagamento.";
            }
            return RedirectToAction(nameof(Detalhe), new { id });
        }

        // POST /Encomendas/AtualizarEstado  (igual ao ChangeOrderStatus, tipicamente admin)
        [HttpPost]
        [Authorize(Roles = "Administrador")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AtualizarEstado(AtualizarEstadoEncomendaModel data)
        {
            try
            {
                await _repo.AtualizarEstadoAsync(data.EncomendaId, data.Estado);
                TempData["successMessage"] = "Estado da encomenda atualizado.";
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao atualizar estado da encomenda {Id}", data.EncomendaId);
                TempData["errorMessage"] = "Não foi possível atualizar o estado.";
            }
            return RedirectToAction(nameof(Detalhe), new { id = data.EncomendaId });
        }

        // (Opcional) lista de todas as encomendas para backoffice
        [Authorize(Roles = "Administrador")]
        public async Task<IActionResult> Todas()
        {
            var encs = await _repo.EncomendasDoUtilizadorAsync(obterTodas: true);
            return View(encs); // podes reutilizar a mesma view da lista
        }
    }
}
