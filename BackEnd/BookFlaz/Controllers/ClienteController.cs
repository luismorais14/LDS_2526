using System.Security.Claims;
using BookFlaz.Application.DTOs;
using BookFlaz.Application.Exceptions;
using BookFlaz.Application.Interfaces;
using Microsoft.AspNetCore.Mvc;
using BookFlaz.Application.Services;
using Microsoft.AspNetCore.Authorization;
using Grpc.Core;
using Mapster;
using BookFlaz.Domain.Entities;

namespace BookFlaz.API.Controllers
{

    [Route("api/[controller]")]
    [ApiController]
    public class ClienteController : ControllerBase
    {
        private readonly IClienteService clienteService;
        private readonly IIdentityService identityService;

        /// <summary>
        /// Construtor que injeta a dependência do serviço responsável pela gestão de clientes.
        /// </summary>
        /// <param name="clienteService">
        /// Instância de <see cref="ClienteService"/> utilizada para processar a lógica de negócio.
        /// </param>
        public ClienteController(IClienteService clienteService, IIdentityService identityService)
        {
            this.clienteService = clienteService;
            this.identityService = identityService;
        }

        /// <summary>
        /// Cria um novo cliente na plataforma.
        /// </summary>
        /// <param name="dto">
        /// Dados necessários para a criação do cliente, enviados via formulário multipart/form-data.
        /// </param>
        /// <returns>
        /// Retorna <see cref="OkObjectResult"/> com mensagem de sucesso caso a criação seja bem-sucedida,
        /// ou <see cref="BadRequestObjectResult"/> se ocorrer erro de validação.
        /// </returns>
        /// <response code="200">Cliente criado com sucesso.</response>
        /// <response code="400">Erro de validação nos dados enviados.</response>
        [HttpPost("registar")]
        public async Task<IActionResult> CriarCliente([FromBody] RegistarClienteDTO dto)
        {
            if (!ModelState.IsValid || dto == null)
            {
                return BadRequest(ModelState);
            }

            var result = await clienteService.CriarUtilizadorAsync(dto);

            if (!result.Success)
                return BadRequest(result.Message);

            return Ok(result.Message);
        }

        /// <summary>
        /// Efetua o login de um cliente na plataforma.
        /// </summary>
        /// <param name="dto"> O email e password do cliente.</param>
        /// <returns> Retorna um token JWT se o login for bem-sucedido, ou uma mensagem de erro em caso contrário.</returns>
        /// <response code="200"> Login bem-sucedido, retorna token JWT.</response>
        [HttpPost("login")]
        public async Task<IActionResult> LoginCliente([FromBody] LoginClienteDTO dto)
        {
            try
            {
                if (!ModelState.IsValid || dto == null)
                {
                    return BadRequest(ModelState);
                }

                var user = identityService.Authenticate(dto.Email, dto.PasswordHash);

                var token = identityService.GenerateToken(user);

                var result = await clienteService.LoginUtilizadorAsync(dto.Email, dto.PasswordHash);
                if (!result.Success)
                    return BadRequest(result.Message);
                return Ok(new
                {
                    Message = result.Message,
                    Token = token

                });
            }	
            catch (NotFoundException ex)
            {
                return BadRequest(ex.Message);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        /// <summary>
        /// Edita os dados de um cliente existente na plataforma.
        /// </summary>
        /// <param name="id">O ID do cliente a ser editado.</param>
        /// <param name="dto"> Novos dados do cliente.</param>
        /// <returns> Retorna <see cref="OkObjectResult"/> com mensagem de sucesso caso a edição seja bem-sucedida, ou <see cref="BadRequestObjectResult"/> se ocorrer erro de validação.</returns>
        [Authorize]
        [HttpPut("editar/{id}")]
        public async Task<IActionResult> EditarCliente(long id, [FromBody] ClienteDTO dto)
        {
            try
            {
                if (!ModelState.IsValid || dto == null)
                {
                    return BadRequest(ModelState);
                }

                var result = await clienteService.EditarUtilizadorAsync(id, dto);

                if (!result.Success)
                {
                    return BadRequest(result.Message);
                }
                return Ok(result.Message);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [Authorize]
        [HttpGet("me")]
        public async Task<IActionResult> GetMeAsync()
        {
            var userId = long.Parse(User.FindFirst(ClaimTypes.NameIdentifier)!.Value);
            Cliente cliente = await clienteService.ObterClientePorId(userId);

            if (cliente != null) 
                return Ok(cliente);

            return BadRequest();
        }

        /// <summary>
        /// Elimina um cliente da plataforma.
        /// </summary>
        /// <param name="id"> O ID do cliente a ser eliminado.</param>
        /// <returns> Retorna <see cref="OkObjectResult"/> com mensagem de sucesso caso a eliminação seja bem-sucedida, ou <see cref="BadRequestObjectResult"/> se ocorrer erro.</returns>
        [Authorize]
        [HttpDelete("eliminar/{id}")]
        public async Task<IActionResult> EliminarCliente(long id)
        {
            try
            {
                var clienteId = long.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));

                if (clienteId != id)
                {
                    return Unauthorized("Unauthorized");
                }
                
                var result = await clienteService.EliminarUtilizadorAsync(clienteId);
                if (!result.Success)
                {
                    return BadRequest(result.Message);
                }
                return Ok(result.Message);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }
        
        /// <summary>
        /// Inicia o processo de recuperação de palavra-passe para um utilizador.
        /// </summary>
        /// <param name="dto">O email do utilizador a recuperar.</param>
        /// <returns>Retorna sempre Ok por razões de segurança, mesmo que o email não exista.</returns>
        [HttpPost("pedir-reset-password")]
        [AllowAnonymous]
        public async Task<IActionResult> PedirResetPassword([FromBody] PedirResetPasswordDTO dto)
        {
            var (success, message) = await clienteService.PedirResetPasswordAsync(dto.Email);

            if (!success)
            {
                return StatusCode(500, message);
            }
            
            return Ok(message);
        }
        
    }
}