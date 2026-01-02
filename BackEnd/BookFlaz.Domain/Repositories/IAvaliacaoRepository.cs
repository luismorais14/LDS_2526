using BookFlaz.Domain.Entities;

namespace BookFlaz.Domain.Repositories;

public interface IAvaliacaoRepository
{
    Task<bool> ExisteAsync(long transacaoId, long clienteId);
    Task AdicionarAsync(Avaliacao avaliacao);
    Task<(double Media, int Total)> CalcularReputacaoAsync(long clienteId);
}