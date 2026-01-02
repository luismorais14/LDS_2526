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
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class ChatController : ControllerBase
    {
        private readonly IChatService chatService;

        public ChatController(IChatService service) 
        {
            chatService = service;
        }

        private bool TryGetUserId(out long userId)
        {
            var idStr = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            return long.TryParse(idStr, out userId);
        }


        /// <summary>
        /// Envia uma mensagem numa conversa (cria a conversa se ainda não existir).
        /// </summary>
        /// <remarks>
        /// <b>Auth:</b> Requer Bearer Token. O remetente é sempre o utilizador autenticado (forçado no servidor).<br/>
        /// <br/>
        /// <b>Regras:</b>
        /// <list type="bullet">
        ///   <item>Se <c>conversaId</c> for fornecido, a mensagem é enviada nessa conversa (é validada a pertença e o anúncio).</item>
        ///   <item>Se não for fornecido, o serviço tenta localizar/abrir conversa entre o comprador (utilizador) e o vendedor do anúncio.</item>
        ///   <item><c>conteudo</c> é obrigatório (1..1000 caracteres, sem só espaços).</item>
        /// </list>
        /// <br/>
        /// <b>Exemplo de pedido (JSON):</b>
        /// <code language="json">
        /// {
        ///   "conversaId": 123,            // opcional
        ///   "anuncioId": 456,             // obrigatório
        ///   "conteudo": "Olá! Ainda está disponível?"
        /// }
        /// </code>
        /// <br/>
        /// <b>Exemplo de resposta 200 (JSON):</b>
        /// <code language="json">
        /// {
        ///   "id": 987,
        ///   "conteudo": "Olá! Ainda está disponível?",
        ///   "dataEnvio": "2025-11-03T21:45:12Z",
        ///   "clienteID": 42,
        ///   "conversaID": 123
        /// }
        /// </code>
        /// </remarks>
        /// <param name="mensagem">
        /// DTO com os dados da mensagem. O campo <c>ClienteId</c> recebido no body é ignorado;
        /// o servidor usa o ID do utilizador autenticado.
        /// </param>
        /// <returns>Mensagem criada, com identificadores e metadados preenchidos.</returns>
        /// <response code="200">Mensagem enviada com sucesso.</response>
        /// <response code="400">
        /// Pedido inválido (ex.: conteúdo vazio, tamanhos inválidos, incoerência de conversa/anúncio).
        /// </response>
        /// <response code="401">Token ausente ou inválido.</response>
        /// <response code="403">
        /// Operação não permitida (ex.: vendedor a tentar iniciar conversa consigo próprio
        /// ou utilizador não pertence à conversa indicada).
        /// </response>
        /// <response code="404">
        /// Recurso não encontrado (ex.: anúncio inexistente ou conversa inexistente).
        /// </response>
        /// <response code="500">Erro interno inesperado ao enviar a mensagem.</response>
        [HttpPost("EnviarMensagem")]
        [ProducesResponseType(typeof(BookFlaz.Domain.Entities.Mensagem), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(string), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(typeof(string), StatusCodes.Status403Forbidden)]
        [ProducesResponseType(typeof(string), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(string), StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> EnviarMensagem([FromBody] CriarMensagemDTO mensagem)
        {
            if (!TryGetUserId(out var userId))
                return Unauthorized("Token inválido: utilizador não identificado.");

            try
            {
                var mensagemEnviada = await chatService.EnviarMensagem(mensagem, userId);

                return Ok(mensagemEnviada);
            }
            catch(ValidationException ex)         
            {
                return BadRequest(ex.Message); 
            }
            catch (UnauthorizedActionException ex) 
            {
                return Forbid(ex.Message);
            }
            catch (NotFoundException ex)           
            {
                return NotFound(ex.Message);
            }
            catch (ApplicationException)           
            {
                return StatusCode(500, "Erro inesperado ao enviar a mensagem.");
            }
        }

        /// <summary>
        /// Obtém a lista de conversas do utilizador autenticado.
        /// </summary>
        /// <remarks>
        /// <b>Auth:</b> Requer Bearer Token.<br/>
        /// <br/>
        /// <b>Notas:</b>
        /// Retorna todas as conversas onde o utilizador é comprador ou vendedor.
        /// </remarks>
        /// <returns>Coleção de conversas do utilizador autenticado.</returns>
        /// <response code="200">Lista de conversas obtida com sucesso (pode vir vazia).</response>
        /// <response code="401">Token ausente ou inválido.</response>
        /// <response code="404">Utilizador não encontrado (quando aplicável).</response>
        /// <response code="500">Erro interno ao obter conversas.</response>
        [HttpGet("conversas")]
        [ProducesResponseType(typeof(IEnumerable<BookFlaz.Domain.Entities.Conversa>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(typeof(string), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(string), StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> ObterConversas()
        {
            if (!TryGetUserId(out var userId))
                return Unauthorized("Token inválido: utilizador não identificado.");

            try
            {
                var conversas = await chatService.ObterConversasPorUsuario(userId);

                return Ok(conversas);
            }
            catch (NotFoundException ex) 
            { 
                return NotFound(ex.Message);
            }
            catch (ApplicationException) 
            {
                return StatusCode(500, "Erro ao obter conversas.");
            }
        }

        /// <summary>
        /// Obtém as mensagens de uma conversa específica.
        /// </summary>
        /// <remarks>
        /// <b>Auth:</b> Requer Bearer Token.<br/>
        /// <br/>
        /// <b>Regras:</b>
        /// <list type="bullet">
        ///   <item><c>id</c> deve ser &gt; 0.</item>
        ///   <item>O utilizador autenticado tem de pertencer à conversa; caso contrário, devolve 403.</item>
        /// </list>
        /// <br/>
        /// <b>Exemplo de resposta 200 (JSON):</b>
        /// <code language="json">
        /// [
        /// {
        ///   "mensagens": [
        ///     { "id": 1, "conteudo": "Olá", "dataEnvio": "2025-11-03T21:40:00Z", "clienteId": 42, "conversaId": 123 },
        ///     { "id": 2, "conteudo": "Olá! Sim, está.", "dataEnvio": "2025-11-03T21:41:10Z", "clienteId": 7, "conversaId": 123 }
        ///   ],
        ///   "pedidos": [
        ///     { "id": 10, "estado": "Pendente", "anuncioId": 5, "conversaId": 123 },
        ///     { "id": 11, "estado": "Aceite",  "anuncioId": 8, "conversaId": 123 }
        ///   ]
        /// }
        /// </code>
        /// </remarks>
        /// <param name="id">Identificador da conversa.</param>
        /// <returns>Lista de mensagens da conversa.</returns>
        /// <response code="200">Mensagens obtidas com sucesso (pode vir vazia).</response>
        /// <response code="400">Identificador de conversa inválido.</response>
        /// <response code="401">Token ausente ou inválido.</response>
        /// <response code="403">O utilizador não tem acesso à conversa.</response>
        /// <response code="404">Conversa não encontrada.</response>
        /// <response code="500">Erro interno ao obter mensagens da conversa.</response>
        [HttpGet("{id:long}")]
        [ProducesResponseType(typeof(IEnumerable<BookFlaz.Domain.Entities.Mensagem>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(string), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(typeof(string), StatusCodes.Status403Forbidden)]
        [ProducesResponseType(typeof(string), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(string), StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> ObterMensagensPorConversa(long id)
        {
            if (id <= 0) return BadRequest("Identificador de conversa inválido.");

            if (!TryGetUserId(out var userId))
                return Unauthorized("Token inválido: utilizador não identificado.");

            try
            {
                var conversas = await chatService.ObterConversasPorUsuario(userId);

                if (conversas.Any(c => c.Id == id))
                {
                    var mensagens = await chatService.ObterMensagensPorConversa(id);
                    var conversa = await chatService.ObterConversa(id);
                    return Ok(new
                    {
                        conversa,
                        mensagens
                    });
                } else
                {
                    return Forbid("Não tem acesso a esta conversa.");
                }
            }
            catch (NotFoundException ex) 
            {
                return NotFound(ex.Message); 
            }
            catch (ApplicationException) 
            {
                return StatusCode(500, "Erro ao obter mensagens da conversa."); 
            }
        }
    }
}
