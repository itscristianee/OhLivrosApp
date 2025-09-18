using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using OhLivrosApp.Models;
using OhLivrosApp.Models.DTO;
using OhLivrosApp.Repositorios;
using System.Net;

namespace OhLivrosApp.Controllers
{
    /// <summary>
    /// Controller do Carrinho:
    /// - Adicionar/Remover livros
    /// - Ver carrinho do utilizador
    /// - Obter total de itens (para badge)
    /// - Checkout (cria Encomenda e limpa carrinho)
    /// 
    /// Requer utilizador autenticado.
    /// </summary>
    [Authorize]
    public class CarrinhosController : Controller
    {
        private readonly ICarrinhoRepositorio _carrinhoRepo;
        private readonly ILogger<CarrinhosController> _logger;

        public CarrinhosController(ICarrinhoRepositorio carrinhoRepo, ILogger<CarrinhosController> logger)
        {
            _carrinhoRepo = carrinhoRepo;
            _logger = logger;
        }

        /// <summary>
        /// Adiciona um livro ao carrinho do utilizador.
        /// - Se redirect==0 devolve 200 OK com o total de itens (para Ajax)
        /// - Caso contrário, redireciona para a página do carrinho
        /// </summary
        public async Task<IActionResult> AdicionarItem(int livroId, int qtd = 1, int redirect = 0)
        {
            try
            {
                var total = await _carrinhoRepo.AdicionarItem(livroId, qtd);

                // Ajax: devolve número total no carrinho
                if (redirect == 0) return Json(new { sucesso = true, total }); 

                TempData["successMessage"] = "Livro adicionado ao carrinho.";
                return RedirectToAction(nameof(MeuCarrinho));
            }
            catch (Exception ex)
            {
                // ponto crítico: qualquer falha no repositório
                _logger.LogError(ex, "Erro a adicionar item ao carrinho");
                if (redirect == 0)
                    return StatusCode(500, new { sucesso = false, erro = "Falha ao adicionar item." }); // <-- JSON em erro

                TempData["errorMessage"] = "Ocorreu um erro ao adicionar o item ao carrinho.";
                return RedirectToAction(nameof(MeuCarrinho));
            }

           
        }

        /// <summary>
        /// Remove 1 unidade do livro indicado. Se ficar a zero, remove o item.
        /// </summary>
        
        public async Task<IActionResult> RemoverItem(int livroId)
        {
             try
            {
                await _carrinhoRepo.RemoverItem(livroId);
                TempData["successMessage"] = "Item removido do carrinho.";
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro a remover item do carrinho");
                TempData["errorMessage"] = "Ocorreu um erro ao remover o item do carrinho.";
            }

            return RedirectToAction(nameof(MeuCarrinho));
        }

        /// <summary>
        /// Mostra o carrinho completo do utilizador autenticado (detalhes + livros + género).
        /// </summary>
        public async Task<IActionResult> MeuCarrinho()
        {
            var carrinho = await _carrinhoRepo.GetCarrinhoUtilizador();
            return View(carrinho);
        }

        /// <summary>
        /// Devolve o total de unidades no carrinho (para ícone/badge no layout).
        /// </summary>
        
        public async Task<IActionResult> TotalNoCarrinho()
        {
            var total = await _carrinhoRepo.GetTotalItensCarrinho();
            return Ok(total);
        }

        /// <summary>
        /// GET do Checkout — só apresenta o formulário de dados de entrega/pagamento.
        /// </summary>
      
            public IActionResult Checkout()
            {
                return View(new Encomenda());
            }

        /// <summary>
        /// POST do Checkout — cria Encomenda + Detalhes e limpa o carrinho.
        /// </summary>
        [HttpPost]
        [ValidateAntiForgeryToken] // protege submissão do formulário
        public async Task<IActionResult> Checkout([Bind("MetodoPagamento")] Encomenda model)
        {
            if (!ModelState.IsValid) return View(model);

            try
            {
                var ok = await _carrinhoRepo.Checkout(model);
                if (!ok) return RedirectToAction(nameof(FalhaEncomenda));

                TempData["successMessage"] = "Encomenda criada com sucesso.";
                return RedirectToAction(nameof(SucessoEncomenda));
            }
            catch (UnauthorizedAccessException ex)
            {
                ModelState.AddModelError(string.Empty, "Utilizador não iniciou sessão");
                return View(model);
            }
            catch (InvalidOperationException ex)
            {
                ModelState.AddModelError(string.Empty, "O carrinho está vazio");
                return View(model);
            }

            catch (Exception ex)
            {
                // ponto crítico: transação no repositório pode fazer rollback
                _logger.LogError(ex, "Erro no checkout");
                TempData["errorMessage"] = "Não foi possível concluir o checkout.";
                return RedirectToAction(nameof(FalhaEncomenda));
            }
        }

        /// <summary>Vista simples de sucesso de encomenda.</summary>
        public IActionResult SucessoEncomenda() => View();

        /// <summary>Vista simples de falha no processo de encomenda.</summary>
        public IActionResult FalhaEncomenda() => View();
    }
}
