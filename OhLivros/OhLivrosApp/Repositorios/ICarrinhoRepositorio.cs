using OhLivrosApp.Models;
using OhLivrosApp.Models.DTO;

namespace OhLivrosApp
{
    public interface ICarrinhoRepositorio
    {
        /// <summary>
        /// Adiciona um livro ao carrinho do utilizador
        /// </summary>
        Task<int> AdicionarItem(int livroId, int qtd);

        /// <summary>
        /// Remove um livro do carrinho do utilizador
        /// </summary>
        Task<int> RemoverItem(int livroId);

        /// <summary>
        /// Obtém o carrinho completo do utilizador autenticado
        /// </summary>
        Task<Carrinho?> GetCarrinhoUtilizador();

        /// <summary>
        /// Obtém um carrinho pelo ID do utilizador
        /// </summary>
        Task<Carrinho?> GetCarrinho(int utilizadorId);

        /// <summary>
        /// Conta os itens do carrinho
        /// </summary>
        Task<int> GetTotalItensCarrinho(int utilizadorId = 0);

        /// <summary>
        /// Realiza o checkout (gera encomenda + esvazia carrinho)
        /// </summary>
        Task<bool> Checkout(Encomenda encomenda);

        Task<CheckoutModelDTO> PrepararCheckoutAsync();

    }
}
