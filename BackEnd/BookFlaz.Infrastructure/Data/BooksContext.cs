using BookFlaz.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BookFlaz.Infrastructure.Data
{
    public class BooksContext : DbContext
    {
        public BooksContext(DbContextOptions<BooksContext> options) : base(options) { }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<Anuncio>()
                .Property(a => a.Preco)
                .HasPrecision(10, 2);

            modelBuilder.Entity<Anuncio>()
                .HasOne(a => a.Vendedor)
                .WithMany(c => c.AnunciosVendedor)
                .HasForeignKey(a => a.VendedorId)
                .OnDelete(DeleteBehavior.Cascade)
                .IsRequired(false);

            modelBuilder.Entity<Anuncio>()
                .HasOne(a => a.Categoria)
                .WithMany()
                .HasForeignKey(a => a.CategoriaId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<Anuncio>()
                .HasOne(a => a.Livro)
                .WithMany()
                .HasForeignKey(a => a.LivroIsbn)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<Favorito>()
                .HasOne(f => f.Anuncio)
                .WithMany(a => a.Favoritos)
                .HasForeignKey(f => f.AnuncioId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<Favorito>()
                .HasOne(f => f.Cliente)
                .WithMany(c => c.Favoritos)
                .HasForeignKey(f => f.ClienteId)
                .OnDelete(DeleteBehavior.NoAction)
                .IsRequired(false);

            modelBuilder.Entity<Avaliacao>()
                .HasOne(a => a.Transacao)
                .WithMany()
                .HasForeignKey(a => a.TransacaoId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<Avaliacao>()
                .HasOne(a => a.Avaliado)
                .WithMany(c => c.AvaliacoesRecebidas)
                .HasForeignKey(a => a.AvaliadoId)
                .OnDelete(DeleteBehavior.NoAction)
                .IsRequired(false);

            modelBuilder.Entity<Avaliacao>()
                .HasOne((a => a.Avaliador))
                .WithMany(c => c.AvaliacoesFeitas)
                .HasForeignKey(a => a.AvaliadorId)
                .OnDelete(DeleteBehavior.NoAction)
                .IsRequired(false);


            modelBuilder.Entity<Conversa>(entity =>
            {
                entity.HasOne<Cliente>()
                    .WithMany()
                    .HasForeignKey(c => c.CompradorId)
                    .OnDelete(DeleteBehavior.NoAction)
                    .IsRequired(false);

                entity.HasOne<Cliente>()
                    .WithMany()
                    .HasForeignKey(c => c.VendedorId)
                    .OnDelete(DeleteBehavior.ClientNoAction)
                    .IsRequired(false);
                
                entity.HasOne<Anuncio>()
                    .WithMany()
                    .HasForeignKey(c => c.AnuncioId)
                    .OnDelete(DeleteBehavior.NoAction);
            });

            modelBuilder.Entity<Mensagem>(entity =>
            {
                entity.HasOne<Conversa>()
                    .WithMany()
                    .HasForeignKey(m => m.ConversaId)
                    .OnDelete(DeleteBehavior.NoAction);
                
                entity.HasOne<Cliente>()
                    .WithMany()
                    .HasForeignKey(m => m.ClienteId)
                    .OnDelete(DeleteBehavior.NoAction)
                    .IsRequired(false);
            });

            modelBuilder.Entity<Notificacao>(entity =>
            {
                entity.HasOne<Cliente>()
                    .WithMany()
                    .HasForeignKey(n => n.ClientId)
                    .OnDelete(DeleteBehavior.Cascade)
                    .IsRequired(false);
            });
            
            modelBuilder.Entity<PedidoTransacao>(entity =>
            {
                entity.HasOne<Cliente>()
                    .WithMany()
                    .HasForeignKey(p => p.RemetenteId)
                    .OnDelete(DeleteBehavior.NoAction);

                entity.HasOne<Cliente>()
                    .WithMany()
                    .HasForeignKey(p => p.DestinatarioId)
                    .OnDelete(DeleteBehavior.NoAction);

                entity.HasOne<Cliente>()
                    .WithMany()
                    .HasForeignKey(p => p.CompradorId)
                    .OnDelete(DeleteBehavior.NoAction)
                    .IsRequired(false);

                entity.HasOne<Cliente>()
                    .WithMany()
                    .HasForeignKey(p => p.VendedorId)
                    .OnDelete(DeleteBehavior.NoAction)
                    .IsRequired(false);

                entity.HasOne<Anuncio>()
                    .WithMany()
                    .HasForeignKey(p => p.AnuncioId)
                    .OnDelete(DeleteBehavior.NoAction);

                entity.HasOne<Conversa>()
                    .WithMany()
                    .HasForeignKey(p => p.ConversaId)
                    .OnDelete(DeleteBehavior.NoAction);
            });

            modelBuilder.Entity<Transacao>(entity =>
            {
                entity.HasOne<PedidoTransacao>()
                    .WithOne()
                    .HasForeignKey<Transacao>(t => t.PedidoId)
                    .OnDelete(DeleteBehavior.Cascade);

                entity.HasOne<Cliente>()
                    .WithMany()
                    .HasForeignKey(t => t.CompradorId)
                    .OnDelete(DeleteBehavior.NoAction)
                    .IsRequired(false);

                entity.HasOne<Cliente>()
                    .WithMany()
                    .HasForeignKey(t => t.VendedorId)
                    .OnDelete(DeleteBehavior.NoAction)
                    .IsRequired(false);
            });

            modelBuilder.Entity<Devolucao>(entity =>
            {
                entity.HasOne<Transacao>()
                    .WithMany()
                    .HasForeignKey(d => d.TransacaoId)
                    .OnDelete(DeleteBehavior.Cascade);

                entity.HasOne<Cliente>()
                    .WithMany()
                    .HasForeignKey(d => d.clienteId)
                    .OnDelete(DeleteBehavior.NoAction)
                    .IsRequired(false);
            });

            modelBuilder.Entity<MovimentoPontos>(entity =>
            {
                entity.HasOne(c => c.Cliente)
                    .WithMany()
                    .HasForeignKey(m => m.ClienteId)
                    .OnDelete(DeleteBehavior.Cascade)
                    .IsRequired(false);

                entity.HasOne(m => m.Transacao)
                    .WithMany()
                    .HasForeignKey(m => m.TransacaoId)
                    .IsRequired(false)
                    .OnDelete(DeleteBehavior.SetNull);
            });

            modelBuilder.Entity<Cliente>().HasQueryFilter(c => c.IsAtivo);
        }

        public DbSet<Anuncio> Anuncios { get; set; }
        public DbSet<Cliente> Clientes { get; set; }
        public DbSet<Livro> Livros { get; set; }
        public DbSet<Categoria> Categorias { get; set; }
        public DbSet<PedidoTransacao> PedidosTransacao { get; set; }
        public DbSet<Transacao> Transacoes { get; set; }
        public DbSet<Conversa> Conversas { get; set; }
        public DbSet<Mensagem> Mensagens { get; set; }
        public DbSet<Favorito> Favoritos { get; set; }
        public DbSet<Notificacao> Notificacoes { get; set; }
        public DbSet<Devolucao> Devolucoes { get; set; }
        public DbSet<Avaliacao> Avaliacoes { get; set; }
        public DbSet<MovimentoPontos> MovimentosPontos { get; set; }
    }
}