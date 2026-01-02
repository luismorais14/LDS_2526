using BookFlaz.Application.DTOs;
using BookFlaz.Application.Exceptions;
using BookFlaz.Application.Interfaces;
using BookFlaz.Domain.Entities;
using BookFlaz.Domain.Enums;
using BookFlaz.Domain.Exceptions;
using BookFlaz.Domain.Repositories;
using FuzzySharp;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory;

namespace BookFlaz.Application.Services
{
    /// <summary>
    /// Serviço responsável pela gestão de anúncios, incluindo criação, edição, remoção, e pesquisa.
    /// </summary>
    public class AnuncioService : IAnuncioService
    {
        private readonly IAnuncioRepository _anuncioRepo;
        private readonly ILivroRepository _livroRepo;
        private readonly ICategoriaRepository _categoriaRepo;
        private readonly IClienteRepository _clienteRepo;
        private readonly IPedidoTransacaoRepository _pedidoRepo;
        private readonly ITransacaoRepository _transacaoRepo;
        private readonly IUploadService _uploadService;
        private readonly IBookInfoService _livroService;
        private readonly IImagemService _imagemService;
        private readonly INotificacaoRepository _notiRepo;
        private readonly INotificacaoService _notiService;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public AnuncioService(
           IAnuncioRepository anuncioRepo,
           ILivroRepository livroRepo,
           ICategoriaRepository categoriaRepo,
           IClienteRepository clienteRepo,
           IPedidoTransacaoRepository pedidoRepo,
           ITransacaoRepository transacaoRepo,
           IUploadService uploadService,
           IBookInfoService livroService,
           IImagemService imagemService,
           INotificacaoRepository notiRepo,
           INotificacaoService notiService,
           IHttpContextAccessor httpContextAccessor)
        {
            _anuncioRepo = anuncioRepo;
            _livroRepo = livroRepo;
            _categoriaRepo = categoriaRepo;
            _clienteRepo = clienteRepo;
            _pedidoRepo = pedidoRepo;
            _transacaoRepo = transacaoRepo;
            _uploadService = uploadService;
            _livroService = livroService;
            _imagemService = imagemService;
            _notiRepo = notiRepo;
            _notiService = notiService;
            _httpContextAccessor = httpContextAccessor;
        }

        /// <summary>
        /// Cria um novo anúncio no sistema.
        /// </summary>
        /// <param name="dto">Dados do anúncio fornecidos pelo utilizador.</param>
        /// <param name="vendedorId">ID do utilizador autenticado que está a criar o anúncio.</param>
        /// <exception cref="NotFoundException">Lançado quando livro, categoria ou cliente não existem.</exception>
        /// <exception cref="BusinessException">Lançado quando o preço é inválido ou imagens violam regras (mínimo 1, máximo 5).</exception>
        /// <exception cref="ApplicationException">Lançado para erros inesperados durante o processo.</exception>
        public async Task CriarAnuncioAsync(CriarAnuncioDTO dto, long vendedorId)
        {
            try
            {
                await VerificarInexistentesAsync(dto.LivroIsbn, dto.CategoriaId, vendedorId);

                if (dto.TipoAnuncio != TipoAnuncio.DOACAO && dto.Preco == null)
                {
                    throw new BusinessException("O preço é obrigatório para este tipo de anúncio.");
                }

                var nomesImagens = await VerificarIntegriadeImagensAsync(dto.Imagens);

                var finalImages = string.Join(";", nomesImagens);

                var anuncio = Anuncio.CriarAnuncio(
                    dto.Preco,
                    dto.LivroIsbn,
                    dto.CategoriaId,
                    vendedorId,
                    dto.EstadoLivro,
                    dto.TipoAnuncio,
                    finalImages
                );

                await _anuncioRepo.AdicionarAsync(anuncio);
            }
            catch (NotFoundException)
            {
                throw; 
            }
            catch (DomainException ex)
            {
                throw new BusinessException(ex.Message);
            }
            catch (BusinessException)
            {
                throw; 
            }
            catch (Exception ex)
            {
                throw new ApplicationException("Erro inesperado ao criar o anúncio.", ex);
            }
        }

        /// <summary>
        /// Remove um anúncio existente.
        /// </summary>
        /// <param name="id">ID do anúncio a ser removido.</param>
        /// <param name="idUser">ID do utilizador autenticado.</param>
        /// <param name="motivo">Motivo da remoção (obrigatório para administradores).</param>
        /// <exception cref="NotFoundException">Lançado se o anúncio não existir.</exception>
        /// <exception cref="BusinessException"> Lançado quando o utilizador não tem permissão ou existem transações ativas.</exception>
        /// <exception cref="ApplicationException">Erro inesperado na operação.</exception>
        public async Task RemoverAnuncioAsync(long id, long idUser, string? motivo)
        {
            try
            {
                var anuncio = await _anuncioRepo.ObterPorIdAsync(id);

                if (anuncio == null)
                {
                    throw new NotFoundException($"O anúncio com ID {id} não foi encontrado.");
                }

                var utilizador = await _clienteRepo.ObterPorIdAsync(idUser);

                if (anuncio.VendedorId != idUser && !utilizador.IsAdmin)
                {
                    throw new BusinessException("Não tens permissões para apagar este anúncio!");
                }

                await VerificarTransacoesAtivas(id);

                await ApagarImagensAssociadas(anuncio.Imagens);

                if (utilizador.IsAdmin)
                {
                    if (motivo == null)
                    {
                        motivo = "O seu anúncio foi eliminado pelo Admin!";
                    }

                     await _notiService.CriarNotificacaoAsync(motivo, TipoNotificacao.SISTEMA, anuncio.VendedorId);
                }

                await _anuncioRepo.RemoverAsync(anuncio);
                
            }
            catch (NotFoundException)
            {
                throw;
            }
            catch (BusinessException)
            {
                throw;
            }
            catch (Exception ex)
            {
                throw new ApplicationException("Erro inesperado ao remover anúncio.", ex);
            }
        }

        /// <summary>
        /// Atualiza um anúncio existente com novos dados e imagens.
        /// </summary>
        /// <param name="id">ID do anúncio a ser atualizado.</param>
        /// <param name="dto">Dados atualizados enviados pelo utilizador.</param>
        /// <param name="idUser">ID do utilizador autenticado.</param>
        /// <exception cref="NotFoundException">Lançado se o anúncio, categoria ou livro não existir.</exception>
        /// <exception cref="BusinessException"> Lançado quando o utilizador não é o dono, tenta remover todas imagens ou excede limite.</exception>
        /// <exception cref="ApplicationException">Erro inesperado ao atualizar.</exception>
        public async Task EditarAnuncioAsync(long id, AtualizarAnuncioDTO dto, long idUser)
        {
            try
            {
                var anuncio = await _anuncioRepo.ObterPorIdAsync(id);

                if (anuncio == null)
                {
                    throw new NotFoundException($"O anúncio com ID {id} não existe.");
                }

                if (anuncio.VendedorId != idUser)
                {
                    throw new BusinessException("Apenas o vendedor pode modificar o seu anúncio");
                }

                await VerificarInexistentesAsync(dto.LivroIsbn, dto.CategoriaId);

                var imagensAtuais = string.IsNullOrEmpty(anuncio.Imagens)
                      ? new List<string>()
                      : anuncio.Imagens.Split(';', StringSplitOptions.RemoveEmptyEntries).ToList();

                if (dto.ImagensRemover != null && dto.ImagensRemover.Any())
                {
                    foreach (var img in dto.ImagensRemover)
                    {
                        if (imagensAtuais.Contains(img))
                        {
                            imagensAtuais.Remove(img);
                            await _uploadService.DeleteImgAsync(img);
                        }
                    }
                }

                if (dto.NovasImagens != null && dto.NovasImagens.Any())
                {
                    var novasImagens = await VerificarIntegriadeImagensAsync(dto.NovasImagens);
                    imagensAtuais.AddRange(novasImagens);
                }

                if (!imagensAtuais.Any())
                {
                    throw new BusinessException("O anúncio deve conter pelo menos uma imagem.");
                }

                if (imagensAtuais.Count > 5)
                {
                    throw new BusinessException("O anúncio não pode ter mais de 5 imagens.");
                }

                var finalImages = string.Join(";", imagensAtuais);

                anuncio.AtualizarAnuncio(
                    dto.Preco,
                    dto.LivroIsbn,
                    dto.CategoriaId,
                    dto.EstadoLivro,
                    dto.TipoAnuncio,
                    finalImages
                );

                await _anuncioRepo.Atualizar(anuncio);

            }
            catch (NotFoundException)
            {
                throw; 
            }
            catch (DomainException ex)
            {
                throw new BusinessException(ex.Message);
            }
            catch (BusinessException)
            {
                throw; 
            }
            catch (Exception ex)
            {
                throw new ApplicationException("Erro inesperado ao atualizar o anúncio.", ex);
            }
        }

        /// <summary>
        /// Realiza a pesquisa de anúncios ativos com base nos filtros fornecidos,
        /// aplicando critérios como categoria, faixa de preço, tipo de anúncio,
        /// estado do livro e termo de pesquisa textual.
        /// </summary>
        /// <param name="filtro">
        /// Objeto <see cref="FiltroAnuncioDTO"/> contendo os parâmetros de filtragem e pesquisa:
        /// categoria, preço mínimo/máximo, tipo de anúncio, estado do livro e termo textual.
        /// </param>
        /// <returns>
        /// Uma lista de <see cref="AnuncioFavoritoDTO"/> representando os anúncios encontrados
        /// que correspondem aos filtros aplicados, ordenados por relevância e data de criação.
        /// </returns>
        /// <exception cref="NotFoundException">Lançada quando não existem anúncios disponíveis ou quando nenhum anúncio corresponde aos filtros aplicados.</exception>
        /// <exception cref="BusinessException">Lançada quando o intervalo de preços é inválido (preço mínimo superior ao máximo).</exception>
        /// <exception cref="ApplicationException"> Lançada em caso de erros inesperados durante o processamento da pesquisa.</exception>
        public async Task<List<AnuncioFavoritoDTO>> PesquisarAnunciosAsync(FiltroAnuncioDTO filtro)
        {
            try
            {
                var anuncios = (await _anuncioRepo.ObterAtivosComLivroEVendedorAsync()) ?? new List<Anuncio>();

                if (!anuncios.Any())
                    throw new NotFoundException("Ainda não existem anúncios");

                filtro ??= new FiltroAnuncioDTO();

                var query = FiltrarAnuncios(anuncios, filtro) ?? Enumerable.Empty<Anuncio>();

                await ValidarIntervaloPrecoAsync(filtro);

                var resultadosFiltrados = AplicarPesquisaTexto(query, filtro.TermoPesquisa) ?? Enumerable.Empty<Anuncio>();

                var resultadosSeguros = resultadosFiltrados.Where(a => a != null && a.Livro != null).ToList();

                var resultados = await MapearParaDTOAsync(resultadosSeguros);

                if (resultados == null || !resultados.Any())
                    throw new NotFoundException("Nenhum anúncio encontrado para este filtro");

                return resultados;
            }
            catch (BusinessException)
            {
                throw;
            }
            catch (NotFoundException)
            {
                throw;
            }
            catch (Exception ex)
            {
                // preserve original exception for logs / caller
                throw new ApplicationException("Erro inesperado ao aplicar filtros de pesquisa.", ex);
            }
        }

        public async Task<VisualizarAnuncioDTO> VisualizarAnuncio(long idAnuncio)
        {
            var anuncio = await _anuncioRepo.ObterPorIdAsync(idAnuncio);

            if (anuncio == null)
            {
                throw new NotFoundException("o anúncio não existe");
            }

            var cliente = await _clienteRepo.ObterPorIdAsync(anuncio.VendedorId);

            if (cliente == null)
            {
                throw new NotFoundException("cliente não foi encontrado");
            }

            var TotalFavoritos = await _anuncioRepo.ContarFavoritosAsync(anuncio.Id);

            var categoria = await _categoriaRepo.ObterPorIdAsync(anuncio.CategoriaId);

            if (categoria == null)
            {
                throw new NotFoundException("a categoria não existe");
            }

            var livro = await _livroRepo.ObterPorIsbnAsync(anuncio.LivroIsbn);

            if (livro == null)
            {
                throw new NotFoundException("O livro não existe");
            }

            var anuncioDTO = new VisualizarAnuncioDTO
            {
                Id = anuncio.Id,
                Preco = anuncio.Preco,
                EstadoLivro = anuncio.EstadoLivro,
                TipoAnuncio = anuncio.TipoAnuncio,
                EstadoAnuncio = anuncio.EstadoAnuncio,
                Imagens = anuncio.Imagens,
                DataCriacao = anuncio.DataCriacao,
                NomeVendedor = cliente.Nome,
                Categoria = categoria.Nome,
                TotalFavoritos = TotalFavoritos,
                Titulo = livro.Titulo,
                Autor = livro.Autor
            };

            return anuncioDTO;
        }

        /// <summary>
        /// Valida a existência de livro, categoria e cliente (opcional) antes de operações.
        /// </summary>
        /// <exception cref="NotFoundException">Entidades obrigatórias não existentes.</exception>
        private async Task VerificarInexistentesAsync(long isbn, long idCategoria, long? idCliente = null)
        {
            if (idCliente.HasValue)
            {
                if (!await _clienteRepo.ExisteAsync(idCliente.Value))
                {
                    throw new NotFoundException($"O cliente {idCliente.Value} não existe.");
                }
            }

            var livroExiste = await VerificarLivroExistenteAsync(isbn);

            if (!livroExiste) {
                throw new NotFoundException("O livro especificado não existe.");
            }

            if (!await _categoriaRepo.ExisteAsync(idCategoria)) {
                throw new NotFoundException("A categoria especificada não existe.");
            }
        }

        /// <summary>
        /// Verifica se o livro existe localmente ou tenta buscá-lo via API externa e salva.
        /// </summary>
        /// <returns>True se existir ou for adicionado.</returns>
        private async Task<bool> VerificarLivroExistenteAsync(long isbn)
        {
            var livroExiste = await _livroRepo.ExisteAsync(isbn);

            if (livroExiste)
            {
                return true;
            }

            var livroDto = await _livroService.ObterLivroPorISBNAsync(isbn.ToString());

            if (livroDto == null)
            {
                return false;
            }

            var novoLivro = Livro.AdicionarLivro(livroDto.Isbn, livroDto.Titulo, livroDto.Autor);

            await _livroRepo.AdicionarAsync(novoLivro);
            return true;
        }

        /// <summary>
        /// Valida, processa e faz upload das imagens do anúncio.
        /// </summary>
        /// <exception cref="BusinessException">Lançado se o ficheiro não contiver um livro, exceder limite ou falhar upload. </exception>
        private async Task<List<string>> VerificarIntegriadeImagensAsync(List<IFormFile> imagens)
        {
            var nomesImagens = new List<string>();

            if (imagens == null || !imagens.Any()) {
                throw new BusinessException("O anúncio deve conter pelo menos uma imagem.");
            }

            if (imagens.Count > 5) {
                throw new BusinessException("O anúncio não pode ter mais de 5 imagens.");
            }

            try
            {
                foreach (var img in imagens)
                {
                    /*var realmenteLivro = await _imagemService.VerificarSeELivroAsync(img);

                    if (!realmenteLivro) {
                        throw new BusinessException($"A imagem '{img.FileName}' não parece conter um livro.");
                    }*/

                    try
                    {
                        var nome = await _uploadService.UploadAsync(img);

                        nomesImagens.Add(nome);
                    }

                    catch (InvalidOperationException ex)
                    {
                        throw new BusinessException($"Erro ao carregar imagem '{img.FileName}': {ex.Message}");
                    }
                }

                return nomesImagens;
            }
            catch (BusinessException)
            {
                throw;
            }
            catch (Exception ex)
            {
                throw new ApplicationException($"Erro ao processar imagens: {ex.Message}", ex);
            }

        }

        /// <summary>
        /// Verifica se existem transações ativas relacionadas ao anúncio antes da remoção.
        /// </summary>
        /// <exception cref="BusinessException">Se existir transação ativa.</exception>
        private async Task VerificarTransacoesAtivas(long anuncioId)
        {
            var pedidosTransacao = await _pedidoRepo.ObterPorAnuncioIdAsync(anuncioId);

            if (pedidosTransacao.Any())
            {
                var idsPedidos = pedidosTransacao.Select(p => p.Id).ToList();

                var transacaoAtiva = await _transacaoRepo.ExisteTransacaoAtivaAsync(idsPedidos);

                if (transacaoAtiva)
                {
                    throw new BusinessException("Não podes apagar um anúncio com transações ativas");
                }

                await _pedidoRepo.RemoverRangeAsync(pedidosTransacao);
            }
        }

        /// <summary>
        /// Remove fisicamente imagens armazenadas associadas ao anúncio.
        /// </summary>
        private async Task ApagarImagensAssociadas(string imagensAnuncio)
        {
            if (!string.IsNullOrEmpty(imagensAnuncio))
            {
                var imagens = imagensAnuncio.Split(';', StringSplitOptions.RemoveEmptyEntries);

                foreach (var imagem in imagens)
                {
                    try 
                    { 
                        await _uploadService.DeleteImgAsync(imagem); 
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"Erro ao eliminar imagem '{imagem}': {ex.Message}");
                    }
                }
            }
        }

        /// <summary>
        /// Aplica os filtros básicos (categoria, preço, tipo de anúncio e estado do livro)
        /// sobre a lista de anúncios ativos, retornando uma coleção filtrada.
        /// </summary>
        /// <param name="anuncios">Coleção de anúncios obtidos do repositório.</param>
        /// <param name="filtro">Objeto contendo os critérios de filtragem definidos pelo utilizador.</param>
        /// <returns>
        /// Um <see cref="IEnumerable{Anuncio}"/> contendo apenas os anúncios que cumprem
        /// os critérios especificados no filtro.
        /// </returns>
        private IEnumerable<Anuncio> FiltrarAnuncios(IEnumerable<Anuncio> anuncios, FiltroAnuncioDTO filtro)
        {
            var query = anuncios.Where(a => a.EstadoAnuncio == EstadoAnuncio.ATIVO);

            if (filtro.CategoriaId.HasValue)
            {
                query = query.Where(a => a.CategoriaId == filtro.CategoriaId);
            }

            if (filtro.PrecoMinimo.HasValue)
            {
                query = query.Where(a => a.Preco >= filtro.PrecoMinimo.Value);
            }

            if (filtro.PrecoMaximo.HasValue)
            {
                query = query.Where(a => a.Preco <= filtro.PrecoMaximo.Value);
            }

            if (filtro.TipoAnuncio.HasValue)
            {
                query = query.Where(a => a.TipoAnuncio == filtro.TipoAnuncio.Value);
            }

            if (filtro.EstadoLivro.HasValue)
            {
                query = query.Where(a => a.EstadoLivro == filtro.EstadoLivro.Value);
            }

            return query;
        }

        /// <summary>
        /// Valida a coerência entre o preço mínimo e máximo definidos no filtro.
        /// Lança uma <see cref="BusinessException"/> se o valor mínimo for superior ao máximo.
        /// </summary>
        /// <param name="filtro">Objeto contendo os limites de preço a validar.</param>
        /// <returns>Uma tarefa completada, caso a validação seja bem-sucedida.</returns>
        /// <exception cref="BusinessException">
        /// Lançada quando o preço mínimo é superior ao preço máximo.
        /// </exception>
        private Task ValidarIntervaloPrecoAsync(FiltroAnuncioDTO filtro)
        {
            if (filtro.PrecoMinimo.HasValue && filtro.PrecoMaximo.HasValue)
            {
                if (filtro.PrecoMinimo > filtro.PrecoMaximo)
                {
                    throw new BusinessException("O valor mínimo não pode ser maior que o valor máximo!");
                }
            }

            return Task.CompletedTask;
        }

        /// <summary>
        /// Aplica a pesquisa textual (título, autor ou ISBN) sobre a coleção de anúncios filtrados.
        /// Utiliza fuzzy matching para identificar resultados relevantes mesmo com pequenas variações no texto.
        /// </summary>
        /// <param name="query">Coleção base com os anúncios filtrados por critérios objetivos.</param>
        /// <param name="termoPesquisa">Texto de pesquisa introduzido pelo utilizador.</param>
        /// <returns>
        /// Uma coleção enumerável de <see cref="Anuncio"/> ordenada por relevância e data de criação.
        /// </returns>
        private IEnumerable<Anuncio> AplicarPesquisaTexto(IEnumerable<Anuncio> query, string? termoPesquisa)
        {
            if (string.IsNullOrWhiteSpace(termoPesquisa))
            {
                return query;
            }

            var termo = termoPesquisa.Trim();

            return query
                .Select(a =>
                {
                    int scoreTitulo = Fuzz.PartialRatio(termo, a.Livro.Titulo);
                    int scoreAutor = Fuzz.PartialRatio(termo, a.Livro.Autor);
                    int scoreIsbn = Fuzz.PartialRatio(termo, a.Livro.Isbn.ToString());
                    int relevancia = Math.Max(scoreTitulo, Math.Max(scoreAutor, scoreIsbn));
                    return new { Anuncio = a, Relevancia = relevancia };
                })
                .Where(x => x.Relevancia >= 70)
                .OrderByDescending(x => x.Relevancia)
                .ThenByDescending(x => x.Anuncio.DataCriacao)
                .Select(x => x.Anuncio);
        }

        /// <summary>
        /// Converte uma coleção de entidades <see cref="Anuncio"/> em objetos de transferência de dados (<see cref="AnuncioFavoritoDTO"/>),
        /// incluindo o cálculo do número total de favoritos de cada anúncio.
        /// </summary>
        /// <param name="anuncios">Coleção de anúncios a serem mapeados para DTOs.</param>
        /// <returns>
        /// Uma lista de <see cref="AnuncioFavoritoDTO"/> representando os anúncios prontos
        /// para envio à camada de apresentação.
        /// </returns>
        private async Task<List<AnuncioFavoritoDTO>> MapearParaDTOAsync(IEnumerable<Anuncio> anuncios)
        {
            var resultado = new List<AnuncioFavoritoDTO>();
            var baseUrl = ObterBaseUploadsUrl(); 

            foreach (var a in anuncios)
            {
                var firstImageName = string.IsNullOrEmpty(a.Imagens)
                    ? null
                    : a.Imagens.Split(';', StringSplitOptions.RemoveEmptyEntries).FirstOrDefault();

                var dto = new AnuncioFavoritoDTO
                {
                    Id = a.Id,
                    Titulo = a.Livro.Titulo,
                    Imagem = firstImageName != null ? $"{baseUrl}{firstImageName}" : null,
                    Preco = a.TipoAnuncio == TipoAnuncio.DOACAO ? 0 : a.Preco,
                    Categoria = a.Categoria.Nome,
                    EstadoLivro = a.EstadoLivro,
                    TipoAnuncio = a.TipoAnuncio,
                    TotalFavoritos = await _anuncioRepo.ContarFavoritosAsync(a.Id)
                };

                resultado.Add(dto);
            }

            return resultado;
        }

        /// <summary>
        /// Obtém URL base para construção de links de imagens.
        /// </summary>
        private string ObterBaseUploadsUrl()
        {
            var request = _httpContextAccessor.HttpContext?.Request;

            if (request != null && request.Host.HasValue)
            {
                return $"{request.Scheme}://{request.Host}/uploads/";
            }

            return "https://localhost:7002/uploads/";
        }
    }
}

