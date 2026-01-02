using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BookFlaz.Domain.Entities
{
    public class Categoria
    {
        public long Id { get; private set; }

        [Required(ErrorMessage = "O nome da categoria é obrigatório")]
        [StringLength(255, ErrorMessage = "O nome da categoria não pode exceder 255 caracteres")]
        [MinLength(1, ErrorMessage = "O nome da categoria deve ter pelo menos 1 caracteres")]
        [RegularExpression(@"^[\p{L}]+$", ErrorMessage = "O nome da categoria deve conter apenas letras")]
        public string Nome { get; private set; } = string.Empty;

        public bool Ativo { get; private set; } = true;

        public Categoria() { }

        internal Categoria(string nome, bool ativo = true)
        {
            this.Nome = nome.Trim();
            this.Ativo = ativo;
        }
        public static Categoria CriarCategoria(string? nome, bool? ativo)
        {
            if (string.IsNullOrWhiteSpace(nome))
            {
                throw new ArgumentException("O nome da categoria não pode ser vazio ou nulo.");
            }

            nome = nome.Trim();

            if (nome.Length > 255)
            {
                throw new ValidationException("O nome da categoria não pode exceder 255 caracteres.");
            }

            if (!System.Text.RegularExpressions.Regex.IsMatch(nome, @"^[\p{L}]+$"))
            {
                throw new ValidationException("O nome da categoria deve conter apenas letras.");
            }

            return new Categoria
            {
                Nome = nome,
                Ativo = ativo ?? true
            };
        }


        public static Categoria EditarCategoria(string? nome, bool? ativo, Categoria categoria)
        {
            if (categoria == null)
                throw new ArgumentNullException(nameof(categoria));

            if (string.IsNullOrWhiteSpace(nome))
                throw new ArgumentException("O nome da categoria é obrigatório.", nameof(nome));

            nome = nome.Trim();

            if (nome.Length > 255)
            {
                throw new ValidationException("O nome da categoria não pode exceder 255 caracteres.");
            }

            if (!System.Text.RegularExpressions.Regex.IsMatch(nome, @"^[\p{L}]+$"))
            {
                throw new ValidationException("O nome da categoria deve conter apenas letras.");
            }

            categoria.Nome = nome;

            if (ativo.HasValue)
                categoria.Ativo = ativo.Value;


            return categoria;
        }

    }
}
