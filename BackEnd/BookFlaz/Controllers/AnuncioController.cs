using BookFlaz.Application.DTOs;
using BookFlaz.Application.Exceptions;
using BookFlaz.Application.Interfaces;
using BookFlaz.Application.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace BookFlaz.API.Controllers
{
    /// <summary>
    /// Controlador responsável por expor os endpoints relacionados à gestão de anúncios na API BookFlaz.
    /// </summary>
    /// <remarks>
    /// Este controlador fornece operações para criar, atualizar e remover anúncios,
    /// delegando a lógica de negócio ao serviço <see cref="IAnuncioService"/>.
    /// Todos os endpoints seguem o padrão RESTful e retornam códigos HTTP adequados
    /// consoante o resultado das operações.
    /// </remarks>
    
    [Route("api/[controller]")]
    [ApiController]
    public class AnuncioController : ControllerBase
    {
        private readonly IAnuncioService anuncioService;

        /// <summary>
        /// Construtor que injeta a dependência do serviço responsável pela gestão de anúncios.
        /// </summary>
        /// <param name="anuncioService">
        /// Instância de <see cref="IAnuncioService"/> utilizada para processar a lógica de negócio.
        /// </param>
        public AnuncioController(IAnuncioService anuncioService)
        {
            this.anuncioService = anuncioService;
        }

        /// <summary>
        /// Cria um novo anúncio.
        /// </summary>
        /// <param name="dto">Dados para criação do anúncio enviados via formulário multipart.</param>
        /// <returns>Mensagem de sucesso ou erro detalhado.</returns>
        /// <remarks>
        /// - Apenas utilizadores autenticados podem aceder
        /// </remarks>
        /// <response code="200">Anúncio criado com sucesso.</response>
        /// <response code="400">Erro de validação dos dados ou regras de negócio.</response>
        /// <response code="401">Token inválido ou ausente.</response>
        /// <response code="404">Entidade associada inexistente (livro/categoria/utilizador).</response>
        /// <response code="500">Erro interno inesperado.</response>
        [Authorize]
        [HttpPost]
        public async Task<IActionResult> CriarAnuncio([FromForm] CriarAnuncioDTO dto)
        {
            try
            {
                var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);

                if (userIdClaim == null)
                {
                    return Unauthorized("Token inválido, ID do utilizador não encontrado.");
                }

                long idUser = long.Parse(userIdClaim.Value);

                await anuncioService.CriarAnuncioAsync(dto, idUser);

                return Ok(new { Message = "Anúncio criado com sucesso." });
            }
            catch (NotFoundException ex)
            {
                return NotFound(new { Error = ex.Message });
            }
            catch (BusinessException ex)
            {
                return BadRequest(new { Error = ex.Message });
            }
            catch (ApplicationException ex)
            {
                return StatusCode(500, new { Error = ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { Error = $"Erro inesperado: {ex.Message}" });
            }
        }

        /// <summary>
        /// Remove um anúncio existente.
        /// </summary>
        /// <param name="id">ID do anúncio.</param>
        /// <param name="motivo">Motivo da remoção (obrigatório para administradores).</param>
        /// <returns>Mensagem de sucesso ou erro.</returns>
        /// <response code="200">Anúncio removido com sucesso.</response>
        /// <response code="400">Não é possível remover o anúncio (regra de negócio).</response>
        /// <response code="401">Utilizador não autenticado.</response>
        /// <response code="404">Anúncio não encontrado.</response>
        /// <response code="500">Erro interno inesperado.</response>
        [Authorize]
        [HttpDelete("{id}")]
        public async Task<IActionResult> RemoverAnuncioAsync(long id, [FromQuery] string? motivo)
        {
            try
            {
                var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);

                if (userIdClaim == null)
                {
                    return Unauthorized("Token inválido, ID do utilizador não encontrado.");
                }

                long idUser = long.Parse(userIdClaim.Value);

                await anuncioService.RemoverAnuncioAsync(id, idUser, motivo);

                return Ok(new { Message = "Anúncio removido com sucesso." });
            }
            catch (NotFoundException ex)
            {
                return NotFound(new { message = ex.Message });
            }
            catch (BusinessException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
            catch (ApplicationException ex)
            {
                return StatusCode(500, new { message = ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Erro inesperado ao remover anúncio.", detail = ex.Message });
            }
        }

        /// <summary>
        /// Atualiza um anúncio existente.
        /// </summary>
        /// <param name="id">ID do anúncio.</param>
        /// <param name="dto">Dados do anúncio a atualizar.</param>
        /// <returns>Mensagem de sucesso ou erro.</returns>
        /// <response code="200">Anúncio atualizado com sucesso.</response>
        /// <response code="400">Dados inválidos ou violação de regra de negócio.</response>
        /// <response code="401">Token inválido.</response>
        /// <response code="404">Anúncio, categoria ou livro inexistente.</response>
        /// <response code="500">Erro inesperado.</response>
        [Authorize]
        [HttpPut("{id}")]
        public async Task<IActionResult> AtualizarAnuncioAsync(long id, [FromForm] AtualizarAnuncioDTO dto)
        {
            try
            {
                var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);

                if (userIdClaim == null)
                {
                    return Unauthorized("Token inválido, ID do utilizador não encontrado.");
                }

                long idUser = long.Parse(userIdClaim.Value);

                await anuncioService.EditarAnuncioAsync(id, dto, idUser);

                return Ok(new { Message = "Anúncio atualizado com sucesso." });
            }
            catch (NotFoundException ex)
            {
                return NotFound(new { Error = ex.Message });
            }
            catch (BusinessException ex)
            {
                return BadRequest(new { Error = ex.Message });
            }
            catch (ApplicationException ex)
            {
                return StatusCode(500, new { Error = ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { Error = $"Erro inesperado: {ex.Message}" });
            }
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> AtualizarAnuncioAsync([FromRoute] int id)
        {
            try
            {
                var anuncio = await anuncioService.VisualizarAnuncio(id);

                return Ok(anuncio);
            }
            catch (NotFoundException ex)
            {
                return NotFound(new { Error = ex.Message });
            }
        }

        /// <summary>
        /// Pesquisa anúncios com base em filtros opcionais.
        /// </summary>
        /// <param name="filtro">Filtros como categoria, preço, estado do livro e texto de busca.</param>
        /// <returns>Lista filtrada de anúncios com contagem total.</returns>
        /// <response code="200">Retorna resultados da pesquisa.</response>
        /// <response code="400">Filtro inválido.</response>
        /// <response code="404">Nenhum anúncio encontrado.</response>
        /// <response code="500">Erro inesperado.</response>
        [HttpGet]
        public async Task<IActionResult> PesquisarAnunciosAsync([FromQuery] FiltroAnuncioDTO filtro)
        {
            try
            {
                var resultados = await anuncioService.PesquisarAnunciosAsync(filtro);

                return Ok(new {sucesso = true, total = resultados.Count,anuncios = resultados});
            }
            catch (NotFoundException ex)
            {
                return NotFound(new { sucesso = false,mensagem = ex.Message});
            }
            catch (BusinessException ex)
            {
                return BadRequest(new { sucesso = false, mensagem = ex.Message });
            }
            catch (ApplicationException ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, new { sucesso = false, mensagem = ex.Message, detalhe = ex.InnerException?.Message});
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, new { sucesso = false, mensagem = "Erro inesperado ao aplicar os filtros de pesquisa.", detalhe = ex.Message});
            }
        }
    }
}
