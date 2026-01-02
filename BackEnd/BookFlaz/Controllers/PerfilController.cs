using BookFlaz.Application.Exceptions;
using BookFlaz.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BookFlaz.API.Controllers
{
    public class PerfilController : ControllerBase
    {
        private readonly IProfileService _profileService;

        public PerfilController(IProfileService profileService)
        {
            _profileService = profileService;
        }

        [Authorize]
        [HttpGet("api/perfil/{id}")]
        public async Task<IActionResult> VerPerfil(long id)
        {
            try
            {
                var perfil = await _profileService.VerPerfil(id);
                return Ok(perfil);
            }
            catch (NotFoundException ex)
            {
                return NotFound(new { message = ex.Message });
            }
        }
    }
}
