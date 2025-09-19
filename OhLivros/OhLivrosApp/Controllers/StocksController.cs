using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using OhLivrosApp.Constantes;
using OhLivrosApp.Models.DTO;
using OhLivrosApp.Repositorios;

namespace OhLivrosApp.Controllers
{
    [Authorize(Roles = nameof(Perfis.Administrador))]
    public class StocksController : Controller
    {
        private readonly IStockRepositorio _stockRepo;

        public StocksController(IStockRepositorio stockRepo)
        {
            _stockRepo = stockRepo;
        }

        public async Task<IActionResult> Index(string termo = "")
        {
            var stocks = await _stockRepo.ObterStocksAsync(termo);
            return View(stocks);
        }

        public async Task<IActionResult> GerirStock(int livroId)
        {
            var existente = await _stockRepo.ObterStockPorLivroIdAsync(livroId);
            var stock = new StockDTO
            {
                LivroFK = livroId,
                Quantidade = existente != null ? existente.Quantidade : 0
            };
            return View(stock);
        }

        [HttpPost]
        public async Task<IActionResult> GerirStock(StockDTO stock)
        {
            if (!ModelState.IsValid)
                return View(stock);

            try
            {
                await _stockRepo.GerirStockAsync(stock);
                TempData["successMessage"] = "Stock atualizado com sucesso.";
            }
            catch (Exception)
            {
                TempData["errorMessage"] = "Ocorreu um erro ao atualizar o stock.";
            }

            return RedirectToAction(nameof(Index));
        }
    }
}
