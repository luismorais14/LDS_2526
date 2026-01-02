using BookFlaz.Application.DTOs;
using BookFlaz.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BookFlaz.Application.Interfaces
{
    public interface IPedidoTransacaoService
    {
        Task<(long PedidoId, long ConversaId)> CriarPedidoAsync(CriarPedidoTransacaoDTO dto, long idCliente);
        Task AceitarPedidoAsync(long pedidoId, long utilizadorId);
        Task RejeitarPedidoAsync(long pedidoId, long utilizadorId);

        Task<PedidoTransacao?> ObterPorIdAsync(long id, long userId);
    }
}