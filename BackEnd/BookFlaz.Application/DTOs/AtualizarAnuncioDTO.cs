using BookFlaz.Domain.Enums;
using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BookFlaz.Application.DTOs
{
    /// <summary>
    /// DTO utilizado para atualizar as informações de um anúncio existente na plataforma BookFlaz.
    /// </summary>
    public class AtualizarAnuncioDTO
    {
        [Required]
        public decimal Preco { get; set; }

        [Required]
        public long LivroIsbn { get; set; }

        [Required]
        public long CategoriaId { get; set; }

        [Required]
        public EstadoLivro EstadoLivro { get; set; }

        [Required]
        public TipoAnuncio TipoAnuncio { get; set; }

        public List<IFormFile>? NovasImagens { get; set; } = new();

        public List<string> ImagensRemover { get; set; } = new();
    }
}
