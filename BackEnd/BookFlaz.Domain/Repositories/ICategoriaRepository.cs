using BookFlaz.Domain.Entities;

namespace BookFlaz.Domain.Repositories
{
    public interface ICategoriaRepository
    {
        Task<Categoria?> ObterPorIdAsync(long id);
        Task<Categoria?> ObterPorNomeAsync(string nome);
        Task<bool> ExisteAsync(long id);
        Task<bool> ExisteComMesmoNomeAsync(string nome, long? ignorandoId = null);

        Task<List<Categoria>> ListarAsync(string? nome, bool? ativo);

        Task<bool> TemAnunciosVinculadosAsync(long categoriaId);

        Task AdicionarAsync(Categoria categoria);
        void Atualizar(Categoria categoria);
        void Remover(Categoria categoria);
    }
}
