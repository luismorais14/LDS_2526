using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BookFlaz.Domain.Entities
{
    public class Conversa
    {
        public long Id { get; private set; }

        [Required]
        public DateTime DataCriacao { get; private set; } = DateTime.Now;

        [Required]
        public long AnuncioId { get; private set; }

        [Required]
        public long CompradorId { get; private set; }

        [Required]
        public long VendedorId { get; private set; }

        private Conversa() { }
        internal Conversa(long vendedorID, long compradorId, long anuncioId)
        {
            AnuncioId = anuncioId;
            CompradorId = compradorId;
            VendedorId = vendedorID;
        }

        public static Conversa CriarConversa(long vendedorID, long compradorID, long anuncioID)
        {
            if (vendedorID <= 0) throw new ArgumentOutOfRangeException(nameof(vendedorID), "VendedorId inválido.");
            if (compradorID <= 0) throw new ArgumentOutOfRangeException(nameof(compradorID), "CompradorId inválido.");
            if (anuncioID <= 0) throw new ArgumentOutOfRangeException(nameof(anuncioID), "AnuncioId inválido.");
            if (vendedorID == compradorID) throw new ArgumentException("Vendedor e comprador não podem ser a mesma pessoa.");

            return new Conversa(vendedorID, compradorID, anuncioID);
        }
    }
}
