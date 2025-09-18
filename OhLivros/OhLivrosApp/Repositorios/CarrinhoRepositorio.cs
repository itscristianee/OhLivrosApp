using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using OhLivrosApp.Data;
using OhLivrosApp.Models;
using System.Security.Claims;

namespace OhLivrosApp.Repositorios
{
    /// <summary>
    /// Repositório para gerir o carrinho de compras:
    /// - Adicionar e remover itens
    /// - Obter carrinho completo do utilizador
    /// - Contar itens
    /// - Checkout (criação de encomenda)
    /// </summary>
    public class CarrinhoRepositorio : ICarrinhoRepositorio
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<IdentityUser> _userManager;
        private readonly IHttpContextAccessor _http;

        public CarrinhoRepositorio(ApplicationDbContext context, IHttpContextAccessor http, UserManager<IdentityUser> userManager)
        {
            _context = context;
            _userManager = userManager;
            _http = http;
        }

        /// <summary>
        /// Adiciona um livro ao carrinho do utilizador autenticado.
        /// - Se não existir carrinho → cria
        /// - Se livro já existir → incrementa quantidade
        /// - Se for novo → cria detalhe
        /// - Tudo dentro de transação para garantir rollback
        /// </summary>
        public async Task<int> AdicionarItem(int livroId, int qtd)
        {
            if (qtd <= 0) throw new ArgumentOutOfRangeException(nameof(qtd), "Quantidade deve ser > 0.");

            var utilizadorId = await GetUserIdAsync();
            if (utilizadorId == 0) throw new UnauthorizedAccessException("Utilizador não autenticado");

            await using var tx = await _context.Database.BeginTransactionAsync();
            try
            {
                 // garante que há carrinho para este utilizador
                 var carrinho = await GetCarrinho(utilizadorId);
                if (carrinho is null)
                {
                    carrinho = (await _context.Carrinhos.AddAsync(new Carrinho { DonoFK = utilizadorId })).Entity;
                    await _context.SaveChangesAsync(); // garantir carrinho.Id
                }

                // verifica se o livro já está no carrinho
                var item = await _context.DetalhesCarrinhos
                    .FirstOrDefaultAsync(a => a.CarrinhoFK == carrinho.Id && a.LivroFK == livroId);

                if (item is not null)
                {
                    item.Quantidade += qtd;      // já existe → incrementa 
                }
                else
                {
                     // cria detalhe novo
                     var livro = await _context.Livros.FindAsync(livroId)
                        ?? throw new ArgumentException("Livro não encontrado", nameof(livroId));

                    await _context.DetalhesCarrinhos.AddAsync(new DetalheCarrinho
                    {
                        LivroFK = livroId,
                        CarrinhoFK = carrinho.Id,
                        Quantidade = qtd,
                        PrecoUnitario = (decimal)livro.Preco
                    });
                }

                await _context.SaveChangesAsync();
                await tx.CommitAsync();
                return await GetTotalItensCarrinho(utilizadorId);
            }
            catch
            {
                await tx.RollbackAsync();
                throw;
            }
        }



        /// <summary>
        /// Remove 1 unidade de um livro do carrinho.
        /// - Se quantidade > 1 → decrementa
        /// - Se quantidade = 1 → remove item
        /// </summary>
        public async Task<int> RemoverItem(int livroId)
        {
            var utilizadorId = await GetUserIdAsync();
            if (utilizadorId == 0) throw new UnauthorizedAccessException("Utilizador não autenticado");

            var carrinho = await GetCarrinho(utilizadorId) ?? throw new InvalidOperationException("Carrinho inválido");

            var item = await _context.DetalhesCarrinhos
                                .FirstOrDefaultAsync(a => a.CarrinhoFK == carrinho.Id && a.LivroFK == livroId)
                      ?? throw new InvalidOperationException("Item não existe no carrinho");

            if (item.Quantidade > 1) item.Quantidade--;
            else _context.DetalhesCarrinhos.Remove(item);

            await _context.SaveChangesAsync();
            return await GetTotalItensCarrinho(utilizadorId);
        }

        /// <summary>
        /// Devolve o carrinho completo do utilizador autenticado,
        /// incluindo detalhes → livros → género.
        /// </summary>
        public async Task<Carrinho?> GetCarrinhoUtilizador()
        {
            var utilizadorId = await GetUserIdAsync();
            if (utilizadorId == 0) return null;

            return await _context.Carrinhos
                            .Include(c => c.DetalhesCarrinho)
                                .ThenInclude(dc => dc.Livro)
                                    .ThenInclude(l => l.Genero)
                            .AsNoTracking()
                            .FirstOrDefaultAsync(c => c.DonoFK == utilizadorId);
        }

        /// <summary>
        /// Obtém carrinho pelo DonoFK.
        /// </summary>
        public Task<Carrinho?> GetCarrinho(int utilizadorId)
            => _context.Carrinhos.FirstOrDefaultAsync(c => c.DonoFK == utilizadorId);

        /// <summary>
        /// Conta o total de unidades no carrinho (somatório de Quantidade).
        /// </summary>
        public async Task<int> GetTotalItensCarrinho(int utilizadorId = 0)
        {
            if (utilizadorId == 0) utilizadorId = await GetUserIdAsync();

            var carrinhoId = await _context.Carrinhos
                                      .Where(c => c.DonoFK == utilizadorId)
                                      .Select(c => (int?)c.Id)
                                      .FirstOrDefaultAsync();

            if (carrinhoId is null) return 0;

            return await _context.DetalhesCarrinhos
                            .Where(dc => dc.CarrinhoFK == carrinhoId.Value)
                            .SumAsync(dc => (int?)dc.Quantidade) ?? 0;
        }

        /// <summary>
        /// Realiza o checkout do carrinho do utilizador autenticado.
        /// </summary>
        public async Task<bool> Checkout(Encomenda encomenda)
        {
            //  Passos:
            //      1.Valida se utilizador está autenticado
            //      2.Obtém o carrinho e respetivos itens
            //      3.Cria uma encomenda(cabecalho)
            //      4.Cria os detalhes da encomenda(linhas)
            //      5.Limpa o carrinho
            //      6.Tudo dentro de uma transação para garantir consistência



            // Inicia transação explícita → se falhar, rollback automático
            await using var tx = await _context.Database.BeginTransactionAsync();

            try
            {
                // 1. Obter ID do utilizador autenticado
                var utilizadorId = await GetUserIdAsync();
                if (utilizadorId == 0) throw new UnauthorizedAccessException("Utilizador não autenticado");

                // 2. Obter carrinho do utilizador
                var carrinho = await GetCarrinho(utilizadorId) ?? throw new InvalidOperationException("Carrinho inválido");

                // Carregar itens do carrinho
                var itens = await _context.DetalhesCarrinhos
                                          .Where(a => a.CarrinhoFK == carrinho.Id)
                                          .AsTracking()
                                          .ToListAsync();
                if (itens.Count == 0) throw new InvalidOperationException("Carrinho vazio");

                // 3. Criar encomenda principal (cabecalho)
                encomenda.DataCriacao = DateTime.Now;
                encomenda.CompradorFK = utilizadorId;
                await _context.Encomendas.AddAsync(encomenda);
                await _context.SaveChangesAsync();

                // 4. Criar detalhes da encomenda (linhas)
                foreach (var item in itens)
                {
                    await _context.DetalhesEncomendas.AddAsync(new DetalheEncomenda
                    {
                        LivroFK = item.LivroFK,
                        EncomendaFK = encomenda.Id,
                        Quantidade = item.Quantidade,
                        PrecoUnitario = item.PrecoUnitario
                    });
                }

                // 5. Limpar carrinho (remover itens)
                _context.DetalhesCarrinhos.RemoveRange(itens);
                await _context.SaveChangesAsync();

                await tx.CommitAsync();
                return true;
            }
            catch
            {
                await tx.RollbackAsync();
                throw;
            }
        }

        /// <summary>
        /// Faz a ponte entre Identity (GUID string) e Utilizador (int).
        /// Procura em Utilizadores.UserName o Id do IdentityUser.
        /// </summary>
        
        private async Task<int> GetUserIdAsync()
        {
            var ctx = _http.HttpContext;
            var principal = ctx?.User;

            if (principal?.Identity?.IsAuthenticated != true)
                return 0;

            // GUID do Identity (NameIdentifier)
            var identityId = principal.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(identityId)) return 0;

            // match com a tua tabela (UserName guarda o GUID)
            var utilizador = await _context.Utilizadores
                .AsNoTracking()
                .FirstOrDefaultAsync(u => u.UserName == identityId);

            return utilizador?.Id ?? 0;
        }
    }
}
