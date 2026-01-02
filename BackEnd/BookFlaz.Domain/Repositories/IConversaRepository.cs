using BookFlaz.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BookFlaz.Domain.Repositories
{
    public interface IConversaRepository
    {
        Task<Conversa?> ObterEntreAsync(long compradorId, long vendedorId, long anuncioId);
        Task<Conversa?> ObterPorIdAsync(long id);
        Task AdicionarAsync(Conversa conversa);
    }
}
