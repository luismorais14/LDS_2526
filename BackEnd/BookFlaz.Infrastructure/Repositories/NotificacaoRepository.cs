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
    public class NotificacaoRepository: INotificacaoRepository
    {
        private readonly BooksContext _context;

        public NotificacaoRepository(BooksContext context)
        {
            _context = context;
        }

        public async Task CriarNotificacao(Notificacao notificacao)
        {
            await _context.Notificacoes.AddAsync(notificacao);
            await _context.SaveChangesAsync();
        }

        public async Task<List<Notificacao>> ObterNotificacoes(long id)
        {
            return await _context.Notificacoes
                .AsNoTracking()
                .Where(m => m.Id == id)
                .OrderBy(m => m.DataEnvio)
                .ToListAsync();
        }
    }
}
