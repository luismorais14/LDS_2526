using BookFlaz.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BookFlaz.Domain.Repositories
{
    public interface IFavoritoRepository
    {
        Task<Favorito?> ObterAsync(long clienteId, long anuncioId);
        Task<List<Favorito>> ObterPorAnuncioAsync(long anuncioId);
        Task<int> ContarPorClienteAsync(long clienteId);
        Task<List<Favorito>> ObterPorClienteAsync(long clienteId);
        Task<int> ContarPorAnuncioAsync(long anuncioId);
        Task AdicionarAsync(Favorito favorito);
        Task RemoverAsync(Favorito favorito);
    }
}
