using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BookFlaz.Application.Interfaces
{
    /// <summary>
    /// Define a interface para o serviço de gestão de uploads de imagens
    /// utilizados nos anúncios.
    /// </summary>
    public interface IUploadService
    {
        /// <summary>
        /// Realiza o upload de um ficheiro de imagem para o diretório de armazenamento configurado.
        /// </summary>
        Task<string> UploadAsync(IFormFile file);

        /// <summary>
        /// Remove uma imagem previamente carregada do sistema de ficheiros.
        /// </summary>
        Task DeleteImgAsync(string fileName);
    }
}
