using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using OhLivrosApp.Constantes;
using OhLivrosApp.Models;
using OhLivrosApp.Models.DTO;
using OhLivrosApp.Repositorios;

namespace OhLivrosApp.Controllers
{
    [Authorize(Roles = nameof(Perfis.Administrador))]
    public class GenerosController : Controller
    {
        private readonly IGeneroRepositorio _generoRepo;

        public GenerosController(IGeneroRepositorio generoRepo)
        {
            _generoRepo = generoRepo;
        }

        // GET: /Generos
        public async Task<IActionResult> Index()
        {
            var generos = await _generoRepo.ObterTodosAsync();
            return View(generos);
        }

        // GET: /Generos/Criar
        public IActionResult Criar()
        {
            return View();
        }

        // POST: /Generos/Criar
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Criar(GeneroDTO dto)
        {
            if (!ModelState.IsValid) return View(dto);

            try
            {
                var genero = new Genero { Id = dto.Id, Nome = dto.Nome };
                await _generoRepo.AdicionarAsync(genero);
                TempData["successMessage"] = "Género adicionado com sucesso.";
                return RedirectToAction(nameof(Index));
            }
            catch
            {
                TempData["errorMessage"] = "Não foi possível adicionar o género.";
                return View(dto);
            }
        }

        // GET: /Generos/Editar/5
        public async Task<IActionResult> Editar(int id)
        {
            var genero = await _generoRepo.ObterPorIdAsync(id);
            if (genero is null) return NotFound();

            var dto = new GeneroDTO { Id = genero.Id, Nome = genero.Nome };
            return View(dto);
        }

        // POST: /Generos/Editar
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Editar(GeneroDTO dto)
        {
            if (!ModelState.IsValid) return View(dto);

            try
            {
                var genero = new Genero { Id = dto.Id, Nome = dto.Nome };
                await _generoRepo.AtualizarAsync(genero);
                TempData["successMessage"] = "Género atualizado com sucesso.";
                return RedirectToAction(nameof(Index));
            }
            catch
            {
                TempData["errorMessage"] = "Não foi possível atualizar o género.";
                return View(dto);
            }
        }

        // POST: /Generos/Eliminar/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Eliminar(int id)
        {
            var genero = await _generoRepo.ObterPorIdAsync(id);
            if (genero is null) return NotFound();

            try
            {
                await _generoRepo.RemoverAsync(genero);
                TempData["successMessage"] = "Género eliminado com sucesso.";
            }
            catch
            {
                TempData["errorMessage"] = "Não foi possível eliminar o género.";
            }

            return RedirectToAction(nameof(Index));
        }
    }
}
