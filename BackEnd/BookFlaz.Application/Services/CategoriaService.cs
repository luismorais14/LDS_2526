using BookFlaz.Application.DTOs;
using BookFlaz.Application.Exceptions;
using BookFlaz.Application.Interfaces;
using BookFlaz.Domain.Entities;
using BookFlaz.Domain.Repositories;
using BookFlaz.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BookFlaz.Application.Services
{
    public class CategoriaService : ICategoriaService
    {
        private readonly ICategoriaRepository _repo;

        public CategoriaService(ICategoriaRepository repo)
        {
            _repo = repo;
        }

        /// <summary>
        /// Cria uma categoria.
        /// </summary>
        public async Task<Categoria> CriarCategoriaAsync(CriarCategoriaDTO dto)
        {
            try
            {
                if (await _repo.ExisteComMesmoNomeAsync(dto.Nome))
                    throw new ValidationException("Já existe uma categoria com esse nome.");

                var categoria = Categoria.CriarCategoria(dto.Nome, dto.Ativo);

                await _repo.AdicionarAsync(categoria);
                return categoria;
            }
            catch (ValidationException) { throw; }
            catch (ArgumentException ex) { throw new ArgumentException(ex.Message, ex); }
            catch (Exception ex)
            {
                throw new ApplicationException("Erro inesperado ao criar a categoria.", ex);
            }
        }


        /// <summary>
        /// Edita uma categoria existente.
        /// </summary>
        public async Task<Categoria> EditarCategoriaAsync(EditarCategoriaDTO dto, long id)
        {
            try
            {
                if (await _repo.ExisteComMesmoNomeAsync(dto.Nome, ignorandoId: id))
                    throw new ValidationException("Já existe uma categoria com esse nome.");

                var categoria = await _repo.ObterPorIdAsync(id)
                    ?? throw new NotFoundException("Categoria não encontrada.");

                var editada = Categoria.EditarCategoria(dto.Nome, dto.Ativo, categoria);

                _repo.Atualizar(editada);
                return editada;
            }
            catch (ValidationException) { throw; }
            catch (ArgumentException ex) { throw new ArgumentException(ex.Message, ex); }
            catch (NotFoundException) { throw; }
            catch (Exception ex)
            {
                throw new ApplicationException("Erro inesperado ao editar a categoria.", ex);
            }
        }

        /// <summary>
        /// Remove uma categoria (se não tiver anúncios associados).
        /// </summary>
        public async Task<bool> RemoverCategoriaAsync(long id)
        {
            try
            {
                var categoria = await _repo.ObterPorIdAsync(id);
                if (categoria is null) return false;

                if (await _repo.TemAnunciosVinculadosAsync(id))
                    throw new ArgumentException("Não é possível remover: existem anúncios vinculados à categoria.");

                _repo.Remover(categoria);
                return true;
            }
            catch (ArgumentException) { throw; } 
            catch (Exception ex)
            {
                throw new ApplicationException("Erro inesperado ao remover a categoria.", ex);
            }
        }

        /// <summary>
        /// Lista categorias com filtros opcionais.
        /// </summary>
        public async Task<List<Categoria>> VisualizarCategoriasAsync(string? nome, bool? ativo)
        {
            try
            {
                return await _repo.ListarAsync(nome, ativo);
            }
            catch (Exception ex)
            {
                throw new ApplicationException("Erro ao listar categorias.", ex);
            }
        }
    }
}
