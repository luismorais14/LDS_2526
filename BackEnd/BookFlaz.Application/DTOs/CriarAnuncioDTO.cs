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
    /// DTO utilizado para criar um novo anúncio na plataforma BookFlaz.
    /// Contém todos os dados necessários para a criação de um anúncio válido,
    /// </summary>
    public class CriarAnuncioDTO
    {
        [Range(0, 10000)]
        public decimal? Preco { get; set; }

        [Required]
        public long LivroIsbn { get; set; }

        [Required]
        public long CategoriaId { get; set; }

        [Required]
        public EstadoLivro EstadoLivro { get; set; }

        [Required]
        public TipoAnuncio TipoAnuncio { get; set; }

        [Required]
        public List<IFormFile>? Imagens { get; set; }
    }
}
