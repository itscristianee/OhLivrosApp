using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using OhLivrosApp.Constantes;
using OhLivrosApp.Data;
using OhLivrosApp.Models;
using OhLivrosApp.Models.DTO;
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
        private readonly ILogger<CarrinhoRepositorio> _logger;

        public CarrinhoRepositorio(ApplicationDbContext context,
                                    IHttpContextAccessor http,
                                    UserManager<IdentityUser> userManager,
                                    ILogger<CarrinhoRepositorio> logger)
        {
            _context = context;
            _userManager = userManager;
            _http = http;
            _logger = logger;

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
        /// incluindo detalhes → livros → género → stock e detalhes de encomenda.
        /// </summary>
        public async Task<Carrinho?> GetCarrinhoUtilizador()
        {
            var utilizadorId = await GetUserIdAsync();
            if (utilizadorId == 0) return null;

            return await _context.Carrinhos
                .Include(c => c.DetalhesCarrinho)
                    .ThenInclude(dc => dc.Livro)
                        .ThenInclude(l => l.Genero)
                .Include(c => c.DetalhesCarrinho)
                    .ThenInclude(dc => dc.Livro)
                        .ThenInclude(l => l.Stock)
                .Include(c => c.DetalhesCarrinho)
                    .ThenInclude(dc => dc.Livro)
                        .ThenInclude(l => l.DetalhesEncomenda)
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
                encomenda.DataCriacao = DateTime.UtcNow;
                encomenda.CompradorFK = utilizadorId;
                encomenda.Pago = false;
                encomenda.Estado = Estados.Pendente;        // enum

                // 4. Criar detalhes da encomenda (linhas)
                foreach (var item in itens)
                {
                    // -- Validar e atualizar stock --
                    var stock = await _context.Stocks
                                              .FirstOrDefaultAsync(s => s.LivroFK == item.LivroFK);

                    if (stock == null)
                        throw new InvalidOperationException("Stock inexistente para o livro selecionado.");

                    if (item.Quantidade > stock.Quantidade)
                        throw new InvalidOperationException(
                            $"Só existem {stock.Quantidade} unidade(s) em stock para o livro {item.LivroFK}."
                        );

                    stock.Quantidade -= item.Quantidade;

                    // -- Criar detalhe da encomenda --
                    await _context.DetalhesEncomendas.AddAsync(new DetalheEncomenda
                    {
                        LivroFK = item.LivroFK,
                        Encomenda = encomenda,
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

        public async Task<CheckoutModelDTO> PrepararCheckoutAsync()
        {
            var utilizadorId = await GetUserIdAsync();
            if (utilizadorId == 0) throw new UnauthorizedAccessException("Utilizador não autenticado");

            var u = await _context.Utilizadores.AsNoTracking()
                     .FirstOrDefaultAsync(x => x.Id == utilizadorId)
                     ?? throw new InvalidOperationException("Utilizador não encontrado");

            var carrinho = await GetCarrinho(utilizadorId)
                         ?? throw new InvalidOperationException("Carrinho inválido");

            var itens = await _context.DetalhesCarrinhos
                .Where(d => d.CarrinhoFK == carrinho.Id)
                .Include(d => d.Livro).ThenInclude(l => l.Genero)
                .AsNoTracking()
                .Select(d => new ResumoCarrinhoItem
                {
                    Titulo = d.Livro.Titulo,
                    Quantidade = d.Quantidade,
                    PrecoUnitario = d.PrecoUnitario
                })
                .ToListAsync();

            if (itens.Count == 0) throw new InvalidOperationException("O carrinho está vazio");

            return new CheckoutModelDTO { Utilizador = u, Itens = itens };
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
            {
                _logger.LogWarning("GetUserIdAsync: utilizador não autenticado.");
                return 0;
            }

            // GUID do Identity (Claim NameIdentifier)
            var identityId = (principal.FindFirstValue(ClaimTypes.NameIdentifier) ?? string.Empty).Trim();
            if (string.IsNullOrWhiteSpace(identityId))
            {
                _logger.LogWarning("GetUserIdAsync: Claim NameIdentifier em falta.");
                return 0;
            }

            // (Opcional) Log de ligação à BD para confirmar que está a apontar para o DB certo
            try
            {
                var cnn = _context.Database.GetDbConnection();
                _logger.LogDebug("GetUserIdAsync: BD '{db}' @ '{src}'", cnn.Database, cnn.DataSource);
            }
            catch { /* ignore */ }

            // Procura na tabela Utilizadores onde UserName guarda o GUID do Identity
            var query = _context.Utilizadores
                                .AsNoTracking()
                                .Where(u => (u.UserName ?? string.Empty).Trim() == identityId);

            _logger.LogDebug("GetUserIdAsync SQL: {sql}", query.ToQueryString());

            var utilizador = await query.FirstOrDefaultAsync();

            if (utilizador == null)
            {
                _logger.LogWarning("GetUserIdAsync: não encontrei Utilizador para IdentityId {guid}.", identityId);

                //  criar automaticamente o registo em Utilizadores
                // quando ainda não existir, e so descomentar este bloco:
                /*
                var nome = principal.Identity?.Name;
                var novo = new Utilizador
                {
                    Nome = string.IsNullOrWhiteSpace(nome) ? "Sem nome" : nome!,
                    NIF = "000000000",
                    UserName = identityId
                };
                _context.Utilizadores.Add(novo);
                await _context.SaveChangesAsync();
                _logger.LogInformation("GetUserIdAsync: criei Utilizador Id={id} para IdentityId {guid}.", novo.Id, identityId);
                return novo.Id;
                */

                return 0;
            }

            _logger.LogInformation("GetUserIdAsync: Utilizador '{nome}' (Id={id}) associado ao IdentityId {guid}.",
                utilizador.Nome, utilizador.Id, identityId);

            return utilizador.Id;
        }

    }
}
