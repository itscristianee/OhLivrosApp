using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using OhLivrosApp.Constantes;
using OhLivrosApp.Data;
using OhLivrosApp.Models;
using System.Security.Claims;

namespace OhLivrosApp.Repositorios
{
    /// <summary>
    /// Repositório para leitura/gestão de <see cref="Encomenda"/>.
    /// Usa Identity para identificar o utilizador atual e aplica includes
    /// para carregar detalhes (linhas e livros) quando necessário.
    /// </summary>
    public class EncUtilizadorRepositorio : IEncUtilizadorRepositorio
    {
        private readonly ApplicationDbContext _context;
        private readonly IHttpContextAccessor _http;
        private readonly UserManager<IdentityUser> _userManager;
        private readonly ILogger<EncUtilizadorRepositorio> _logger;

        public EncUtilizadorRepositorio(
            ApplicationDbContext context,
            IHttpContextAccessor http,
            UserManager<IdentityUser> userManager,
            ILogger<EncUtilizadorRepositorio> logger)
        {
            _context = context;
            _http = http;
            _userManager = userManager;
            _logger = logger;
        }

        /// <summary>
        /// Devolve as encomendas do utilizador autenticado (por omissão).
        /// Quando <paramref name="obterTodas"/> for <c>true</c>, devolve todas as encomendas.
        /// </summary>
        /// <param name="obterTodas">Se <c>true</c>, ignora o filtro por utilizador.</param>
        /// <returns>Lista de encomendas ordenadas por data (desc).</returns>
        /// <exception cref="UnauthorizedAccessException">Se o utilizador não estiver autenticado.</exception>
        public async Task<IEnumerable<Encomenda>> EncomendasDoUtilizadorAsync(bool obterTodas = false)
        {
            // query base com includes usuais + filtro de eliminadas + leitura sem tracking
            IQueryable<Encomenda> q = _context.Encomendas
                .AsNoTracking()
                .Where(e => !e.Eliminado)                                 // filtra eliminadas
                .Include(e => e.DetalhesEncomenda)
                    .ThenInclude(d => d.Livro)
                        .ThenInclude(l => l.Genero)
                .Include(e => e.Comprador);

            if (!obterTodas)
            {
                var utilizadorId = await ObterUtilizadorIdAsync();
                if (utilizadorId == 0)
                    throw new UnauthorizedAccessException("Utilizador não autenticado");

                q = q.Where(e => e.CompradorFK == utilizadorId);
            }

            return await q.OrderByDescending(e => e.DataCriacao).ToListAsync();
        }

        /// <summary>
        /// Obtém uma encomenda por Id, incluindo detalhes, livros e comprador.
        /// </summary>
        /// <param name="id">Identificador da encomenda.</param>
        /// <param name="incluirDetalhes">Se true, inclui linhas e comprador.</param>
        /// <returns>Encomenda completa ou <c>null</c> se não existir.</returns>
        public async Task<Encomenda?> ObterPorIdAsync(int id, bool incluirDetalhes = true)
        {
            // leitura sem tracking + filtro de eliminadas
            IQueryable<Encomenda> q = _context.Encomendas
                .AsNoTracking()
                .Where(e => !e.Eliminado);                                // filtra eliminadas

            if (incluirDetalhes)
            {
                q = q
                    .Include(e => e.DetalhesEncomenda)
                        .ThenInclude(d => d.Livro)
                            .ThenInclude(l => l.Genero)
                    .Include(e => e.Comprador);
            }

            return await q.FirstOrDefaultAsync(e => e.Id == id);
        }

        /// <summary>
        /// Alterna o estado de pagamento (Pago/Não Pago) de uma encomenda.
        /// </summary>
        /// <param name="encomendaId">Id da encomenda.</param>
        /// <exception cref="InvalidOperationException">Se a encomenda não existir.</exception>
        public async Task AlternarPagamentoAsync(int encomendaId)
        {
            // evita mexer em encomendas eliminadas
            var enc = await _context.Encomendas
                .FirstOrDefaultAsync(e => e.Id == encomendaId && !e.Eliminado); // filtra eliminadas

            if (enc is null)
                throw new InvalidOperationException($"Encomenda #{encomendaId} não encontrada.");

            enc.Pago = !enc.Pago;
            await _context.SaveChangesAsync();
        }

        /// <summary>
        /// Atualiza o estado da encomenda (Pendente, Enviado, Entregue, etc.).
        /// </summary>
        /// <param name="encomendaId">Id da encomenda.</param>
        /// <param name="novoEstado">Novo estado.</param>
        /// <exception cref="InvalidOperationException">Se a encomenda não existir.</exception>
        public async Task AtualizarEstadoAsync(int encomendaId, Estados novoEstado)
        {
            // evita mexer em encomendas eliminadas
            var enc = await _context.Encomendas
                .FirstOrDefaultAsync(e => e.Id == encomendaId && !e.Eliminado); // filtra eliminadas

            if (enc is null)
                throw new InvalidOperationException($"Encomenda #{encomendaId} não encontrada.");

            enc.Estado = novoEstado;
            await _context.SaveChangesAsync();
        }

        // ---------- helpers ----------

        /// <summary>
        /// Faz a ponte entre Identity (GUID string) e Utilizador (int).
        /// Procura em Utilizadores.UserName o Id do IdentityUser.
        /// </summary>
        private async Task<int> ObterUtilizadorIdAsync()
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

            // (Opcional) Log da conexão
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

                //  criar automaticamente o registo em Utilizadores (opcional)
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
