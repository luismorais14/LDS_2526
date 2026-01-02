using BookFlaz.Application.DTOs;
using BookFlaz.Application.Exceptions;
using BookFlaz.Application.Interfaces;
using BookFlaz.Domain.Entities;
using BookFlaz.Domain.Enums;
using BookFlaz.Domain.Repositories;

namespace BookFlaz.Application.Services
{
    /// <summary>
    /// Serviço responsável pela gestão dos favoritos no sistema BookFlaz.
    /// Implementa as regras de negócio relacionadas com a adição, remoção
    /// e listagem de anúncios favoritos de um cliente.
    /// </summary>
    public class FavoritoService : IFavoritoService
    {
        private readonly IFavoritoRepository _favoritoRepo;
        private readonly IClienteRepository _clienteRepo;
        private readonly IAnuncioRepository _anuncioRepo;

        public FavoritoService(
           IFavoritoRepository favoritoRepo,
           IClienteRepository clienteRepo,
           IAnuncioRepository anuncioRepo)
        {
            _favoritoRepo = favoritoRepo;
            _clienteRepo = clienteRepo;
            _anuncioRepo = anuncioRepo;
        }

        /// <summary>
        /// Adiciona ou remove um anúncio da lista de favoritos de um cliente.
        /// </summary>
        /// <param name="clienteId">ID do cliente autenticado.</param>
        /// <param name="anuncioId">ID do anúncio a ser favoritado/removido.</param>
        /// <exception cref="NotFoundException">Cliente ou anúncio inexistente.</exception>
        /// <exception cref="BusinessException">Se tentar favoritar o próprio anúncio.</exception>
        /// <exception cref="ValidationException">Se exceder limite máximo de favoritos.</exception>
        /// <exception cref="ApplicationException">Erro inesperado.</exception>
        public async Task AlternarFavoritoAsync(long clienteId, long anuncioId)
        {
            try
            {
                await ValidarClienteAsync(clienteId);

                var anuncio = await ObterAnuncioValidoAsync(anuncioId, clienteId);

                var favoritoExistente = await _favoritoRepo.ObterAsync(clienteId, anuncioId);

                if (favoritoExistente != null)
                {
                    await _favoritoRepo.RemoverAsync(favoritoExistente);

                    return;
                }

                await ValidarLimiteFavoritosAsync(clienteId);

                var novoFavorito = Favorito.AdicionarFavorito(anuncioId, clienteId);

                await _favoritoRepo.AdicionarAsync(novoFavorito);
            }
            catch (NotFoundException)
            {
                throw; 
            }
            catch (BusinessException)
            {
                throw;
            }
            catch (ValidationException)
            {
                throw;
            }
            catch (Exception)
            {
                throw new ApplicationException("Ocorreu um erro inesperado. Tente novamente mais tarde.");
            }
        }

        /// <summary>
        /// Obtém todos os anúncios marcados como favoritos por um determinado cliente.
        /// </summary>
        /// <param name="clienteId">Identificador único do cliente.</param>
        /// <returns>
        /// Lista de objetos <see cref="AnuncioFavoritoDTO"/> representando os anúncios favoritos do cliente.
        /// Retorna uma lista vazia caso o cliente não tenha favoritos.
        /// </returns>
        /// <exception cref="NotFoundException"> Lançada quando o cliente não existe. </exception>
        /// <exception cref="ApplicationException"> Lançada em caso de erro inesperado ao obter os favoritos.</exception>
        public async Task<List<AnuncioFavoritoDTO>> ObterAnunciosFavoritosAsync(long clienteId)
        {
            try
            {
                await ValidarClienteAsync(clienteId);

                var favoritos = await _favoritoRepo.ObterPorClienteAsync(clienteId);

                if (!favoritos.Any())
                {
                    return new List<AnuncioFavoritoDTO>();
                }

                var anuncios = await _anuncioRepo.ObterAtivosComLivroEVendedorAsync();

                return MapearAnunciosFavoritos(anuncios, favoritos);
            }
            catch (NotFoundException ex)
            {
                throw;
            }
            catch (Exception ex)
            {
                throw new ApplicationException("Ocorreu um erro inesperado. Tente novamente mais tarde.");
            }
        }

        /// <summary>
        /// Verifica se o cliente existe na base de dados.
        /// </summary>
        /// <param name="clienteId">Identificador do cliente.</param>
        /// <exception cref="NotFoundException">Lançada se o cliente não for encontrado.</exception>
        private async Task ValidarClienteAsync(long clienteId)
        {
            if (!await _clienteRepo.ExisteAsync(clienteId))
            {
                throw new NotFoundException("Cliente não encontrado.");
            }
        }

        /// <summary>
        /// Obtém e valida o anúncio antes de adicionar ou remover dos favoritos.
        /// </summary>
        /// <param name="anuncioId">Identificador do anúncio.</param>
        /// <param name="clienteId">Identificador do cliente.</param>
        /// <returns>Objeto <see cref="Anuncio"/> válido e ativo.</returns>
        /// <exception cref="NotFoundException">Lançada se o anúncio não existir.</exception>
        /// <exception cref="BusinessException">Lançada se o cliente for o vendedor do anúncio ou se o anúncio não estiver ativo.</exception>
        private async Task<Anuncio> ObterAnuncioValidoAsync(long anuncioId, long clienteId)
        {
            var anuncio = await _anuncioRepo.ObterPorIdAsync(anuncioId);

            if (anuncio == null)
            {
                throw new NotFoundException("Anúncio não encontrado.");
            }

            if (anuncio.VendedorId == clienteId)
            {
                throw new BusinessException("Não podes dar favorito ao teu próprio anúncio.");
            }

            if (anuncio.EstadoAnuncio != EstadoAnuncio.ATIVO)
            {
                throw new BusinessException("O anúncio tem de estar disponível.");
            }

            return anuncio;
        }

        /// <summary>
        /// Garante que o cliente não ultrapassou o limite máximo de 10 favoritos.
        /// </summary>
        /// <param name="clienteId">Identificador do cliente.</param>
        /// <exception cref="ValidationException">Lançada se o cliente já tiver 10 favoritos.</exception>
        private async Task ValidarLimiteFavoritosAsync(long clienteId)
        {
            var totalFavoritos = await _favoritoRepo.ContarPorClienteAsync(clienteId);

            if (totalFavoritos >= 10)
            {
                throw new ValidationException("Já atingiste o limite máximo de 10 favoritos.");
            }
        }

        /// <summary>
        /// Converte a lista de entidades de anúncios e favoritos em DTOs.
        /// </summary>
        /// <param name="anuncios">Lista de anúncios ativos com livro e vendedor.</param>
        /// <param name="favoritos">Lista de favoritos do cliente.</param>
        /// <returns>Lista de <see cref="AnuncioFavoritoDTO"/> com os dados do anúncio favorito.</returns>
        private List<AnuncioFavoritoDTO> MapearAnunciosFavoritos(List<Anuncio> anuncios, List<Favorito> favoritos)
        {
            return anuncios
                .Where(a => favoritos.Any(f => f.AnuncioId == a.Id))
                .Select(a => new AnuncioFavoritoDTO
                {
                    Id = a.Id,
                    Titulo = a.Livro?.Titulo ?? "Título não disponível",
                    Imagem = string.IsNullOrEmpty(a.Imagens)
                        ? null
                        : a.Imagens.Split(';').FirstOrDefault(),
                    Preco = a.TipoAnuncio == TipoAnuncio.DOACAO ? 0 : a.Preco,
                    EstadoLivro = a.EstadoLivro,
                    TipoAnuncio = a.TipoAnuncio,
                    TotalFavoritos = _favoritoRepo.ContarPorAnuncioAsync(a.Id).Result,
                    Favorito = true
                })
                .ToList();
        }

    }
}
