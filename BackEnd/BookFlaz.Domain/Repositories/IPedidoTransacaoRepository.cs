using BookFlaz.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BookFlaz.Domain.Repositories
{
    public interface IPedidoTransacaoRepository
    {
        Task<List<PedidoTransacao>> ObterPorAnuncioIdAsync(long anuncioId);
        Task RemoverRangeAsync(IEnumerable<PedidoTransacao> pedidos);
        Task AdicionarAsync(PedidoTransacao pedido);
        Task<PedidoTransacao?> ObterPorIdAsync(long id);
        Task AtualizarAsync(PedidoTransacao pedido);
        Task<bool> ExistePendenteEntreAsync(long remetenteId, long destinatarioId, long anuncioId);
        Task<PedidoTransacao?> ObterPendenteEntreAsync(long utilizador1, long utilizador2, long anuncioId);
        Task<bool> ExistemPedidosNaConversaAsync(long conversaId);
        Task<PedidoTransacao?> ObterUltimoPedidoDaConversaAsync(long conversaId);

    }
}
