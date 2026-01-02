using BookFlaz.Application.DTOs;
using BookFlaz.Application.Exceptions;
using BookFlaz.Application.Interfaces;
using BookFlaz.Application.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace BookFlaz.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class FavoritoController : ControllerBase
    {
        private readonly IFavoritoService _favoritoService;

        public FavoritoController(IFavoritoService favoritoService)
        {
            _favoritoService = favoritoService;
        }

        /// <summary>
        /// Adiciona ou remove um anúncio da lista de favoritos do utilizador autenticado.
        /// </summary>
        /// <param name="idAnuncio">ID do anúncio a favoritar ou remover dos favoritos.</param>
        /// <response code="200">Favorito adicionado ou removido com sucesso.</response>
        /// <response code="401">Token inválido ou usuário não autenticado.</response>
        /// <response code="404">O anúncio ou usuário não foi encontrado.</response>
        /// <response code="409">Regra de negócio violada.</response>
        /// <response code="500">Erro inesperado no servidor.</response>
        [Authorize]
        [HttpPost]
        public async Task<IActionResult> AlternarFavoritoAsync(long idAnuncio)
        {
            try
            {
                var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);

                if (userIdClaim == null)
                {
                    return Unauthorized("Token inválido, ID do utilizador não encontrado.");
                }

                long idUser = long.Parse(userIdClaim.Value);

                await _favoritoService.AlternarFavoritoAsync(idUser, idAnuncio);

                return Ok(new
                {
                    sucesso = true,
                    mensagem = "Operação realizada com sucesso (adicionado ou removido)."
                });
            }
            catch (NotFoundException ex)
            {
                return NotFound(new { sucesso = false, mensagem = ex.Message});
            }
            catch (ValidationException ex)
            {
                return BadRequest(new { sucesso = false, mensagem = ex.Message });
            }
            catch (BusinessException ex)
            {
                return Conflict(new {sucesso = false, mensagem = ex.Message});
            }
            catch (ApplicationException ex)
            {
                return StatusCode(500, new { sucesso = false, mensagem = ex.Message});
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { sucesso = false, mensagem = "Ocorreu um erro inesperado. Tente novamente mais tarde.", detalhe = ex.Message});
            }
        }

        /// <summary>
        /// Obtém todos os anúncios favoritos do utilizador autenticado.
        /// </summary>
        /// <returns>Lista de anúncios favoritos do usuário.</returns>
        /// <response code="200">Lista retornada com sucesso (pode estar vazia).</response>
        /// <response code="401">Usuário não autenticado.</response>
        /// <response code="500">Erro inesperado no servidor.</response>
        [Authorize]
        [HttpGet]
        public async Task<IActionResult> ListarFavoritos()
        {
            try
            {
                var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);

                if (userIdClaim == null)
                {
                    return Unauthorized("Token inválido, ID do utilizador não encontrado.");
                }

                long idUser = long.Parse(userIdClaim.Value);

                var favoritos = await _favoritoService.ObterAnunciosFavoritosAsync(idUser);

                if (favoritos == null || !favoritos.Any())
                {
                    return Ok(new
                    {
                        sucesso = true,
                        mensagem = "Não tens favoritos neste momento."
                    });
                }

                return Ok(favoritos);
            }
            catch (NotFoundException ex)
            {
                return NotFound(new { sucesso = false, mensagem = ex.Message });
            }
            catch (ApplicationException ex)
            {
                return StatusCode(500, new {  sucesso = false, mensagem = ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new  {sucesso = false, mensagem = "Ocorreu um erro inesperado. Tente novamente mais tarde.", detalhe = ex.Message });
            }
        }
    }
}