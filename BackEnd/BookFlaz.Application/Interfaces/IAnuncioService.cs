using BookFlaz.Application.DTOs;
using BookFlaz.Domain.Entities;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BookFlaz.Application.Interfaces
{
    /// <summary>
    /// Define o contrato para operações relacionadas com anúncios.
    /// </summary>
    public interface IAnuncioService
    {

        Task CriarAnuncioAsync(CriarAnuncioDTO dto, long vendedorId);

        Task RemoverAnuncioAsync(long id, long idUser, string? motivo);

        /// <summary>
        /// Edita um anúncio existente com novos dados.
        /// </summary>
        /// <param name="id">Identificador do anúncio.</param>
        /// <param name="dto">Objeto com os dados atualizados do anúncio.</param>
        /// <returns>
        /// Uma tupla <c>(Success, Message)</c> indicando o resultado da operação.
        /// </returns>
        Task EditarAnuncioAsync(long id, AtualizarAnuncioDTO dto, long idUser);

        Task<List<AnuncioFavoritoDTO>> PesquisarAnunciosAsync(FiltroAnuncioDTO filtro);

        Task<VisualizarAnuncioDTO> VisualizarAnuncio(long idAnuncio);
    }
}
