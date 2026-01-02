using BookFlaz.Domain.Repositories;
using BookFlaz.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BookFlaz.Domain.Entities;

namespace BookFlaz.Infrastructure.Repositories
{
    public class ClienteRepository : IClienteRepository
    {
        private readonly BooksContext _context;

        public ClienteRepository(BooksContext context)
        {
            _context = context;
        }

        public async Task<bool> ExisteAsync(long id)
        {
            return await _context.Clientes.AnyAsync(c => c.Id == id);
        }

        public async Task<bool> ExisteAsync(string email)
        {
            return await _context.Clientes.AnyAsync(c => c.Email == email);
        }

        public async Task<bool> GuardarAsync(Cliente cliente)
        {
            await _context.Clientes.AddAsync(cliente);
            return await _context.SaveChangesAsync() > 0;
        }

        public async Task<Cliente> ObterPorEmailAsync(string email)
        {
            return await _context.Clientes.FirstOrDefaultAsync(c => c.Email == email);
        }

        public async Task<bool> AtualizarAsync(Cliente cliente)
        {
            _context.Entry(cliente).State = EntityState.Modified;
            return await _context.SaveChangesAsync() > 0;
        }

        public async Task<Cliente> ObterPorIdAsync(long id)
        {
            return await _context.Clientes.FindAsync(id);
        }

        public async Task<bool> EliminarAsync(Cliente client)
        {
            _context.Clientes.Remove(client);
            return await _context.SaveChangesAsync() > 0;
        }

        public async Task<int> ObterPontosAsync(long clienteId)
        {
            return await _context.Clientes
                .Where(c => c.Id == clienteId)
                .Select(c => c.Pontos)
                .FirstOrDefaultAsync();
        }
    }
}
