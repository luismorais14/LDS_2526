using BookFlaz.Domain.Enums;

namespace BookFlaz.Application.DTOs
{
    public class AnuncioFavoritoDTO
    {
        public long Id { get; set; }
        public string? Titulo { get; set; }
        public string? Imagem { get; set; }
        public decimal Preco { get; set; }
        public string Categoria { get; set; }
        public EstadoLivro EstadoLivro { get; set; }
        public TipoAnuncio TipoAnuncio { get; set; }
        public int TotalFavoritos { get; set; }
        public bool Favorito { get; set; } = true;
    }
}
