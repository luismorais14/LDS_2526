using BookFlaz.Domain.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BookFlaz.Application.DTOs
{
    public class VisualizarAnuncioDTO
    {
        public long Id { get; set; }
        public decimal Preco { get; set; }
        public EstadoLivro EstadoLivro { get; set; }
        public TipoAnuncio TipoAnuncio { get; set; }
        public EstadoAnuncio EstadoAnuncio { get; set; }
        public string Imagens { get; set; }
        public DateTime DataCriacao { get; set; }
        public string NomeVendedor { get; set; }
        public string Categoria { get; set; }
        public int TotalFavoritos { get; set; }
        public string Titulo { get; set; }
        public string Autor { get; set; }
    }
}
