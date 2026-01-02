using BookFlaz.Domain.Entities;
using BookFlaz.Domain.Enums;
using BookFlaz.Domain.Repositories;
using BookFlaz.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BookFlaz.Infrastructure.Repositories
{
    public class PedidoTransacaoRepository : IPedidoTransacaoRepository
    {
        private readonly BooksContext _context;

        public PedidoTransacaoRepository(BooksContext context)
        {
            _context = context;
        }

        public async Task<List<PedidoTransacao>> ObterPorAnuncioIdAsync(long anuncioId)
        {
            return await _context.PedidosTransacao
                .Where(p => p.AnuncioId == anuncioId)
                .ToListAsync();
        }

        public async Task RemoverRangeAsync(IEnumerable<PedidoTransacao> pedidos)
        {
            _context.PedidosTransacao.RemoveRange(pedidos);
            await _context.SaveChangesAsync();
        }
        public async Task AdicionarAsync(PedidoTransacao pedido)
        {
            await _context.PedidosTransacao.AddAsync(pedido);
            await _context.SaveChangesAsync();
        }

        public async Task<PedidoTransacao?> ObterPorIdAsync(long id)
        {
            return await _context.PedidosTransacao
               .FirstOrDefaultAsync(p => p.Id == id);
        }

        public async Task AtualizarAsync(PedidoTransacao pedido)
        {
            _context.PedidosTransacao.Update(pedido);
            await _context.SaveChangesAsync();
        }

        public async Task<bool> ExistePendenteEntreAsync(long remetenteId, long destinatarioId, long anuncioId)
        {
            return await _context.PedidosTransacao.AnyAsync(p =>
                p.RemetenteId == remetenteId &&
                p.DestinatarioId == destinatarioId &&
                p.AnuncioId == anuncioId &&
                p.EstadoPedido == EstadoPedido.PENDENTE
            );
        }
        public async Task<bool> ExistemPedidosNaConversaAsync(long conversaId)
        {
            return await _context.PedidosTransacao
                .AnyAsync(p => p.ConversaId == conversaId);
        }

        public async Task<PedidoTransacao?> ObterPendenteEntreAsync(long utilizador1, long utilizador2, long anuncioId)
        {
            return await _context.PedidosTransacao
                .Where(p =>
                    p.AnuncioId == anuncioId &&
                    p.EstadoPedido == EstadoPedido.PENDENTE &&
                    ((p.CompradorId == utilizador1 && p.VendedorId == utilizador2) ||
                     (p.CompradorId == utilizador2 && p.VendedorId == utilizador1)))
                .FirstOrDefaultAsync();
        }

        public async Task<PedidoTransacao?> ObterUltimoPedidoDaConversaAsync(long conversaId)
        {
            return await _context.PedidosTransacao
                .Where(p => p.ConversaId == conversaId)
                .OrderByDescending(p => p.DataCriacao) 
                .FirstOrDefaultAsync();
        }

    }
}
