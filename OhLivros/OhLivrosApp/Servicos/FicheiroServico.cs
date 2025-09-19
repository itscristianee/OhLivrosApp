using OhLivrosApp.Servicos;

namespace OhLivrosApp.Servicos
{
    public interface IFicheiroServico
    {
        /// <summary>
        /// Apaga um ficheiro existente na pasta de imagens.
        /// </summary>
        /// <param name="nomeFicheiro">Nome do ficheiro a apagar.</param>
        void Apagar(string nomeFicheiro);

        /// <summary>
        /// Guarda um ficheiro enviado pelo utilizador (upload) no servidor.
        /// </summary>
        /// <param name="ficheiro">O ficheiro a guardar (upload).</param>
        /// <param name="extensoesPermitidas">Lista de extensões aceites (.jpg, .png, etc.).</param>
        /// <returns>Nome do ficheiro guardado no disco.</returns>
        Task<string> GuardarAsync(IFormFile ficheiro, string[] extensoesPermitidas);
    }
}


/// <summary>
/// Implementação do serviço de ficheiros.
/// Responsável por validar, guardar e apagar imagens no diretório wwwroot/imagens.
/// </summary>
public class FicheiroServico : IFicheiroServico
{
        private readonly IWebHostEnvironment _ambiente;

        public FicheiroServico(IWebHostEnvironment ambiente)
        {
            _ambiente = ambiente;
        }



    /// <summary>
    /// Guarda o ficheiro recebido (upload) dentro da pasta "imagens".
    /// Gera um nome único (GUID) para evitar conflitos.
    /// </summary>
    /// <param name="ficheiro">Ficheiro enviado pelo utilizador.</param>
    /// <param name="extensoesPermitidas">Extensões aceites (ex: .jpeg, .jpg, .png).</param>
    /// <returns>Nome do ficheiro guardado.</returns>
        public async Task<string> GuardarAsync(IFormFile ficheiro, string[] extensoesPermitidas)
        {
            // Caminho físico para wwwroot
            var wwwPath = _ambiente.WebRootPath;
            var pasta = Path.Combine(wwwPath, "imagens"); 

            if (!Directory.Exists(pasta))
            {
                Directory.CreateDirectory(pasta);
            }

            var extensao = Path.GetExtension(ficheiro.FileName);
            if (!extensoesPermitidas.Contains(extensao))
            {
                throw new InvalidOperationException(
                    $"Só são permitidos ficheiros com extensões: {string.Join(", ", extensoesPermitidas)}"
                );
            }

            string nomeFicheiro = $"{Guid.NewGuid()}{extensao}";
            string caminhoCompleto = Path.Combine(pasta, nomeFicheiro);

             // Grava o ficheiro fisicamente
            using var stream = new FileStream(caminhoCompleto, FileMode.Create);
            await ficheiro.CopyToAsync(stream);

            return nomeFicheiro;
        }

        /// <summary>
        /// Apaga um ficheiro existente na pasta de imagens.
        /// </summary>
        /// <param name="nomeFicheiro">Nome do ficheiro a apagar.</param>

        public void Apagar(string nomeFicheiro)
        {
            var wwwPath = _ambiente.WebRootPath;
            var caminhoCompleto = Path.Combine(wwwPath, "imagens", nomeFicheiro);

            if (!File.Exists(caminhoCompleto))
                throw new FileNotFoundException(nomeFicheiro);

            File.Delete(caminhoCompleto);
        }
    
}
