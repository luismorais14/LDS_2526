using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BookFlaz.Application.DTOs;

namespace BookFlaz.Application.Interfaces
{
    /// <summary>
    /// Define a interface para o serviço responsável por obter informações detalhadas de livros
    /// com base no seu código ISBN.
    public interface IBookInfoService
    {
        /// <summary>
        /// Obtém as informações de um livro a partir do código ISBN fornecido.
        /// </summary>
        Task<LivroDTO> ObterLivroPorISBNAsync(string isbn);
    }
}
