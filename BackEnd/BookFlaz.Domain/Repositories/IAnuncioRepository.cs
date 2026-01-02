using BookFlaz.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BookFlaz.Domain.Repositories
{
    public interface IAnuncioRepository
    {
        Task<Anuncio?> ObterPorIdAsync(long id);
        Task<List<Anuncio>> ObterAtivosComLivroEVendedorAsync();
        Task AdicionarAsync(Anuncio anuncio);
        Task Atualizar(Anuncio anuncio);
        Task RemoverAsync(Anuncio anuncio);
        Task<int> ContarFavoritosAsync(long anuncioId);
    }
}
