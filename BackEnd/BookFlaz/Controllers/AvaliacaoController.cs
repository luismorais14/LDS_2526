using System.Security.Claims;
using BookFlaz.Application.DTOs;
using BookFlaz.Application.Exceptions;
using BookFlaz.Application.Interfaces;
using BookFlaz.Domain.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BookFlaz.API.Controllers
{

    [ApiController]
    [Authorize]
    [Route("api/[controller]")]
    public class AvaliacaoController : ControllerBase
    {
        private readonly IAvaliacaoService avaliacaoService;

        public AvaliacaoController(IAvaliacaoService avaliacaoService)
        {
            this.avaliacaoService = avaliacaoService;
        }

        [HttpPost("transacoes/{transacaoId:long}/avaliacoes")]
        public async Task<IActionResult> CriarAvaliacao(
            [FromRoute] long transacaoId,
            [FromQuery] long avaliadoId,
            [FromBody] AvaliacaoDTO dto)
        {
            try
            { 
                var avaliadorId = long.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));

                if (avaliadorId == avaliadoId)
                {
                    return BadRequest("Utilizador não se pode avaliar a si prórpio.");
                }
                
                var avaliacao = await avaliacaoService.AvaliarAsync(avaliadorId, avaliadoId, transacaoId, dto);

                return Ok(avaliacao);
            }
            
            catch (NotFoundException ex)
            {
                return NotFound(ex.Message);
            }
            catch (BusinessException ex)
            {
                return BadRequest(ex.Message);
            }
        }
    }
}