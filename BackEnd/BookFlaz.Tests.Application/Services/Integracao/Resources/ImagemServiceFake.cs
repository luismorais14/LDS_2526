using BookFlaz.Application.Interfaces;
using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BookFlaz.Tests.Application.Services.Integracao.Resources
{
    public class ImagemServiceFake : IImagemService
    {
        public Task<bool> VerificarSeELivroAsync(IFormFile imagem)
        {
            return Task.FromResult(true); 
        }
    }
}
