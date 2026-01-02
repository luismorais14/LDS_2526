using BookFlaz.Application.Exceptions;
using BookFlaz.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace BookFlaz.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class PontosController : ControllerBase
    {
        private readonly IPontosService pontosService;

        public PontosController(IPontosService pontosService)
        {
            this.pontosService = pontosService;
        }

        /// <summary>
        /// Obtém o total de pontos do cliente autenticado.
        /// </summary>
        /// <remarks>
        /// Este endpoint requer autenticação via JWT Bearer Token.
        /// O ID do utilizador é extraído automaticamente do token.
        /// </remarks>
        /// <response code="200">Pontos obtidos com sucesso.</response>
        /// <response code="401">Token inválido ou não enviado.</response>
        /// <response code="404">Cliente não encontrado.</response>
        /// <response code="500">Erro interno ao obter os pontos.</response>
        [Authorize]
        [HttpGet]
        public async Task<IActionResult> VisualizarPontos()
        {
            try
            {
                var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);

                if (userIdClaim == null)
                {
                    return Unauthorized("Token inválido, ID do utilizador não encontrado.");
                }

                long idUser = long.Parse(userIdClaim.Value);

                var pontos =  await pontosService.ObterPontosAsync(idUser);

                return Ok(new { Pontos = pontos });

            }
            catch (NotFoundException ex)
            {
                return NotFound(new { Error = ex.Message });
            }
            catch (Exception ex)
            {
                throw new ApplicationException("Erro ao obter pontos do cliente.", ex);
            }
        }


        /// <summary>
        /// Obtém o histórico de movimentações de pontos do cliente autenticado.
        /// </summary>
        /// <remarks>
        /// Este endpoint retorna todas as operações de ganho e uso de pontos,
        /// ordenadas por data descendente (mais recentes primeiro).
        /// </remarks>
        /// <returns>Lista com o histórico de pontos e detalhes associados às transações.</returns>
        /// <response code="200">Histórico retornado com sucesso.</response>
        /// <response code="401">Token ausente ou inválido.</response>
        /// <response code="404">Nenhum movimento de pontos encontrado ou cliente inexistente.</response>
        /// <response code="500">Erro interno ao obter o histórico.</response>
        [Authorize]
        [HttpGet("Historico")]
        public async Task<IActionResult> ObterHistorico()
        {
            try
            {
                var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);

                if (userIdClaim == null)
                {
                    return Unauthorized("Token inválido, ID do utilizador não encontrado.");
                }

                long idUser = long.Parse(userIdClaim.Value);

                var historico = await pontosService.ObterHistoricoAsync(idUser);

                return Ok(historico);
            }
            catch (NotFoundException ex)
            {
                return NotFound(new { Error = ex.Message });
            }
            catch (Exception ex)
            {
                throw new ApplicationException("Erro ao obter histórico de pontos.", ex);
            }
        }
    }
}
