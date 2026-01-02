using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using BookFlaz.Domain.Exceptions;

namespace BookFlaz.Domain.Entities
{
    public class Cliente
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public long Id { get; private set; }

        [Required]
        [StringLength(250)]
        public string Nome { get; private set; } = string.Empty;

        [Required]
        [StringLength(300)]
        [EmailAddress]
        public string Email { get; private set; } = string.Empty;

        [Required]
        public string PasswordHash { get; private set; } = string.Empty;

        [Required]
        [StringLength(20)]
        [Phone]
        public string Telefone { get; private set; } = string.Empty;

        [Required]
        public DateTime Dob { get; private set; }

        [Required]
        public int Pontos { get; private set; }

        [Required]
        public bool IsAdmin { get; private set; } = false;
        [Required]
        public bool NotificacoesAtivas { get; private set; } = true;
        
        [Required]
        public bool IsAtivo { get; private set; } = true;

        [Required] 
        public double ReputacaoMedia { get; private set; } = 0.0;
        
        [Required]
        public int TotalAvaliacoes { get; private set; } = 0;
        
        public string? PasswordResetToken { get; private set; }
        public DateTime? PasswordResetTokenExpiry { get; private set; } = DateTime.Now;

        public virtual ICollection<Anuncio> AnunciosVendedor { get; private set; } = new List<Anuncio>();
        public virtual ICollection<Favorito> Favoritos { get; private set; } = new List<Favorito>();
        public virtual ICollection<Avaliacao> AvaliacoesFeitas { get; private set; } = new List<Avaliacao>();
        public virtual ICollection<Avaliacao> AvaliacoesRecebidas { get; private set; } = new List<Avaliacao>();


        /// <summary>
        /// Define o hash da senha do cliente.
        /// </summary>
        /// <param name="passwordHash">O hash da senha a ser definido.</param>
        public void SetPasswordHash(string passwordHash)
        {
            this.PasswordHash = passwordHash;
        }

        public void AdicionarPontos(int pontos)
        {
            if (pontos < 0)
            {
                throw new DomainException("Os pontos a serem adicionados devem ser um valor positivo.");
            }

            this.Pontos += pontos;
        }

        public void RemoverPontos(int pontos)
        {
            if (pontos < 0)
            {
                throw new DomainException("Os pontos a serem removidos devem ser um valor positivo.");
            }

            if (pontos > this.Pontos) {
                throw new DomainException("O cliente não tem pontos suficientes para esta operação.");
            }

            if (this.Pontos - pontos < 0)
            {
                this.Pontos = 0;
            } else
            {
                this.Pontos -= pontos;
            }
        }

        public void EditarNotificacoesAtivas(bool ativo)
        {
            NotificacoesAtivas = ativo;
        }
        
        public void Inativar()
        {
            this.IsAtivo = false;
        }

        public void Reativar()
        {
            this.IsAtivo = true;
        }
        
        /// <summary>
        /// Atualiza os campos de reputação pré-calculados do cliente.
        /// </summary>
        /// <param name="novaMedia">A nova média calculada.</param>
        /// <param name="totalAvaliacoes">O número total de avaliações.</param>
        public void AtualizarReputacao(double novaMedia, int totalAvaliacoes)
        {
            this.ReputacaoMedia = novaMedia;
            this.TotalAvaliacoes = totalAvaliacoes;
        }
        
        public void SetPasswordResetToken(string token)
        {
            this.PasswordResetToken = token;
            this.PasswordResetTokenExpiry = DateTime.UtcNow.AddHours(1);
        }
    }
}