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
    public class ProfileRepository : IProfileRepository
    {
        private readonly BooksContext _context;

        public ProfileRepository(BooksContext context)
        {
            _context = context;
        }

        public async Task<Cliente> ObterPerfilAsync(long id)
        {
            return await _context.Clientes.Include(c => c.AnunciosVendedor)
                .Include(c => c.AvaliacoesRecebidas)
                .SingleOrDefaultAsync(c => c.Id == id);
        }
    }
}
