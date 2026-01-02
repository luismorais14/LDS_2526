using BookFlaz.Application.DTOs;
using BookFlaz.Domain.Entities;

namespace BookFlaz.Application.Interfaces;

public interface IAvaliacaoService
{
    Task<Avaliacao> AvaliarAsync(long avaliadorId, long avaliadoId, long transacaoId, AvaliacaoDTO dto);
}