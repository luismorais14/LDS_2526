using BookFlaz.Application.Interfaces;
using Microsoft.AspNetCore.Identity.Data;
using Microsoft.AspNetCore.Mvc;

namespace BookFlaz.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class IdentityController : ControllerBase
    {
        private readonly IIdentityService _identityService;

        /// <summary>
        /// Constructor for IdentityController.
        /// </summary>
        public IdentityController(IIdentityService identityService)
        {
            _identityService = identityService;
        }

        /// <summary>
        /// This endpoint allows a user to log in by providing their email and password.
        /// </summary>
        /// <param name="request">Request body containing email and password.</param>
        /// <returns> A JWT token if authentication is successful; otherwise, an unauthorized response.</returns>
        [HttpPost("login")]
        public IActionResult Login([FromBody] LoginRequest request)
        {
            var user = _identityService.Authenticate(request.Email, request.Password);
            if (user == null)
                return Unauthorized(new { message = "Email ou password incorretos" });

            var token = _identityService.GenerateToken(user);
            return Ok(new { token });
        }
    }
}