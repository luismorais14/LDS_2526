using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BookFlaz.Domain.Entities;

namespace BookFlaz.Domain.Repositories
{
    public interface IClienteRepository
    {
        Task<bool> ExisteAsync(long id);

        Task<bool> ExisteAsync(string email);

        Task<bool> GuardarAsync(Cliente cliente);

        Task<Cliente> ObterPorEmailAsync(string email);

        Task<bool> AtualizarAsync(Cliente cliente);

        Task<Cliente> ObterPorIdAsync(long id);

        Task<bool> EliminarAsync(Cliente client);

        Task<int> ObterPontosAsync(long clienteId);
    }
}
