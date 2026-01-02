using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BookFlaz.Application.DTOs;
using BookFlaz.Domain.Entities;

namespace BookFlaz.Application.Interfaces
{
	public interface ITransacaoService
	{

        // / <summary>
        // / Cria uma nova transação com base nos dados fornecidos.
        // / </summary>
        Task<TransacaoDTO> CriarTransacaoAsync(CriarTransacaoDTO dto, long compradorId);

        Task CancelarTransacaoAsync(long transacaoId, long utilizadorId);

        /// <summary>
        /// Confirma a receção do item pelo comprador.
        /// </summary>
        Task ConfirmarRececaoCompradorAsync(long transacaoId, long utilizadorId);

        /// <summary>
        /// Confirma a devolução do item pelo vendedor.
        /// </summary>
        Task ConfirmarDevolucaoAsync(long transacaoId, long vendedorId);

        /// <summary>
        /// Regista uma devolução para a transação especificada.
        /// </summary>
        Task RegistarDevolucaoAsync(long transacaoId, long compradorId);


		/// <summary>
		/// Obtém o resumo das transações do utilizador com base no filtro fornecido.
        /// </summary>
		Task<List<TransacaoResumoDTO>> ObterRegistoAsync(long utilizadorId, TransacaoFiltroDTO filtro);
	}
}
