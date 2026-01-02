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
    public class MovimentoPontosRepository : IMovimentoPontosRepository
    {
        private readonly BooksContext _context;

        public MovimentoPontosRepository(BooksContext context)
        {
            _context = context;
        }

        public async Task AdicionarMovimento(MovimentoPontos movimento)
        {
            await _context.MovimentosPontos.AddAsync(movimento);
            await _context.SaveChangesAsync();
        }

        public async Task<List<MovimentoPontos>> ObterMovimentosPorClienteAsync(long clienteId)
        {
            return await _context.MovimentosPontos
                .Where(m => m.ClienteId == clienteId)
                .OrderByDescending(m => m.DataMovimento)
                .ToListAsync();
        }
    }
}
