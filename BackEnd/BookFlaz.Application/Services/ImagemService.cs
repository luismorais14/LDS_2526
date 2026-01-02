using BookFlaz.Application.Interfaces;
using Google.Cloud.Vision.V1;
using Microsoft.AspNetCore.Http;

namespace BookFlaz.Application.Services
{
    /// <summary>
    /// Analisa uma imagem fornecida e determina se o seu conteúdo aparenta representar um livro,
    /// recorrendo ao serviço de reconhecimento de imagens do Google Cloud Vision API.
    /// </summary>
    /// <param name="imagem">
    /// Objeto <see cref="IFormFile"/> que representa o ficheiro de imagem enviado pelo utilizador.
    /// </param>
    /// <returns>
    /// Um valor booleano que indica se a imagem contém características associadas a livros:<br/>
    /// <c>true</c> — se a imagem contiver elementos típicos de um livro (por exemplo: "book", "page", "text", "literature", "reading");<br/>
    /// <c>false</c> — caso contrário.
    /// </returns>
    public class ImagemService : IImagemService
    {
        public async Task<bool> VerificarSeELivroAsync(IFormFile imagem)
        {
            var tempPath = Path.GetTempFileName();

            await using (var stream = new FileStream(tempPath, FileMode.Create))
            {
                await imagem.CopyToAsync(stream);
            }

            var client = await ImageAnnotatorClient.CreateAsync();
            var image = Image.FromFile(tempPath);

            var response = await client.DetectLabelsAsync(image);

            var pareceLivro = response.Any(label =>
                  label.Description.ToLower().Contains("book") ||
                  label.Description.ToLower().Contains("page") ||
                  label.Description.ToLower().Contains("text") ||
                  label.Description.ToLower().Contains("literature") ||
                  label.Description.ToLower().Contains("reading")
            );

            File.Delete(tempPath);

            return pareceLivro;
        }
    }
}
