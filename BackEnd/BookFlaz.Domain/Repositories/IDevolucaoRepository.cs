using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BookFlaz.Domain.Entities;

namespace BookFlaz.Domain.Repositories
{
    public interface IDevolucaoRepository
    {
        Task<Devolucao?> ObterPorTransacaoIdAsync(long transacaoId);
        Task AdicionarAsync(Devolucao devolucao);
        Task AtualizarAsync(Devolucao devolucao);
    }
}
