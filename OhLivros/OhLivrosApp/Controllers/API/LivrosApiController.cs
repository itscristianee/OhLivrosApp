using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using OhLivrosApp.Constantes;
using OhLivrosApp.DTO;
using OhLivrosApp.Repositorios;

namespace OhLivrosApp.Controllers.Api;

[ApiController]
[Route("api/livros")]
public class LivrosApiController : ControllerBase
{
    private readonly ILivroRepositorio _livros;
    private readonly IGeneroRepositorio _generos;

    public LivrosApiController(ILivroRepositorio livros, IGeneroRepositorio generos)
    {
        _livros = livros;
        _generos = generos;
    }

    // GET: api/livros
    [HttpGet]
    [AllowAnonymous]
    public async Task<ActionResult<IEnumerable<LivroListItemDTO>>> GetAll()
    {
        var data = await _livros.ObterTodosAsync();

        var result = data.Select(l => new LivroListItemDTO(
            l.Id,
            l.Titulo,
            l.Autor ?? "",
            l.Genero?.Nome ?? "",
            l.Preco,
            l.Imagem
        ));

        return Ok(result);
    }

    // GET: api/livros/5
    [HttpGet("{id:int}")]
    [AllowAnonymous]
    public async Task<ActionResult<LivroDetailsDTO>> GetById(int id)
    {
        var l = await _livros.ObterPorIdAsync(id);
        if (l is null) return NotFound();

        var dto = new LivroDetailsDTO(
            l.Id,
            l.Titulo,
            l.Autor ?? "",
            l.GeneroFK,
            l.Genero?.Nome ?? "",
            l.Preco,
            l.Imagem
        );

        return Ok(dto);
    }

    // POST: api/livros
    [HttpPost]
    [Authorize(Roles = nameof(Perfis.Administrador))]
    public async Task<ActionResult<LivroDetailsDTO>> Create([FromBody] LivroCreateDTO dto)
    {
        // validação de género
        var g = await _generos.ObterPorIdAsync(dto.GeneroFK);
        if (g is null) return ValidationProblem("O género indicado não existe.");

        var entity = new OhLivrosApp.Models.Livro
        {
            Titulo = dto.Titulo,
            Autor = dto.Autor,
            GeneroFK = dto.GeneroFK,
            Preco = dto.Preco
        };

        await _livros.AdicionarAsync(entity);

        var result = new LivroDetailsDTO(entity.Id, entity.Titulo, entity.Autor ?? "", entity.GeneroFK, g.Nome, entity.Preco, entity.Imagem);

        return CreatedAtAction(nameof(GetById), new { id = entity.Id }, result);
    }

    // PUT: api/livros/5
    [HttpPut("{id:int}")]
    [Authorize(Roles = nameof(Perfis.Administrador))]
    public async Task<IActionResult> Update(int id, [FromBody] LivroUpdateDTO dto)
    {
        if (id != dto.Id) return BadRequest("Id do URL e do corpo não coincidem.");

        var entity = await _livros.ObterPorIdAsync(id);
        if (entity is null) return NotFound();

        var g = await _generos.ObterPorIdAsync(dto.GeneroFK);
        if (g is null) return ValidationProblem("O género indicado não existe.");

        entity.Titulo = dto.Titulo;
        entity.Autor = dto.Autor;
        entity.GeneroFK = dto.GeneroFK;
        entity.Preco = dto.Preco;

        await _livros.AtualizarAsync(entity);
        return NoContent();
    }

    // DELETE: api/livros/5
    [HttpDelete("{id:int}")]
    [Authorize(Roles = nameof(Perfis.Administrador))]
    public async Task<IActionResult> Delete(int id)
    {
        var entity = await _livros.ObterPorIdAsync(id);
        if (entity is null) return NotFound();

        await _livros.RemoverAsync(entity);
        return NoContent();
    }
}
