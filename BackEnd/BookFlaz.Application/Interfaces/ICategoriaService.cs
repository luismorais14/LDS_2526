using BookFlaz.Application.DTOs;
using BookFlaz.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BookFlaz.Application.Interfaces
{
    public interface ICategoriaService
    {
        Task<Categoria> CriarCategoriaAsync(CriarCategoriaDTO categoriaDTO);
        Task<Categoria> EditarCategoriaAsync(EditarCategoriaDTO dto, long id);
        Task<bool> RemoverCategoriaAsync(long id);
        Task<List<Categoria>> VisualizarCategoriasAsync(string? nome, bool? ativo);
    }
}
