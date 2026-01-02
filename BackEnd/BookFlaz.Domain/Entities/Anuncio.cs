using BookFlaz.Domain.Enums;
using BookFlaz.Domain.Exceptions;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BookFlaz.Domain.Entities
{
    public class Anuncio
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public long Id { get; private set; }

        public decimal Preco { get; private set; }

        [Required]
        [Column(TypeName = "varchar(20)")]
        public EstadoLivro EstadoLivro { get; private set; }

        [Required]
        [Column(TypeName = "varchar(20)")]
        public TipoAnuncio TipoAnuncio { get; private set; }

        [Required]
        [Column(TypeName = "varchar(20)")]
        public EstadoAnuncio EstadoAnuncio { get; private set; }

        [Required]
        public string Imagens { get; private set; } = string.Empty;

        [Required]
        public DateTime DataCriacao { get; private set; }

        public DateTime DataAtualizacao { get; private set; }

        [Required]
        [ForeignKey("Categoria")]
        public long CategoriaId { get; private set; }

        [Required]
        [ForeignKey("Livro")]
        public long LivroIsbn { get; private set; }

        [Required]
        [ForeignKey("Cliente")]
        public long VendedorId { get; private set; }

        public virtual Categoria? Categoria { get; private set; }
        public virtual Livro? Livro { get; private set; }
        public virtual Cliente? Vendedor { get; private set; }
        public virtual ICollection<Favorito> Favoritos { get; private set; } = new List<Favorito>();


        public Anuncio() { }

        internal Anuncio(long categoria_id, long vendedor_id, decimal preco, EstadoAnuncio estadoAnuncio,
                   EstadoLivro estadoLivro, TipoAnuncio tipoAnuncio, long livro_isbn, string imagens)
        {
            this.CategoriaId = categoria_id;
            this.VendedorId = vendedor_id;
            this.Preco = preco;
            this.EstadoAnuncio = estadoAnuncio;
            this.EstadoLivro = estadoLivro;
            this.TipoAnuncio = tipoAnuncio;
            this.LivroIsbn = livro_isbn;
            this.Imagens = imagens;
            this.DataCriacao = DateTime.Now;
            this.DataAtualizacao = DateTime.Now;
        }

        public static Anuncio CriarAnuncio(decimal? preco, long livroIsbn, long categoriaId, long vendedorId, EstadoLivro estadoLivro, TipoAnuncio tipoAnuncio, string imagens)
        {
            ValidarCriacao(preco, estadoLivro, tipoAnuncio);

            if (tipoAnuncio == TipoAnuncio.DOACAO)
            {
                preco = 0;
            }

            var anuncio = new Anuncio
            {
                Preco = preco ?? 0,
                EstadoLivro = estadoLivro,
                TipoAnuncio = tipoAnuncio,
                EstadoAnuncio = EstadoAnuncio.ATIVO,
                Imagens = imagens,
                DataCriacao = DateTime.UtcNow,
                LivroIsbn = livroIsbn,
                CategoriaId = categoriaId,
                VendedorId = vendedorId,
            };

            return anuncio;
        }

        public void AtualizarAnuncio(decimal preco, long livroIsbn, long categoriaId, EstadoLivro estadoLivro, TipoAnuncio tipoAnuncio, string imagensNovas)
        {
            ValidarCriacao(preco, estadoLivro, tipoAnuncio);

            this.Preco = tipoAnuncio == TipoAnuncio.DOACAO ? 0 : preco;
            this.LivroIsbn = livroIsbn;
            this.CategoriaId = categoriaId;
            this.EstadoLivro = estadoLivro;
            this.TipoAnuncio = tipoAnuncio;
            this.Imagens = imagensNovas;
            this.DataAtualizacao = DateTime.UtcNow;
        }

        public void AtualizarEstadoAnuncio(EstadoAnuncio estado)
        {
            this.EstadoAnuncio = estado;
            this.DataAtualizacao = DateTime.UtcNow;
        }

        private static void ValidarCriacao(decimal? preco, EstadoLivro estadoLivro, TipoAnuncio tipoAnuncio)
        {
            if (!Enum.IsDefined(typeof(TipoAnuncio), tipoAnuncio))
            {
                throw new DomainException("Tipo de anúncio inválido.");
            }

            if (!Enum.IsDefined(typeof(EstadoLivro), estadoLivro))
            {
                throw new DomainException("Estado do livro inválido.");
            }

            if (tipoAnuncio != TipoAnuncio.DOACAO && preco == null)
            {
                throw new DomainException("O preço é obrigatório para anúncios de venda.");
            }

            if (preco < 0)
            {
                throw new DomainException("Não pode ter preços negativos.");
            }

            if (preco > 10000)
            {
                throw new DomainException("O preço da venda não pode ser superior a 10000.");
            }
        }
    }
}
