using BookFlaz.Application.DTOs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BookFlaz.Application.Interfaces
{
    public interface IProfileService
    {
        Task<VerPerfilDTO> VerPerfil(long id);
        
    }
}
