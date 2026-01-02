using BookFlaz.Application.Exceptions;
using BookFlaz.Application.Interfaces;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;


namespace BookFlaz.Application.Services
{
    /// <summary>
    /// Serviço responsável por processar o upload de imagens.
    /// Garante a validação do formato, tamanho e armazenamento seguro dos ficheiros.
    /// </summary>
    public class UploadService : IUploadService
    {
        private readonly string uploadPath;

        /// <summary>
        /// Inicializa uma nova instância do serviço de upload,
        /// garantindo que a pasta <c>Uploads</c> existe no diretório raiz do projeto.
        /// </summary>
        public UploadService(IWebHostEnvironment env)
        {
            uploadPath = Path.Combine(env.WebRootPath ?? env.ContentRootPath, "uploads");

            if (!Directory.Exists(uploadPath))
            {
                Directory.CreateDirectory(uploadPath);
            }
        }

        /// <summary>
        /// Realiza o upload de um ficheiro de imagem, validando a sua extensão e tamanho.
        /// </summary>
        /// <param name="file">O ficheiro enviado pelo utilizador.</param>
        /// <returns>O nome gerado do ficheiro armazenado.</returns>
        /// <exception cref="InvalidOperationException">Se a extensão for inválida ou o ficheiro exceder o tamanho permitido.</exception>
        /// <exception cref="Exception">Se ocorrer um erro inesperado durante o upload.</exception>
        public async Task<string> UploadAsync(IFormFile file)
        {
            try
            {
                var validExtensions = new List<string> { ".jpg", ".jpeg", ".png" };
                var extension = Path.GetExtension(file.FileName).ToLowerInvariant();

                if (!validExtensions.Contains(extension))
                {
                    throw new InvalidOperationException($"Extensão inválida ({extension}). Permitidas: {string.Join(", ", validExtensions)}");
                }

                const long maxSize = 5 * 1024 * 1024;

                if (file.Length > maxSize)
                {
                    throw new InvalidOperationException("Tamanho máximo permitido é 5MB");
                }

                var fileName = Guid.NewGuid().ToString() + extension;
                var filePath = Path.Combine(uploadPath, fileName);

                await using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    await file.CopyToAsync(stream);
                }

                return fileName;
            }
            catch (InvalidOperationException ex)
            {
                throw new InvalidOperationException($"Erro na validação do upload: {ex.Message}");
            }
            catch (Exception ex)
            {
                throw new Exception($"Erro no processamento do upload da imagem: {ex.Message}");
            }
        }


        /// <summary>
        /// Remove uma imagem previamente armazenada no diretório <c>Uploads</c>.
        /// É utilizada, quando um anúncio é eliminado ou uma imagem é substituída.
        /// </summary>
        /// <param name="fileName">O nome do ficheiro a ser eliminado (incluindo extensão).</param>
        /// <remarks>
        /// Caso o ficheiro não exista, o método apenas ignora a operação sem lançar exceção.
        /// </remarks>
        public async Task DeleteImgAsync(string fileName)
        {
            try
            {
                var filePath = Path.Combine(uploadPath, fileName);

                if (!File.Exists(filePath))
                {
                    return;
                }

                await Task.Run(() => File.Delete(filePath));
            }
            catch (Exception ex)
            {
                throw new Exception($"Erro ao eliminar imagem: {ex.Message}");
            }
        }
    }
}
