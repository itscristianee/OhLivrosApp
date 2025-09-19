using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using OhLivrosApp.Models;
using OhLivrosApp.Models.DTO.Api;
using OhLivrosApp.Repositorios;
using OhLivrosApp.Constantes;
using OhLivrosApp.Servicos;

namespace OhLivrosApp.Controllers.Api
{
    [ApiController]
    [Route("api/livros")]
    public class LivrosApiController : ControllerBase
    {
        private readonly ILivroRepositorio _livros;
        private readonly IGeneroRepositorio _generos;
        private readonly IFicheiroServico _ficheiros;

        public LivrosApiController(ILivroRepositorio livros, IGeneroRepositorio generos, IFicheiroServico ficheiros)
        {
            _livros = livros;
            _generos = generos;
            _ficheiros = ficheiros;
        }

        // 1) TODOS PODEM VER A LISTA
        [HttpGet]
        [AllowAnonymous]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<ActionResult<IEnumerable<LivroListDTO>>> GetAll()
        {
            var data = await _livros.ObterTodosAsync();
            var dtos = data.Select(l => new LivroListDTO(
                l.Id,
                l.Titulo,
                l.Autor ?? string.Empty,
                l.Genero?.Nome ?? "",
                l.Preco,
                string.IsNullOrWhiteSpace(l.Imagem) ? null : Url.Content($"~/images/{l.Imagem}")
            ));
            return Ok(dtos);
        }

        // 2) DETALHE: SÓ UTILIZADOR AUTENTICADO (user normal OU admin)
        [HttpGet("{id:int}")]
        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<LivroListDTO>> GetById(int id)
        {
            var l = await _livros.ObterPorIdAsync(id);
            if (l == null) return NotFound();

            var dto = new LivroListDTO(
                l.Id,
                l.Titulo,
                l.Autor ?? string.Empty,
                l.Genero?.Nome ?? "",
                l.Preco,
                string.IsNullOrWhiteSpace(l.Imagem) ? null : Url.Content($"~/images/{l.Imagem}")
            );
            return Ok(dto);
        }

        // 3) CRIAR: SÓ ADMIN
        [HttpPost]
        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme, Roles = nameof(Perfis.Administrador))]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<ActionResult> Create([FromBody] LivroCreateDTO dto)
        {
            if (!ModelState.IsValid) return ValidationProblem(ModelState);

            var genero = await _generos.ObterPorIdAsync(dto.GeneroFK);
            if (genero == null) return BadRequest("Género inválido.");

            var livro = new Livro
            {
                Titulo = dto.Titulo,
                Autor = dto.Autor,
                GeneroFK = dto.GeneroFK,
                Preco = dto.Preco,
                Imagem = dto.Imagem
            };

            await _livros.AdicionarAsync(livro);
            return CreatedAtAction(nameof(GetById), new { id = livro.Id }, new { livro.Id });
        }

        // 4) ATUALIZAR: SÓ ADMIN
        [HttpPut("{id:int}")]
        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme, Roles = nameof(Perfis.Administrador))]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult> Update(int id, [FromBody] LivroUpdateDTO dto)
        {
            if (!ModelState.IsValid) return ValidationProblem(ModelState);

            var existente = await _livros.ObterPorIdAsync(id);
            if (existente == null) return NotFound();

            var genero = await _generos.ObterPorIdAsync(dto.GeneroFK);
            if (genero == null) return BadRequest("Género inválido.");

            existente.Titulo = dto.Titulo;
            existente.Autor = dto.Autor;
            existente.GeneroFK = dto.GeneroFK;
            existente.Preco = dto.Preco;
            if (!string.IsNullOrWhiteSpace(dto.Imagem))
                existente.Imagem = dto.Imagem;

            await _livros.AtualizarAsync(existente);
            return NoContent();
        }

        // 5) ELIMINAR: SÓ ADMIN
        [HttpDelete("{id:int}")]
        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme, Roles = nameof(Perfis.Administrador))]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult> Delete(int id)
        {
            var livro = await _livros.ObterPorIdAsync(id);
            if (livro == null) return NotFound();

            await _livros.RemoverAsync(livro);

            if (!string.IsNullOrWhiteSpace(livro.Imagem))
                _ficheiros.Apagar(livro.Imagem);

            return NoContent();
        }
    }
}
