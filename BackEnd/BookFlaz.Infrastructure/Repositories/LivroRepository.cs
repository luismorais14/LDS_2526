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
    public class LivroRepository : ILivroRepository
    {
        private readonly BooksContext _context;

        public LivroRepository(BooksContext context)
        {
            _context = context;
        }

        public async Task<bool> ExisteAsync(long isbn)
        {
            return await _context.Livros.AnyAsync(l => l.Isbn == isbn);
        }

        public async Task<Livro?> ObterPorIsbnAsync(long isbn)
        {
            return await _context.Livros.FirstOrDefaultAsync(l => l.Isbn == isbn);
        }

        public async Task AdicionarAsync(Livro livro)
        {
            await _context.Livros.AddAsync(livro);
            await _context.SaveChangesAsync();
        }
    }
}
