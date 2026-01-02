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
    public class AnuncioRepository : IAnuncioRepository
    {
        private readonly BooksContext _context;

        public AnuncioRepository(BooksContext context)
        {
            _context = context;
        }

        public async Task<Anuncio?> ObterPorIdAsync(long id)
        {
            return await _context.Anuncios
                .Include(a => a.Livro)
                .FirstOrDefaultAsync(a => a.Id == id);
        }

        public async Task<List<Anuncio>> ObterAtivosComLivroEVendedorAsync()
        {
            return await _context.Anuncios
                .Include(a => a.Livro)
                .Include(a => a.Vendedor)
                .Include(a => a.Categoria)
                .Where(a => a.EstadoAnuncio == EstadoAnuncio.ATIVO)
                .ToListAsync();
        }

        public async Task AdicionarAsync(Anuncio anuncio)
        {
            await _context.Anuncios.AddAsync(anuncio);
            await _context.SaveChangesAsync();
        }

        public async Task Atualizar(Anuncio anuncio)
        {
            _context.Anuncios.Update(anuncio);
            await _context.SaveChangesAsync();
        }

        public async Task RemoverAsync(Anuncio anuncio)
        {
            _context.Anuncios.Remove(anuncio);
            await _context.SaveChangesAsync();
        }

        public async Task<int> ContarFavoritosAsync(long anuncioId)
        {
            return await _context.Favoritos.CountAsync(f => f.AnuncioId == anuncioId);
        }
    }
}
