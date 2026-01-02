using BookFlaz.Application.Exceptions;
using BookFlaz.Application.Interfaces;
using BookFlaz.Domain.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace BookFlaz.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class NotificacaoController : ControllerBase
    {
        private readonly INotificacaoService _notiService;
        private readonly IClienteService _clienteService;

        public NotificacaoController(INotificacaoService notiService, IClienteService clienteService)
        {
            _notiService = notiService;
            _clienteService = clienteService;
        }

        private bool TryGetUserId(out long userId)
        {
            var idStr = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            return long.TryParse(idStr, out userId);
        }

        /// <summary>
        /// Lista as notificações do utilizador autenticado.
        /// Use ?respeitarPreferencia=true para retornar vazio/204 quando o utilizador tiver a preferência desligada.
        /// </summary>
        /// <param name="respeitarPreferencia">Se true, respeita a preferência guardada no perfil (pode devolver 204).</param>
        [HttpGet]
        [ProducesResponseType(typeof(IEnumerable<Notificacao>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> ObterNotificacoes([FromQuery] bool respeitarPreferencia = false)
        {
            if (!TryGetUserId(out var userId))
                return Unauthorized("Token inválido, ID do utilizador não encontrado.");

            try
            {
                var notificacoes = await _notiService.ObterNotificacoesAsync(userId);
                return Ok(notificacoes);
            }
            catch (ApplicationException ex) 
            { 
                return StatusCode(500, ex.Message); 
            }
        }

        /// <summary>
        /// Obtém a preferência atual do utilizador (se quer receber notificações no front).
        /// </summary>
        [HttpGet("preferencias")]
        [ProducesResponseType(typeof(object), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> ObterPreferencia()
        {
            if (!TryGetUserId(out var userId))
                return Unauthorized("Token inválido, ID do utilizador não encontrado.");

            try
            {
                var recebe = await _clienteService.RecebeNotificacaoAsync(userId);
                return Ok(recebe);
            }
            catch (NotFoundException ex) 
            { 
                return NotFound(ex.Message); 
            }
            catch (ApplicationException ex) 
            { 
                return StatusCode(500, ex.Message); 
            }
        }

        /// <summary>
        /// Altera a preferência do utilizador (se quer receber notificações no front).
        /// </summary>
        /// <param name="preferencia">true para receber; false para silenciar no front.</param>
        [HttpPut("preferencias")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> AlterarPreferencia([FromQuery] bool preferencia)
        {
            if (!TryGetUserId(out var userId))
                return Unauthorized("Token inválido, ID do utilizador não encontrado.");

            try
            {
                var ok = await _clienteService.AlterarPreferenciaNotificacaoAsync(userId, preferencia);
                if (!ok) return BadRequest("Não foi possível alterar a preferência.");
                return NoContent();
            }
            catch (NotFoundException ex) 
            { 
                return NotFound(ex.Message); 
            }
            catch (ApplicationException ex) 
            { 
                return StatusCode(500, ex.Message); 
            }
        }
    }
}
