using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BookFlaz.Application.DTOs;

namespace BookFlaz.Application.Interfaces
{
    public interface IFavoritoService
    {
        /// <summary>
        /// Alterna o estado de favorito de um livro para o utilizador indicado.
        /// Se o livro ainda não for favorito, será adicionado;
        /// caso contrário, será removido dos favoritos.
        /// </summary>
        Task AlternarFavoritoAsync(long clienteId, long anuncioId);


        /// <summary>
        /// Obtém a lista de anuncios marcados como favoritos por um determinado utilizador.
        /// </summary>
        Task<List<AnuncioFavoritoDTO>> ObterAnunciosFavoritosAsync(long clienteId);

    }
}
