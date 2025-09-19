using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using OhLivrosApp.Constantes;
using OhLivrosApp.Repositorios;
using OhLivrosApp.Models;
using OhLivrosApp.Servicos;
using OhLivrosApp.Models.DTO;

namespace OhLivrosApp.Controllers
{
    [Authorize(Roles = nameof(Perfis.Administrador))]
    public class LivrosController : Controller
    {
        private readonly ILivroRepositorio _livroRepo;
        private readonly IGeneroRepositorio _generoRepo;
        private readonly IFicheiroServico _ficheiroServico; 


        public LivrosController(ILivroRepositorio livroRepo, IGeneroRepositorio generoRepo, IFicheiroServico ficheiroServico)
        {
            _livroRepo = livroRepo;
            _generoRepo = generoRepo;
            _ficheiroServico = ficheiroServico;
        }

        // GET: /Livros
        public async Task<IActionResult> Index()
        {
            var livros = await _livroRepo.ObterTodosAsync();
            return View(livros);
        }

        // GET: /Livros/Criar
        public async Task<IActionResult> Criar()
        {
            var generoSelectList = (await _generoRepo.ObterTodosAsync())
                .Select(g => new SelectListItem { Text = g.Nome, Value = g.Id.ToString() });

            var dto = new LivroDTO { Generos = generoSelectList };
            return View(dto);
        }

        // POST: /Livros/Criar
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Criar(LivroDTO dto)
        {
            // Reconstituir lista para o caso de validação falhar
            dto.Generos = (await _generoRepo.ObterTodosAsync())
                .Select(g => new SelectListItem { Text = g.Nome, Value = g.Id.ToString(), Selected = g.Id == dto.GeneroFK });

            if (!ModelState.IsValid)
                return View(dto);

            try
            {
                // Upload de imagem (opcional)
                if (dto.ImagemFicheiro != null)
                {
                    if (dto.ImagemFicheiro.Length > 1 * 1024 * 1024)
                        throw new InvalidOperationException("O ficheiro de imagem não pode exceder 1 MB.");

                    string[] extensoesPermitidas = [".jpeg", ".jpg", ".png"];
                    string nomeImagem = await _ficheiroServico.GuardarAsync(dto.ImagemFicheiro, extensoesPermitidas);
                    dto.Imagem = nomeImagem;
                }

                // Mapear DTO -> Entidade
                var livro = new Livro
                {
                    Titulo = dto.Titulo,
                    Autor = dto.Autor,
                    Imagem = dto.Imagem,
                    GeneroFK = dto.GeneroFK,
                    Preco = dto.Preco
                };

                await _livroRepo.AdicionarAsync(livro);
                TempData[NotificationType.SuccessMessage] = "Livro adicionado com sucesso.";
                return RedirectToAction(nameof(Index));
            }
            catch (InvalidOperationException ex)
            {
                TempData["errorMessage"] = ex.Message;
                return View(dto);
            }
            catch (FileNotFoundException ex)
            {
                TempData["errorMessage"] = ex.Message;
                return View(dto);
            }
            catch
            {
                TempData[NotificationType.ErrorMessage] = "Erro ao guardar os dados.";
                return View(dto);
            }
        }

        // GET: /Livros/Editar/5
        public async Task<IActionResult> Editar(int id)
        {
            var livro = await _livroRepo.ObterPorIdAsync(id);
            if (livro == null)
            {
                TempData[NotificationType.ErrorMessage] = $"Livro com o id {id} não foi encontrado.";
                return RedirectToAction(nameof(Index));
            }

            var generoSelectList = (await _generoRepo.ObterTodosAsync())
                .Select(g => new SelectListItem { Text = g.Nome, Value = g.Id.ToString(), Selected = g.Id == livro.GeneroFK });

            var dto = new LivroDTO
            {
                Id = livro.Id,
                Titulo = livro.Titulo,
                Autor = livro.Autor ?? string.Empty,
                GeneroFK = livro.GeneroFK,
                Preco = livro.Preco,
                Imagem = livro.Imagem,
                Generos = generoSelectList
            };

            return View(dto);
        }

        // POST: /Livros/Editar
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Editar(LivroDTO dto)
        {
            dto.Generos = (await _generoRepo.ObterTodosAsync())
                .Select(g => new SelectListItem { Text = g.Nome, Value = g.Id.ToString(), Selected = g.Id == dto.GeneroFK });

            if (!ModelState.IsValid)
                return View(dto);

            try
            {
                string imagemAntiga = "";

                if (dto.ImagemFicheiro != null)
                {
                    if (dto.ImagemFicheiro.Length > 1 * 1024 * 1024)
                        throw new InvalidOperationException("O ficheiro de imagem não pode exceder 1 MB.");

                    string[] extensoesPermitidas = [".jpeg", ".jpg", ".png"];
                    string nomeImagem = await _ficheiroServico.GuardarAsync(dto.ImagemFicheiro, extensoesPermitidas);

                    // Guardar nome antigo para apagar depois
                    imagemAntiga = dto.Imagem ?? "";
                    dto.Imagem = nomeImagem;
                }

                var livro = new Livro
                {
                    Id = dto.Id,
                    Titulo = dto.Titulo,
                    Autor = dto.Autor ?? string.Empty,
                    GeneroFK = dto.GeneroFK,
                    Preco = dto.Preco,
                    Imagem = dto.Imagem
                };

                await _livroRepo.AtualizarAsync(livro);

                if (!string.IsNullOrWhiteSpace(imagemAntiga))
                    _ficheiroServico.Apagar(imagemAntiga);

                TempData["successMessage"] = "Livro atualizado com sucesso.";
                return RedirectToAction(nameof(Index));
            }
            catch (InvalidOperationException ex)
            {
                TempData["errorMessage"] = ex.Message;
                return View(dto);
            }
            catch (FileNotFoundException ex)
            {
                TempData["errorMessage"] = ex.Message;
                return View(dto);
            }
            catch
            {
                TempData[NotificationType.ErrorMessage] = "Erro ao guardar os dados.";
                return View(dto);
            }
        }

        // POST: /Livros/Eliminar/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Eliminar(int id)
        {
            try
            {
                var livro = await _livroRepo.ObterPorIdAsync(id);
                if (livro == null)
                {
                    TempData[NotificationType.ErrorMessage] = $"Livro com o id {id} não foi encontrado.";
                }
                else
                {
                    await _livroRepo.RemoverAsync(livro);

                    if (!string.IsNullOrWhiteSpace(livro.Imagem))
                        _ficheiroServico.Apagar(livro.Imagem);

                    TempData[NotificationType.SuccessMessage] = "Livro eliminado com sucesso.";
                }
            }
            catch (InvalidOperationException ex)
            {
                TempData["errorMessage"] = ex.Message;
            }
            catch (FileNotFoundException ex)
            {
                TempData["errorMessage"] = ex.Message;
            }
            catch
            {
                TempData[NotificationType.ErrorMessage] = "Erro ao eliminar os dados.";
            }

            return RedirectToAction(nameof(Index));
        }
    }
}
