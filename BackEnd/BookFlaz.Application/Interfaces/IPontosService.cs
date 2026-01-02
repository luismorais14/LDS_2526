using BookFlaz.Application.DTOs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BookFlaz.Application.Interfaces
{
    public interface IPontosService
    {
        Task<int> ObterPontosAsync(long clienteId);
        Task<List<PontosMovimentadosDTO>> ObterHistoricoAsync(long clienteId);
        Task AdicionarPontos(long clienteId, int pontos, long transacaoId);
        Task RemoverPontos(long clienteId, int pontos, long transacaoId);
    }
}
