using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BookFlaz.Application.DTOs
{
    public class EditarCategoriaDTO
    {
        [Required(ErrorMessage = "O nome da categoria é obrigatório")]
        [StringLength(255, ErrorMessage = "O nome da categoria não pode exceder 255 caracteres")]
        [MinLength(1, ErrorMessage = "O nome da categoria deve ter pelo menos 1 caracteres")]
        [RegularExpression(@"^[\p{L}]+$", ErrorMessage = "O nome da categoria deve conter apenas letras")]
        public string Nome { get; set; } = string.Empty;

        public bool Ativo { get; set; }
    }
}
