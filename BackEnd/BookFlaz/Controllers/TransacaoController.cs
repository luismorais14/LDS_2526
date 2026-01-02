using BookFlaz.Application.DTOs;
using BookFlaz.Application.Exceptions;
using BookFlaz.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace BookFlaz.API.Controllers
{

    [ApiController]
    [Route("api/[controller]")]
    public class TransacaoController : ControllerBase
    {
        private readonly ITransacaoService _transacaoService;

        /// <summary>
        /// Construtor do controlador de transações.
        /// </summary>
        /// <param name="transacaoService"></param>
        public TransacaoController(ITransacaoService transacaoService)
        {
            _transacaoService = transacaoService;
        }


        /// <summary>
        /// Cria uma nova transação.
        /// </summary>
        /// <remarks>
        /// Requer autenticação via JWT Bearer Token.
        /// O ID do utilizador é extraído automaticamente do token.
        /// O corpo da requisição deve conter os dados de <see cref="CriarTransacaoDTO"/>.
        /// </remarks>
        /// <param name="dto">Dados para criação da transação.</param>
        /// <response code="200">Transação criada com sucesso e definida como pendente.</response>
        /// <response code="400">Dados inválidos, erro de validação ou regra de negócio.</response>
        /// <response code="401">Token ausente ou inválido.</response>
        /// <response code="404">Recurso associado não encontrado.</response>
        /// <response code="500">Erro interno ao criar a transação.</response>
        [Authorize]
        [HttpPost]
        public async Task<IActionResult> CriarTransacao([FromBody] CriarTransacaoDTO dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            try
            {
                var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);

                if (userIdClaim == null)
                {
                    return Unauthorized(new { sucesso = false, mensagem = "Token inválido, ID do utilizador não encontrado." });
                }

                long idUser = long.Parse(userIdClaim.Value);

                var transacaoCriada = await _transacaoService.CriarTransacaoAsync(dto, idUser);

                return Ok(
                    new
                    {
                        sucesso = true,
                        mensagem = "Transação criada com sucesso e definida como pendente.",
                        transacao = transacaoCriada
                    }
                );
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
            catch (Exception)
            {
                return StatusCode(
                    500,
                    new { sucesso = false, mensagem = "Erro inesperado ao criar a transação." }
                );
            }
        }


        /// <summary>
        /// Cancela uma transação existente.
        /// </summary>
        /// <remarks>
        /// Requer autenticação via JWT Bearer Token.
        /// O utilizador autenticado deve ter permissões para cancelar a transação.
        /// </remarks>
        /// <param name="transacaoId">Identificador da transação a cancelar.</param>
        /// <response code="200">Transação cancelada com sucesso.</response>
        /// <response code="400">Regra de negócio impedindo o cancelamento.</response>
        /// <response code="401">Token ausente ou inválido.</response>
        /// <response code="404">Transação não encontrada.</response>
        /// <response code="500">Erro interno ao cancelar a transação.</response>
        [Authorize]
        [HttpPost("cancelar/{transacaoId:long}")]
        public async Task<IActionResult> CancelarTransacao(
            long transacaoId
        )
        {
            try
            {
                var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
                if (userIdClaim == null)
                {
                    return Unauthorized(new { sucesso = false, mensagem = "Token inválido, ID do utilizador não encontrado." });
                }

                long idUser = long.Parse(userIdClaim.Value);


                await _transacaoService.CancelarTransacaoAsync(transacaoId, idUser);
                return Ok(new { sucesso = true, mensagem = "Transação cancelada com sucesso." });
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
            catch (Exception)
            {
                return StatusCode(
                    500,
                    new { sucesso = false, mensagem = "Erro inesperado ao cancelar a transação." }
                );
            }
        }

        /// <summary>
        /// Confirma a receção do item pelo comprador.
        /// </summary>
        /// <remarks>
        /// Requer autenticação via JWT Bearer Token.
        /// Apenas o comprador associado à transação pode executar esta ação.
        /// </remarks>
        /// <param name="transacaoId">Identificador da transação.</param>
        /// <response code="200">Receção confirmada; transação concluída com sucesso.</response>
        /// <response code="400">Regra de negócio impedindo a confirmação.</response>
        /// <response code="401">Token ausente ou inválido.</response>
        /// <response code="404">Transação não encontrada.</response>
        /// <response code="500">Erro interno ao confirmar a receção.</response
        [Authorize]
        [HttpPost("confirmar-rececao-comprador/{transacaoId:long}")]
        public async Task<IActionResult> ConfirmarRececaoComprador(
            long transacaoId
        )
        {
            try
            {
                var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
                if (userIdClaim == null)
                {
                    return Unauthorized(new { sucesso = false, mensagem = "Token inválido, ID do utilizador não encontrado." });
                }
                long idUser = long.Parse(userIdClaim.Value);
                await _transacaoService.ConfirmarRececaoCompradorAsync(transacaoId, idUser);
                return Ok(
                    new
                    {
                        sucesso = true,
                        mensagem = "Transação concluída com sucesso (confirmada pelo comprador).",
                    }
                );
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
            catch (Exception)
            {
                return StatusCode(
                    500,
                    new { sucesso = false, mensagem = "Erro inesperado ao confirmar receção." }
                );
            }
        }

        /// <summary>
        /// Regista o pedido de devolução de uma transação.
        /// </summary>
        /// <remarks>
        /// Requer autenticação via JWT Bearer Token.
        /// Normalmente efetuado pelo comprador após a entrega do item.
        /// </remarks>
        /// <param name="transacaoId">Identificador da transação.</param>
        /// <response code="200">Devolução registada; a aguardar confirmação do vendedor.</response>
        /// <response code="400">Regra de negócio impedindo o registo de devolução.</response>
        /// <response code="401">Token ausente ou inválido.</response>
        /// <response code="403">Utilizador sem permissões para registar a devolução.</response>
        /// <response code="404">Transação não encontrada.</response>
        /// <response code="500">Erro interno ao registar a devolução.</response>
        [Authorize]
        [HttpPost("devolucao/{transacaoId:long}")]
        public async Task<IActionResult> RegistarDevolucao(
            long transacaoId
        )
        {
            try
            {
                var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
                if (userIdClaim == null)
                {
                    return Unauthorized(new { sucesso = false, mensagem = "Token inválido, ID do utilizador não encontrado." });
                }


                long idUser = long.Parse(userIdClaim.Value);

                await _transacaoService.RegistarDevolucaoAsync(transacaoId, idUser);
                return Ok(
                    new
                    {
                        sucesso = true,
                        mensagem = "Devolução registada com sucesso. Aguarda confirmação do vendedor.",
                    }
                );
            }
            catch (UnauthorizedActionException ex)
            {
                return StatusCode(403, new { sucesso = false, mensagem = ex.Message });
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
            catch (Exception)
            {
                return StatusCode(
                    500,
                    new { sucesso = false, mensagem = "Erro inesperado ao registar devolução." }
                );
            }
        }

        /// <summary>
        /// Confirma a devolução da transação pelo vendedor.
        /// </summary>
        /// <remarks>
        /// Requer autenticação via JWT Bearer Token.
        /// Apenas o vendedor associado à transação pode confirmar a devolução.
        /// </remarks>
        /// <param name="transacaoId">Identificador da transação.</param>
        /// <response code="200">Devolução confirmada; anúncio reativado.</response>
        /// <response code="401">Token ausente ou inválido.</response>
        /// <response code="403">Utilizador sem permissões para confirmar a devolução.</response>
        /// <response code="404">Transação não encontrada.</response>
        /// <response code="500">Erro interno ao confirmar a devolução.</response>
        [Authorize]
        [HttpPost("devolucao/confirmar/{transacaoId:long}")]
        public async Task<IActionResult> ConfirmarDevolucao(
            long transacaoId
        )
        {
            try
            {
                var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);

                if (userIdClaim == null)
                {
                    return Unauthorized(new { sucesso = false, mensagem = "Token inválido, ID do utilizador não encontrado." });
                }

                long idUser = long.Parse(userIdClaim.Value);

                await _transacaoService.ConfirmarDevolucaoAsync(transacaoId, idUser);
                return Ok(
                    new
                    {
                        sucesso = true,
                        mensagem = "Devolução confirmada pelo vendedor. O anúncio foi reativado.",
                    }
                );
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
            catch (Exception)
            {
                return StatusCode(
                    500,
                    new { sucesso = false, mensagem = "Erro inesperado ao confirmar devolução." }
                );
            }
        }

       

        [Authorize]
        [HttpGet("registo")]
        public async Task<IActionResult> ObterRegistoAsync([FromQuery] TransacaoFiltroDTO filtro)
        {
            try
            {
                var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
                if (userIdClaim == null)
                {
                    return Unauthorized(new { sucesso = false, mensagem = "Token inválido, ID do utilizador não encontrado." });
                }

                long idUser = long.Parse(userIdClaim.Value);

                var lista = await _transacaoService.ObterRegistoAsync(idUser, filtro);
                return Ok(lista);
            }
            catch (NotFoundException ex)
            {
                return NotFound(new { sucesso = false, mensagem = ex.Message });
            }
            catch (ApplicationException ex)
            {
                return StatusCode(500, new { sucesso = false, mensagem = ex.Message });
            }
            catch (Exception)
            {
                return StatusCode(500, new { sucesso = false, mensagem = "Erro inesperado ao obter registo de transações." });
            }
        }
    }
}



