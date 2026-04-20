namespace ToyShoppingWebApp.API.Controllers
{
    using Microsoft.AspNetCore.Mvc;
    using ToyShoppingWebApp.Application.DTOs;
    using ToyShoppingWebApp.Application.Services.Interfaces;

    [ApiController]
    [Route("api/[controller]")]
    public class AuthController : ControllerBase
    {
        private readonly IAuthenticationService _authService;

        public AuthController(IAuthenticationService authService)
        {
            _authService = authService;
        }

        /// <summary>
        /// Register new user
        /// </summary>
        [HttpPost("register")]
        [Produces(typeof(AuthResponse))]
        [Consumes("application/json")]
        public async Task<ActionResult<AuthResponse>> Register([FromBody] RegisterRequest request)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var response = await _authService.RegisterAsync(request);

            if (!response.Success)
                return BadRequest(response);

            return Ok(response);
        }

        /// <summary>
        /// Login user
        /// </summary>
        [HttpPost("login")]
        [Produces(typeof(AuthResponse))]
        [Consumes("application/json")]
        public async Task<ActionResult<AuthResponse>> Login([FromBody] LoginRequest request)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var response = await _authService.LoginAsync(request);

            if (!response.Success)
                return Unauthorized(response);

            return Ok(response);
        }
    }
}