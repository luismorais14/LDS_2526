using BookFlaz.Application.DTOs;
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
    public class PedidoTransacaoController : ControllerBase
    {
        private readonly IPedidoTransacaoService _pedidoService;

        public PedidoTransacaoController(IPedidoTransacaoService pedidoService)
        {
            _pedidoService = pedidoService;
        }

        /// <summary>
        /// Cria um novo pedido de transação.
        /// </summary>
        /// <remarks>
        /// Requer autenticação via JWT Bearer Token.
        /// O ID do utilizador é extraído automaticamente do token.
        /// O corpo da requisição deve conter os dados de <see cref="CriarPedidoTransacaoDTO"/>.
        /// </remarks>
        /// <param name="dto">Dados necessários para criação do pedido.</param>
        /// <response code="200">Pedido criado com sucesso e associado à conversa.</response>
        /// <response code="400">Dados inválidos, erro de validação ou regra de negócio.</response>
        /// <response code="401">Token ausente ou inválido.</response>
        /// <response code="403">Utilizador sem permissões para criar o pedido.</response>
        /// <response code="404">Recurso associado não encontrado (ex.: anúncio/conversa).</response>
        /// <response code="500">Erro interno ao criar o pedido.</response>
        [Authorize]
        [HttpPost]
        public async Task<IActionResult> CriarPedidoAsync([FromBody] CriarPedidoTransacaoDTO dto)
        {
            try
            {
                var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);

                if (userIdClaim == null)
                {
                    return Unauthorized("Token inválido, ID do utilizador não encontrado.");
                }

                long idUser = long.Parse(userIdClaim.Value);

                var (pedidoId, conversaId) = await _pedidoService.CriarPedidoAsync(dto, idUser);

                return Ok(new
                {
                    sucesso = true,
                    mensagem = "Pedido criado com sucesso e associado à conversa.",
                    pedidoId,
                    conversaId
                });
            }
            catch (ValidationException ex)
            {
                return BadRequest(new { sucesso = false, mensagem = ex.Message });
            }
            catch (BusinessException ex)
            {
                return BadRequest(new { sucesso = false, mensagem = ex.Message });
            }
            catch (UnauthorizedActionException ex)
            {
                return StatusCode(403, new { sucesso = false, mensagem = ex.Message });
            }
            catch (NotFoundException ex)
            {
                return NotFound(new { sucesso = false, mensagem = ex.Message });
            }
            catch (ApplicationException ex)
            {
                return StatusCode(500, new { sucesso = false, mensagem = ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { sucesso = false, mensagem = "Erro inesperado ao criar o pedido." });
            }
        }

        /// <summary>
        /// Aceita um pedido de transação pendente.
        /// </summary>
        /// <remarks>
        /// Requer autenticação via JWT Bearer Token.
        /// O utilizador autenticado deve ter permissões para aceitar o pedido.
        /// </remarks>
        /// <param name="id">Identificador do pedido de transação a aceitar.</param>
        /// <response code="200">Pedido aceite com sucesso.</response>
        /// <response code="400">Regra de negócio impedindo a aceitação.</response>
        /// <response code="401">Token ausente ou inválido.</response>
        /// <response code="404">Pedido de transação não encontrado.</response>
        /// <response code="500">Erro interno ao aceitar o pedido.</response>
        [Authorize]
        [HttpPut("{id}/aceitar")]
        public async Task<IActionResult> AceitarPedidoAsync(long id)
        {
            try
            {
                var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);

                if (userIdClaim == null)
                {
                    return Unauthorized("Token inválido, ID do utilizador não encontrado.");
                }

                long idUser = long.Parse(userIdClaim.Value);

                await _pedidoService.AceitarPedidoAsync(id, idUser);

                return Ok(new
                {
                    sucesso = true,
                    mensagem = "Pedido aceite com sucesso."
                });
            }
            catch (BusinessException ex)
            {
                return BadRequest(new { sucesso = false, mensagem = ex.Message });
            }
            catch (NotFoundException ex)
            {
                return NotFound(new { sucesso = false, mensagem = ex.Message });
            }
            catch (ApplicationException ex)
            {
                return StatusCode(500, new { sucesso = false, mensagem = ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { sucesso = false, mensagem = "Erro inesperado ao aceitar o pedido." });
            }
        }

        /// <summary>
        /// Rejeita um pedido de transação pendente.
        /// </summary>
        /// <remarks>
        /// Requer autenticação via JWT Bearer Token.
        /// O utilizador autenticado deve ter permissões para rejeitar o pedido.
        /// </remarks>
        /// <param name="id">Identificador do pedido de transação a rejeitar.</param>
        /// <response code="200">Pedido rejeitado com sucesso.</response>
        /// <response code="400">Regra de negócio impedindo a rejeição.</response>
        /// <response code="401">Token ausente ou inválido.</response>
        /// <response code="404">Pedido de transação não encontrado.</response>
        /// <response code="500">Erro interno ao rejeitar o pedido.</response>
        [Authorize]
        [HttpPut("{id}/rejeitar")]
        public async Task<IActionResult> RejeitarPedidoAsync(long id)
        {
            try
            {
                var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);

                if (userIdClaim == null)
                {
                    return Unauthorized("Token inválido, ID do utilizador não encontrado.");
                }

                long idUser = long.Parse(userIdClaim.Value);

                await _pedidoService.RejeitarPedidoAsync(id, idUser);

                return Ok(new
                {
                    sucesso = true,
                    mensagem = "Pedido rejeitado com sucesso."
                });
            }
            catch (BusinessException ex)
            {
                return BadRequest(new { sucesso = false, mensagem = ex.Message });
            }
            catch (NotFoundException ex)
            {
                return NotFound(new { sucesso = false, mensagem = ex.Message });
            }
            catch (ApplicationException ex)
            {
                return StatusCode(500, new { sucesso = false, mensagem = ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { sucesso = false, mensagem = "Erro inesperado ao rejeitar o pedido." });
            }
        }

        [Authorize]
        [HttpGet("{id}")]
        public async Task<IActionResult> GetInformacoesPedido(long id)
        {
            try
            {
                var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);

                if (userIdClaim == null)
                {
                    return Unauthorized("Token inválido, ID do utilizador não encontrado.");
                }

                long idUser = long.Parse(userIdClaim.Value);

                var pedido = await _pedidoService.ObterPorIdAsync(id, idUser);
                
                if (pedido == null)
                {
                    return NotFound(new { sucesso = false, mensagem = "Pedido não encontrado." });
                }

                return Ok(pedido);
            }
            catch (BusinessException ex)
            {
                return BadRequest(new { sucesso = false, mensagem = ex.Message });
            }
            catch (NotFoundException ex)
            {
                return NotFound(new { sucesso = false, mensagem = ex.Message });
            }
            catch (ApplicationException ex)
            {
                return StatusCode(500, new { sucesso = false, mensagem = ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { sucesso = false, mensagem = "Erro inesperado receber pedido." });
            }
        }
    }
}
   
