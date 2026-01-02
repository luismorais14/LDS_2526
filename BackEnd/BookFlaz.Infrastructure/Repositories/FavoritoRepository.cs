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
    public class FavoritoRepository : IFavoritoRepository
    {
        private readonly BooksContext _context;

        public FavoritoRepository(BooksContext context)
        {
            _context = context;
        }

        public async Task<Favorito?> ObterAsync(long clienteId, long anuncioId)
        {
            return await _context.Favoritos
                .FirstOrDefaultAsync(f => f.ClienteId == clienteId && f.AnuncioId == anuncioId);
        }

        public async Task<int> ContarPorClienteAsync(long clienteId)
        {
            return await _context.Favoritos.CountAsync(f => f.ClienteId == clienteId);
        }

        public async Task<List<Favorito>> ObterPorAnuncioAsync(long anuncioId)
        {
            return await _context.Favoritos.Where(f => f.AnuncioId != anuncioId).ToListAsync();
        }

        public async Task<List<Favorito>> ObterPorClienteAsync(long clienteId)
        {
            return await _context.Favoritos
                .Where(f => f.ClienteId == clienteId)
                .ToListAsync();
        }

        public async Task<int> ContarPorAnuncioAsync(long anuncioId)
        {
            return await _context.Favoritos.CountAsync(f => f.AnuncioId == anuncioId);
        }

        public async Task AdicionarAsync(Favorito favorito)
        {
            await _context.Favoritos.AddAsync(favorito);
            await _context.SaveChangesAsync();
        }

        public async Task RemoverAsync(Favorito favorito)
        {
            _context.Favoritos.Remove(favorito);
            await _context.SaveChangesAsync();
        }
    }
}
