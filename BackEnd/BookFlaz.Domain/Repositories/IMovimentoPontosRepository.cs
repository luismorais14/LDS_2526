using BookFlaz.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BookFlaz.Domain.Repositories
{
    public interface IMovimentoPontosRepository
    {
        Task<List<MovimentoPontos>> ObterMovimentosPorClienteAsync(long clienteId);
        Task AdicionarMovimento(MovimentoPontos movimento);
    }
}
