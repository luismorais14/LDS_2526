using BookFlaz.Application.DTOs;
using BookFlaz.Application.Exceptions;
using BookFlaz.Application.Interfaces;
using BookFlaz.Application.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System;
using System.ComponentModel.DataAnnotations;
using System.Security.Claims;
using System.Threading.Tasks;
using ValidationException = BookFlaz.Application.Exceptions.ValidationException;

namespace BookFlaz.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class CategoriaController : ControllerBase
    {
        private readonly IClienteService clienteService;
        private readonly ICategoriaService categoriaService;

        public CategoriaController(ICategoriaService categoriaService, IClienteService clienteService)
        {
            this.categoriaService = categoriaService;
            this.clienteService = clienteService;
        }

        private bool TryGetUserId(out long userId)
        {
            var idStr = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            return long.TryParse(idStr, out userId);
        }

        private async Task<IActionResult?> EnsureAdminAsync()
        {
            if (!TryGetUserId(out var userId))
                return Unauthorized("Token inválido: ID do utilizador não encontrado.");

            var isAdmin = await clienteService.IsAdminAsync(userId);
            if (!isAdmin)
                return StatusCode(403, "Apenas administradores podem aceder a este recurso."); ;

            return null;
        }

        /// <summary>
        /// Cria uma nova categoria.
        /// </summary>
        /// <remarks>
        /// Exemplo:
        ///
        ///     POST /api/categoria
        ///     {
        ///       "nome": "Romance",
        ///       "ativo": true
        ///     }
        ///
        /// Retorna a categoria criada.
        /// </remarks>
        /// <param name="dto">Dados da categoria a criar.</param>
        /// <returns>Categoria criada.</returns>
        /// <response code="200">Categoria criada com sucesso.</response>
        /// <response code="400">Erro de validação ou pedido inválido.</response>
        /// <response code="401">Token ausente ou inválido.</response>
        /// <response code="403">Utilizador sem permissões.</response>
        [HttpPost]
        [ProducesResponseType(typeof(Domain.Entities.Categoria), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        public async Task<IActionResult> CriarCategoria([FromBody] CriarCategoriaDTO dto)
        {
            if (await EnsureAdminAsync() is IActionResult guard) return guard;

            try
            {
                var categoria = await categoriaService.CriarCategoriaAsync(dto);
                return Ok(categoria);
            }
            catch (ValidationException ex) { return BadRequest(ex.Message); }
            catch (ArgumentException ex) { return BadRequest(ex.Message); }
            catch (ApplicationException) { return StatusCode(500, "Erro inesperado ao criar a categoria."); }
        }

        /// <summary>
        /// Atualiza uma categoria existente.
        /// </summary>
        /// <remarks>
        /// Exemplo:
        ///
        ///     PUT /api/categoria/1
        ///     {
        ///       "nome": "Ficção",
        ///       "ativo": false
        ///     }
        ///
        /// Retorna a categoria atualizada.
        /// </remarks>
        /// <param name="id">Identificador da categoria.</param>
        /// <param name="dto">Dados a atualizar.</param>
        /// <returns>Categoria atualizada.</returns>
        /// <response code="200">Categoria atualizada com sucesso.</response>
        /// <response code="400">Erro de validação ou pedido inválido.</response>
        /// <response code="401">Token ausente ou inválido.</response>
        /// <response code="403">Utilizador sem permissões.</response>
        [HttpPut("{id}")]
        [ProducesResponseType(typeof(Domain.Entities.Categoria), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        public async Task<IActionResult> EditarCategoria(long id, [FromBody] EditarCategoriaDTO dto)
        {
            if (await EnsureAdminAsync() is IActionResult guard) return guard;

            try
            {
                var categoriaEditada = await categoriaService.EditarCategoriaAsync(dto, id);
                return Ok(categoriaEditada);
            }
            catch (NotFoundException ex) { return NotFound(ex.Message); }
            catch (ValidationException ex) { return BadRequest(ex.Message); }
            catch (ArgumentException ex) { return BadRequest(ex.Message); }
            catch (ApplicationException) { return StatusCode(500, "Erro inesperado ao editar a categoria."); }
        }

        /// <summary>
        /// Remove uma categoria.
        /// </summary>
        /// <remarks>
        /// Exemplo:
        ///
        ///     DELETE /api/categoria/1
        ///
        /// Retorna <c>true</c> quando removida.
        /// </remarks>
        /// <param name="id">Identificador da categoria.</param>
        /// <returns>Resultado da remoção.</returns>
        /// <response code="200">Categoria removida com sucesso.</response>
        /// <response code="400">Não foi possível remover a categoria.</response>
        /// <response code="401">Token ausente ou inválido.</response>
        /// <response code="403">Utilizador sem permissões.</response>
        [HttpDelete("{id}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        public async Task<IActionResult> RemoverCategoria(long id)
        {
            if (await EnsureAdminAsync() is IActionResult guard) return guard;

            try
            {
                var categoriaRemovida = await categoriaService.RemoverCategoriaAsync(id);

                if (!categoriaRemovida)
                    return BadRequest(categoriaRemovida);

                return Ok(categoriaRemovida);
            }
            catch (ArgumentException ex) { return BadRequest(ex.Message); } // ex.: tem anúncios vinculados
            catch (ApplicationException) { return StatusCode(500, "Erro inesperado ao remover a categoria."); }
        }

        /// <summary>
        /// Lista categorias com filtros opcionais.
        /// </summary>
        /// <remarks>
        /// Exemplo:
        ///
        ///     GET /api/categoria/categorias?nome=Romance&amp;ativo=true
        ///
        /// Retorna uma coleção (pode ser vazia).
        /// </remarks>
        /// <param name="nome">Filtro por nome (parcial).</param>
        /// <param name="ativo">Filtro por estado (true=ativas, false=inativas).</param>
        /// <returns>Lista de categorias.</returns>
        /// <response code="200">Lista obtida com sucesso.</response>
        /// <response code="401">Token ausente ou inválido.</response>
        /// <response code="403">Utilizador sem permissões.</response>
        [HttpGet("categorias")]
        [ProducesResponseType(typeof(IEnumerable<Domain.Entities.Categoria>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        public async Task<IActionResult> VisualizarCategorias([FromQuery] string? nome, [FromQuery] bool? ativo)
        {
            if (await EnsureAdminAsync() is IActionResult guard) return guard;

            try
            {
                var categorias = await categoriaService.VisualizarCategoriasAsync(nome, ativo);
                return Ok(categorias);
            }
            catch (ApplicationException)
            {
                return StatusCode(500, "Erro ao listar categorias.");
            }
        }

        /// <summary>
        /// Lista apenas as categorias ativas (público, não requer autenticação).
        /// </summary>
        /// <remarks>
        /// Exemplo:
        ///
        ///     GET /api/categoria/ativas
        ///
        /// Retorna apenas as categorias que estão ativas no sistema.
        /// </remarks>
        /// <returns>Lista de categorias ativas.</returns>
        /// <response code="200">Lista obtida com sucesso.</response>
        /// <response code="500">Erro inesperado ao listar categorias.</response>
        [HttpGet("categorias/disponiveis")]
        [AllowAnonymous]
        [ProducesResponseType(typeof(IEnumerable<Domain.Entities.Categoria>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> VisualizarCategoriasDisponíveis()
        {
            try
            {
                var categorias = await categoriaService.VisualizarCategoriasAsync(null, true);
                return Ok(categorias);
            }
            catch (ApplicationException)
            {
                return StatusCode(500, "Erro ao listar categorias.");
            }
        }
    }
}
