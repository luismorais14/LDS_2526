using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BookFlaz.Domain.Entities
{
    public class Favorito
    {
        [Key]
        public long Id { get; private set; }

        [Required]
        public DateTime DataAdicionado { get; private set; }

        [Required]
        public long AnuncioId { get; private set; }

        [Required]
        public long ClienteId { get; private set; }

        // Navegações
        public virtual Cliente? Cliente { get; private set; }
        public virtual Anuncio? Anuncio { get; private set; }

        public static Favorito AdicionarFavorito(long anuncioId, long clienteId)
        {
            var favorito = new Favorito
            {
                DataAdicionado = DateTime.UtcNow,
                AnuncioId = anuncioId,
                ClienteId = clienteId
            };

            return favorito;
        }
    }
}
