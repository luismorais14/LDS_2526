using BookFlaz.Application.Services;
using Microsoft.Extensions.Configuration;
using Moq;
using Moq.Protected;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;


namespace BookFlaz.Tests.Application.Services.Unitarios
{

    /// <summary>
    /// Testes unitários para a classe <see cref="LivroService"/>, que é responsável por
    /// obter informações de livros a partir da API Google Books.
    /// </summary>
    public class LivroServiceTests
    {
        private readonly IConfiguration _configuration;

        public LivroServiceTests()
        {
            var inMemorySettings = new Dictionary<string, string> {
                {"GoogleBooksApi:BaseUrl", "https://www.googleapis.com/books/v1/volumes"},
                {"GoogleBooksApi:ApiKey", "myapikeyfake"}
            };
            _configuration = new ConfigurationBuilder()
                .AddInMemoryCollection(inMemorySettings)
                .Build();
        }

        /// <summary>
        /// Cria uma instância de <see cref="HttpClient"/> configurada para devolver uma
        /// resposta simulada com o conteúdo JSON fornecido.
        /// 
        /// Este método é essencial para evitar chamadas reais à API externa, permitindo
        /// o isolamento total do teste.
        /// </summary>
        /// <param name="conteudoJson">Conteúdo JSON simulado que será devolvido pela requisição.</param>
        /// <param name="statusCode">Código de estado HTTP a devolver (por omissão, 200 OK).</param>
        /// <returns>Uma instância de <see cref="HttpClient"/> com resposta controlada.</returns>

        private HttpClient CriarHttpClientComResposta(string conteudoJson, HttpStatusCode statusCode = HttpStatusCode.OK)
        {
            var handlerMock = new Mock<HttpMessageHandler>();

            handlerMock.Protected()
                .Setup<Task<HttpResponseMessage>>(
                    "SendAsync",
                    ItExpr.IsAny<HttpRequestMessage>(),
                    ItExpr.IsAny<CancellationToken>()
                )
                .ReturnsAsync(new HttpResponseMessage
                {
                    StatusCode = statusCode,
                    Content = new StringContent(conteudoJson)
                });

            return new HttpClient(handlerMock.Object);
        }

        /// <summary>
        /// Verifica se o método <see cref="LivroService.ObterLivroPorISBNAsync"/> devolve um
        /// objeto <see cref="BookFlaz.Application.DTOs.LivroDTO"/> válido quando a resposta JSON
        /// contém título e autor.
        /// </summary>
        [Fact]
        public async Task ObterLivroPorISBNAsync_DeveRetornarLivroDTO_QuandoRespostaValida()
        {
            var isbn = "9789720047081";

            var json = @"{
                'items': [
                    {
                        'volumeInfo': {
                            'title': 'História de Portugal',
                            'authors': ['José Mattoso']
                        }
                    }
                ]
            }";

            var httpClient = CriarHttpClientComResposta(json);
            var service = new LivroService(httpClient, _configuration);

            var result = await service.ObterLivroPorISBNAsync(isbn);

            Assert.NotNull(result);
            Assert.Equal(long.Parse(isbn), result.Isbn);
            Assert.Equal("História de Portugal", result.Titulo);
            Assert.Equal("José Mattoso", result.Autor);
        }

        /// <summary>
        /// Verifica se o método devolve <c>null</c> quando a resposta JSON da API
        /// não contém nenhum item no array <c>items</c>.
        /// </summary>
        [Fact]
        public async Task ObterLivroPorISBNAsync_DeveRetornarNull_QuandoNaoExistemItems()
        {
            var isbn = "1234567890123";
            var json = @"{ 'items': [] }";
            var httpClient = CriarHttpClientComResposta(json);
            var service = new LivroService(httpClient, _configuration);

            var result = await service.ObterLivroPorISBNAsync(isbn);

            Assert.Null(result);
        }

        /// <summary>
        /// Garante que o método lança uma <see cref="ArgumentException"/> quando o
        /// parâmetro <paramref name="isbn"/> é nulo ou vazio, cumprindo as regras de validação.
        /// </summary>
        [Fact]
        public async Task ObterLivroPorISBNAsync_DeveLancarArgumentException_QuandoISBNVazio()
        {
            var httpClient = CriarHttpClientComResposta("{}");
            var service = new LivroService(httpClient, _configuration);

            await Assert.ThrowsAsync<ArgumentException>(() => service.ObterLivroPorISBNAsync(""));
        }

        /// <summary>
        /// Verifica se o método retorna valores por defeito ("Título não disponível" e
        /// "Autor não disponível") quando o JSON da API não contém campos de título
        /// nem autores.
        /// 
        /// Este teste garante robustez contra respostas incompletas da API externa.
        /// </summary>
        [Fact]
        public async Task ObterLivroPorISBNAsync_DeveUsarValoresPorDefeito_QuandoCamposFaltam()
        {
            var isbn = "9789720047081";
            var json = @"{
                'items': [
                    {
                        'volumeInfo': { }
                    }
                ]
            }";

            var httpClient = CriarHttpClientComResposta(json);
            var service = new LivroService(httpClient, _configuration);

            var result = await service.ObterLivroPorISBNAsync(isbn);

            Assert.NotNull(result);
            Assert.Equal("Título não disponível", result.Titulo);
            Assert.Equal("Autor não disponível", result.Autor);
        }
    }
}

