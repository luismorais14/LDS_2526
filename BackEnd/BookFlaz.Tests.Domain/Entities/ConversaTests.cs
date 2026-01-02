using BookFlaz.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BookFlaz.Tests.Domain.Entities
{
    public class ConversaTests
    {
        [Fact]
        public void CriarConversa_ComDadosValidos_DeveRetornarConversa()
        {
            long vendedorID = 1;
            long compradorID = 2;
            long anuncioID = 5;

            var conversa = Conversa.CriarConversa(vendedorID, compradorID, anuncioID);

            Assert.Equal(vendedorID, conversa.VendedorId);
            Assert.Equal(compradorID, conversa.CompradorId);
            Assert.Equal(anuncioID, conversa.AnuncioId);
            Assert.True(conversa.DataCriacao <= DateTime.Now);
        }

        [Theory]
        [InlineData(0, 2, 3, "vendedorID")]
        [InlineData(-1, 2, 3, "vendedorID")]
        [InlineData(1, 0, 3, "compradorID")]
        [InlineData(1, -5, 3, "compradorID")]
        [InlineData(1, 2, 0, "anuncioID")]
        [InlineData(1, 2, -3, "anuncioID")]
        public void CriarConversa_ComIdsInvalidos_DeveLancarOutOfRange(long vend, long comp, long anun, string _)
        {
            Assert.Throws<ArgumentOutOfRangeException>(() => Conversa.CriarConversa(vend, comp, anun));
        }

        [Fact]
        public void CriarConversa_ComVendedorIgualComprador_DeveLancar()
        {
            Assert.Throws<ArgumentException>(() => Conversa.CriarConversa(10, 10, 1));
        }
    }
}

