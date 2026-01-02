using BookFlaz.Domain.Entities;
using System.ComponentModel.DataAnnotations;

namespace BookFlaz.Application.DTOs
{
    public class VerPerfilDTO
    {
        [Required]
        public string Nome { get; set; }

        public AnuncioResumoDTO[] Anuncios { get; set; } = Array.Empty<AnuncioResumoDTO>();

        public AvaliacaoDTO[] Avaliacoes { get; set; } = Array.Empty<AvaliacaoDTO>();
    }
}
