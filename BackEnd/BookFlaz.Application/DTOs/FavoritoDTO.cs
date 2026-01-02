using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BookFlaz.Application.DTOs
{
    public class FavoritoDTO
    {
        [Required]
        public long AnuncioId { get; set; }
        [Required]
        public long ClienteId { get; set; }
    }
}
