
using BookFlaz.Application.DTOs;
using BookFlaz.Application.Interfaces;
using BookFlaz.Domain.Repositories;
using Mapster;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BookFlaz.Application.Exceptions;

namespace BookFlaz.Application.Services
{
    public class ProfileService : IProfileService
    {
        private readonly IProfileRepository _profileRepository;

        public ProfileService(IProfileRepository profileRepository)
        {
            _profileRepository = profileRepository;
        }

        public async Task<VerPerfilDTO> VerPerfil(long id)
        {
            var perfil = await _profileRepository.ObterPerfilAsync(id);

            if (perfil == null)
            {
                throw new NotFoundException("Perfil não encontrado.");
            }

            var dto = new VerPerfilDTO
            {
                Nome = perfil.Nome,
                Anuncios = perfil.AnunciosVendedor.Adapt<AnuncioResumoDTO[]>(),
                Avaliacoes = perfil.AvaliacoesRecebidas.Adapt<AvaliacaoDTO[]>()
            };

            return dto;
        }
    }
}
