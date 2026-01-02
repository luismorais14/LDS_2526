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
    public class TransacaoRepository : ITransacaoRepository
    {
        private readonly BooksContext _context;

        public TransacaoRepository(BooksContext context)
        {
            _context = context;
        }

        public async Task AdicionarAsync(Transacao transacao)
        {
            await _context.Transacoes.AddAsync(transacao);
            await _context.SaveChangesAsync();
        }

        public async Task AtualizarAsync(Transacao transacao)
        {
            _context.Transacoes.Update(transacao);
            await _context.SaveChangesAsync();
        }

        public async Task<bool> ExistePorPedidoIdAsync(long pedidoId)
        {
            return await _context.Transacoes.AnyAsync(t => t.PedidoId == pedidoId);
        }

        public async Task<bool> ExisteTransacaoAtivaAsync(List<long> pedidosIds)
        {
            return await _context.Transacoes.AnyAsync(t =>
                pedidosIds.Contains(t.PedidoId) &&
                t.EstadoTransacao != EstadoTransacao.CONCLUIDA &&
                t.EstadoTransacao != EstadoTransacao.CANCELADA);
        }

        public async Task<List<Transacao>> ObterPorClienteIdAsync(long clienteId)
        {
            return await _context.Transacoes
               .Where(t => t.CompradorId == clienteId || t.VendedorId == clienteId)
               .OrderByDescending(t => t.DataCriacao)
               .ToListAsync();
        }

        public async Task<Transacao?> ObterPorIdAsync(long id)
        {
            return await _context.Transacoes.FindAsync(id);
        }

		
		public async Task<List<Transacao>> ObterDoUtilizadorAsync(
			long utilizadorId,
			DateTime? de,
			DateTime? ate,
			EstadoTransacao? estado)
		{
			var q = _context.Transacoes.AsQueryable();

			
			q = q.Where(t => t.CompradorId == utilizadorId || t.VendedorId == utilizadorId);

			if (de.HasValue) q = q.Where(t => t.DataCriacao >= de.Value);
			if (ate.HasValue) q = q.Where(t => t.DataCriacao <= ate.Value);
			if (estado.HasValue) q = q.Where(t => t.EstadoTransacao == estado.Value);

			return await q.OrderByDescending(t => t.DataCriacao).ToListAsync();
		}

		public async Task<Transacao?> ObterPorPedidoIdAsync(long pedidoId)
        {
            return await _context.Transacoes
                .FirstOrDefaultAsync(t => t.PedidoId == pedidoId);
        }
    }
}

