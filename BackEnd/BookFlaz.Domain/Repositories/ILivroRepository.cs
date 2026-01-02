using BookFlaz.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BookFlaz.Domain.Repositories
{
    public interface ILivroRepository
    {
        Task<bool> ExisteAsync(long isbn);
        Task<Livro?> ObterPorIsbnAsync(long isbn);
        Task AdicionarAsync(Livro livro);
    }
}
