using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BookFlaz.Domain.Entities
{
    public class Livro
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public long Isbn { get; private set; }

        [Required]
        [StringLength(255)]
        public string Titulo { get; private set; } = string.Empty;

        [Required]
        [StringLength(255)]
        public string Autor { get; private set; } = string.Empty;

        public static Livro AdicionarLivro(long Isbn, string Titulo, string Autor)
        {
            return new Livro(Isbn, Titulo, Autor);
        }

        private Livro() { }

        internal Livro(long isbn, string titulo, string autor)
        {
            this.Isbn = isbn;
            this.Titulo = titulo;
            this.Autor = autor;
        }
    }

}
