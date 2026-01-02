using BookFlaz.Domain.Entities;
using BookFlaz.Domain.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BookFlaz.Domain.Repositories
{
    public interface ITransacaoRepository
    {
        Task<bool> ExisteTransacaoAtivaAsync(List<long> pedidosIds);
        Task<Transacao?> ObterPorIdAsync(long id);
        Task AdicionarAsync(Transacao transacao);
        Task AtualizarAsync(Transacao transacao);
        Task<List<Transacao>> ObterPorClienteIdAsync(long clienteId);
        Task<bool> ExistePorPedidoIdAsync(long pedidoId);
        Task<List<Transacao>> ObterDoUtilizadorAsync(long vendedorId, DateTime? de, DateTime? ate, EstadoTransacao? estado);
        Task<Transacao?> ObterPorPedidoIdAsync(long pedidoId);
    }
}
