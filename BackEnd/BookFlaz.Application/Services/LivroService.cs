using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BookFlaz.Application.DTOs;
using BookFlaz.Application.Interfaces;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json.Linq;

namespace BookFlaz.Application.Services
{
    public class LivroService : IBookInfoService
    {
        private readonly HttpClient httpClient;
        private readonly IConfiguration _configuration;

        public LivroService(HttpClient httpClient, IConfiguration config)
        {
            this.httpClient = httpClient;
            _configuration = config;
        }

        /// <summary>
        /// Obtém as informações de um livro com base no número ISBN, recorrendo à API do Google Books.
        /// </summary>
        /// <param name="isbn">Número ISBN do livro a pesquisar.</param>
        /// <returns>
        /// Um objeto <see cref="LivroDTO"/> contendo as informações principais do livro,
        /// ou <c>null</c> caso não seja encontrado ou ocorra algum erro no pedido.
        /// </returns>
        /// <exception cref="ArgumentException">Lançada quando o ISBN é nulo, vazio ou contém apenas espaços.</exception>
        public async Task<LivroDTO?> ObterLivroPorISBNAsync(string isbn)
        {
            if (string.IsNullOrWhiteSpace(isbn))
            {
                throw new ArgumentException("O ISBN não pode ser nulo ou vazio.", nameof(isbn));
            }

            var baseUrl = _configuration["ExternalApis:GoogleBooks:BaseUrl"]
                              ?? "https://www.googleapis.com/books/v1/volumes";
            var apiKey = _configuration["ExternalApis:GoogleBooks:ApiKey"];

            var url = string.IsNullOrWhiteSpace(apiKey)
                ? $"{baseUrl}?q=isbn:{isbn}"
                : $"{baseUrl}?q=isbn:{isbn}&key={apiKey}";

            var response = await httpClient.GetAsync(url);

            if (!response.IsSuccessStatusCode)
            {
                return null;
            }

            var json = await response.Content.ReadAsStringAsync();
            var bookData = JObject.Parse(json);

            var items = (JArray?)bookData["items"];

            if (items == null || !items.Any())
            {
                return null;
            }

            var book = bookData["items"]?.FirstOrDefault()?["volumeInfo"];

            if (book == null)
            {
                return null;
            }

            return new LivroDTO
            {
                Isbn = long.Parse(isbn),
                Titulo = book["title"]?.ToString() ?? "Título não disponível",
                Autor = book["authors"]?.FirstOrDefault()?.ToString() ?? "Autor não disponível"
            };
        }
    }
}
