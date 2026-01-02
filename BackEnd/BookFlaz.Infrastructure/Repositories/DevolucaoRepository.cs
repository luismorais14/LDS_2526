using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BookFlaz.Domain.Entities;
using BookFlaz.Domain.Repositories;
using BookFlaz.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace BookFlaz.Infrastructure.Repositories
{
    public class DevolucaoRepository : IDevolucaoRepository
    {
        private readonly BooksContext _context;

        public DevolucaoRepository(BooksContext context)
        {
            _context = context;
        }

        public async Task AdicionarAsync(Devolucao devolucao)
        {
            await _context.Devolucoes.AddAsync(devolucao);
            await _context.SaveChangesAsync();
        }

        public async Task AtualizarAsync(Devolucao devolucao)
        {
            _context.Devolucoes.Update(devolucao);
            await _context.SaveChangesAsync();
        }

        public async Task<Devolucao?> ObterPorTransacaoIdAsync(long transacaoId)
        {
            return await _context.Devolucoes
               .FirstOrDefaultAsync(d => d.TransacaoId == transacaoId);
        }
    }
}
