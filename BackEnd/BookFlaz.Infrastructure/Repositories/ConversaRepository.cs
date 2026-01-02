using BookFlaz.Domain.Entities;
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
    public class ConversaRepository : IConversaRepository
    {
        private readonly BooksContext _context;
        public ConversaRepository(BooksContext context) => _context = context;

        public async Task<Conversa?> ObterEntreAsync(long compradorId, long vendedorId, long anuncioId)
        {
            return await _context.Conversas
                .FirstOrDefaultAsync(c =>
                    c.CompradorId == compradorId &&
                    c.VendedorId == vendedorId &&
                    c.AnuncioId == anuncioId);
        }

        public async Task<Conversa?> ObterPorIdAsync(long id)
        {
            return await _context.Conversas.FindAsync(id);
        }

        public async Task AdicionarAsync(Conversa conversa)
        {
            await _context.Conversas.AddAsync(conversa);
            await _context.SaveChangesAsync();
        }
    }
}
