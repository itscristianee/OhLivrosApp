using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using OhLivrosApp.Data;
using OhLivrosApp.Models;

namespace OhLivrosApp.Repositorios
{
    public class CarrinhoRepositorio : ICarrinhoRepositorio
    {
        private readonly ApplicationDbContext _db;
        private readonly UserManager<IdentityUser> _userManager;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public CarrinhoRepositorio(ApplicationDbContext db, IHttpContextAccessor httpContextAccessor,
            UserManager<IdentityUser> userManager)
        {
            _db = db;
            _userManager = userManager;
            _httpContextAccessor = httpContextAccessor;
        }

        /// <summary>
        /// Adiciona um livro ao carrinho do utilizador
        /// </summary>
        public async Task<int> AdicionarItem(int livroId, int qtd)
        {
            int utilizadorId = GetUserId();
            using var transaction = await _db.Database.BeginTransactionAsync();

            if (utilizadorId == 0)
                throw new UnauthorizedAccessException("Utilizador não está autenticado");

            var carrinho = await GetCarrinho(utilizadorId);

            if (carrinho is null)
            {
                carrinho = new Carrinho { DonoFK = utilizadorId };
                _db.Carrinhos.Add(carrinho);
            }

            await _db.SaveChangesAsync();

            // detalhe do carrinho
            var item = _db.DetalhesCarrinhos
                          .FirstOrDefault(a => a.CarrinhoFK == carrinho.Id && a.LivroFK == livroId);

            if (item is not null)
            {
                item.Quantidade += qtd;
            }
            else
            {
                var livro = _db.Livros.Find(livroId)
                            ?? throw new ArgumentException("Livro não encontrado");

                item = new DetalheCarrinho
                {
                    LivroFK = livroId,
                    CarrinhoFK = carrinho.Id,
                    Quantidade = qtd,
                    PrecoUnitario = livro.Preco
                };
                _db.DetalhesCarrinhos.Add(item);
            }

            await _db.SaveChangesAsync();
            await transaction.CommitAsync();

            return await GetTotalItensCarrinho(utilizadorId);
        }

        /// <summary>
        /// Remove um livro do carrinho
        /// </summary>
        public async Task<int> RemoverItem(int livroId)
        {
            int utilizadorId = GetUserId();

            if (utilizadorId == 0)
                throw new UnauthorizedAccessException("Utilizador não está autenticado");

            var carrinho = await GetCarrinho(utilizadorId);
            if (carrinho is null)
                throw new InvalidOperationException("Carrinho inválido");

            var item = _db.DetalhesCarrinhos
                          .FirstOrDefault(a => a.CarrinhoFK == carrinho.Id && a.LivroFK == livroId);

            if (item is null)
                throw new InvalidOperationException("Item não existe no carrinho");

            if (item.Quantidade == 1)
                _db.DetalhesCarrinhos.Remove(item);
            else
                item.Quantidade--;

            await _db.SaveChangesAsync();

            return await GetTotalItensCarrinho(utilizadorId);
        }

        /// <summary>
        /// Obtém o carrinho completo do utilizador logado
        /// </summary>
        public async Task<Carrinho?> GetCarrinhoUtilizador()
        {
            int utilizadorId = GetUserId();
            if (utilizadorId == 0)
                throw new InvalidOperationException("Utilizador inválido");

            return await _db.Carrinhos
                            .Include(c => c.DetalhesCarrinho)
                            .ThenInclude(dc => dc.Livro)
                            .ThenInclude(l => l.Genero)
                            .Where(c => c.DonoFK == utilizadorId)
                            .AsNoTracking()
                            .FirstOrDefaultAsync();
        }

        /// <summary>
        /// Obtém o carrinho pelo dono
        /// </summary>
        public async Task<Carrinho?> GetCarrinho(int utilizadorId)
        {
            return await _db.Carrinhos
                            .FirstOrDefaultAsync(c => c.DonoFK == utilizadorId);
        }

        /// <summary>
        /// Conta os itens no carrinho do utilizador
        /// </summary>
        public async Task<int> GetTotalItensCarrinho(int utilizadorId = 0)
        {
            if (utilizadorId == 0)
                utilizadorId = GetUserId();

            return await _db.Carrinhos
                            .Include(c => c.DetalhesCarrinho)
                            .Where(c => c.DonoFK == utilizadorId)
                            .SelectMany(c => c.DetalhesCarrinho)
                            .CountAsync();
        }

        /// <summary>
        /// Faz o checkout do carrinho
        /// </summary>
        public async Task<bool> Checkout(Encomenda encomenda)
        {
            using var transaction = await _db.Database.BeginTransactionAsync();

            int utilizadorId = GetUserId();
            if (utilizadorId == 0)
                throw new UnauthorizedAccessException("Utilizador não autenticado");

            var carrinho = await GetCarrinho(utilizadorId);
            if (carrinho is null)
                throw new InvalidOperationException("Carrinho inválido");

            var itens = _db.DetalhesCarrinhos
                           .Where(a => a.CarrinhoFK == carrinho.Id)
                           .ToList();

            if (itens.Count == 0)
                throw new InvalidOperationException("Carrinho vazio");

            // guarda encomenda
            encomenda.DataCriacao = DateTime.Now;
            encomenda.CompradorFK = utilizadorId;
            _db.Encomendas.Add(encomenda);
            await _db.SaveChangesAsync();

            foreach (var item in itens)
            {
                var detalhe = new DetalheEncomenda
                {
                    LivroFK = item.LivroFK,
                    EncomendaFK = encomenda.Id,
                    Quantidade = item.Quantidade,
                    PrecoUnitario = item.PrecoUnitario
                };
                _db.DetalhesEncomendas.Add(detalhe);
            }

            // esvaziar carrinho
            _db.DetalhesCarrinhos.RemoveRange(itens);
            await _db.SaveChangesAsync();

            await transaction.CommitAsync();
            return true;
        }

        private int GetUserId()
        {
            var principal = _httpContextAccessor.HttpContext?.User;
            var userId = _userManager.GetUserId(principal);

            // ⚠️ O teu Utilizador.Id é int, por isso precisas mapear IdentityUser → Utilizador
            var utilizador = _db.Utilizadores.FirstOrDefault(u => u.UserName == userId);
            return utilizador?.Id ?? 0;
        }
    }
}
