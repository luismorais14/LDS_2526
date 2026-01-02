using System.Net.Http;
using System.Threading.Tasks;
using BookFlaz.Application.Services;
using Microsoft.Extensions.Configuration;
using Xunit;

namespace BookFlaz.Tests.Application.Services.Integracao
{
    /// <summary>
    /// Testes de integração para a classe <see cref="LivroService"/>, responsáveis por
    /// validar o comportamento real do serviço quando comunica com a API pública do Google Books.
    /// </summary>
    public class LivroServiceTestes
    {

        private readonly IConfiguration _configuration;

        public LivroServiceTestes()
        {
            var builder = new ConfigurationBuilder()
                .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true)
                .AddEnvironmentVariables();
            _configuration = builder.Build();
        }

        /// <summary>
        /// Teste de integração que confirma que o método 
        /// <see cref="LivroService.ObterLivroPorISBNAsync(string)"/> devolve um 
        /// <see cref="BookFlaz.Application.DTOs.LivroDTO"/> válido ao consultar a API real
        /// do Google Books com um ISBN existente.           
        /// </summary>
        [Fact]
        public async Task ObterLivroPorISBNAsync_ComAPIReal_DeveRetornarLivroValido()
        {
            var httpClient = new HttpClient();
            var service = new LivroService(httpClient, _configuration);
            var isbn = "9780140440263";

            var result = await service.ObterLivroPorISBNAsync(isbn);

            Assert.NotNull(result);
            Assert.True(result!.Titulo.Length > 0);
            Assert.NotEqual("Autor não disponível", result.Autor);
        }

        /// <summary>
        /// Teste de integração que confirma que o método 
        /// <see cref="LivroService.ObterLivroPorISBNAsync(string)"/> devolve <c>null</c>
        /// quando é fornecido um ISBN inválido ou inexistente.
        [Fact]
        public async Task ObterLivroPorISBNAsync_ComISBNInvalido_DeveRetornarNull()
        {
            var httpClient = new HttpClient();
            var service = new LivroService(httpClient, _configuration);
            var isbn = "0000000000000";

            var result = await service.ObterLivroPorISBNAsync(isbn);

            Assert.Null(result);
        }
    }
}
