using BookFlaz.Domain.Entities;
using BookFlaz.Domain.Repositories;
using BookFlaz.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace BookFlaz.Infrastructure.Repositories
{
    public class CategoriaRepository : ICategoriaRepository
    {
        private readonly BooksContext _context;

        public CategoriaRepository(BooksContext context)
        {
            _context = context;
        }

        public async Task<Categoria?> ObterPorIdAsync(long id)
        {
            return await _context.Categorias.FindAsync(id);
        }

        public async Task<Categoria?> ObterPorNomeAsync(string nome)
        {
            return await _context.Categorias
                .AsNoTracking()
                .FirstOrDefaultAsync(c => c.Nome == nome);
        }

        public async Task<bool> ExisteAsync(long id)
        {
            return await _context.Categorias.AnyAsync(c => c.Id == id);
        }

        public async Task<bool> ExisteComMesmoNomeAsync(string nome, long? ignorandoId = null)
        {
            return await _context.Categorias
                .AnyAsync(c => c.Nome == nome && (!ignorandoId.HasValue || c.Id != ignorandoId.Value));
        }

        public async Task<List<Categoria>> ListarAsync(string? nome, bool? ativo)
        {
            var query = _context.Categorias.AsQueryable();

            if (!string.IsNullOrWhiteSpace(nome))
                query = query.Where(c => c.Nome.Contains(nome));

            if (ativo.HasValue)
                query = query.Where(c => c.Ativo == ativo.Value);

            return await query.AsNoTracking().ToListAsync();
        }

        public async Task<bool> TemAnunciosVinculadosAsync(long categoriaId)
        {
            return await _context.Anuncios.AnyAsync(a => a.CategoriaId == categoriaId);
        }

        public async Task AdicionarAsync(Categoria categoria)
        {
            await _context.Categorias.AddAsync(categoria);
            await _context.SaveChangesAsync();
        }

        public void Atualizar(Categoria categoria)
        {
            _context.Categorias.Update(categoria);
            _context.SaveChanges();
        }

        public void Remover(Categoria categoria)
        {
            _context.Categorias.Remove(categoria);
            _context.SaveChanges();
        }
    }
}
