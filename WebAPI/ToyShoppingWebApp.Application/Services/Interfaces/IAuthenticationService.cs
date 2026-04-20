namespace ToyShoppingWebApp.Application.Services.Interfaces
{
    using ToyShoppingWebApp.Application.DTOs;

    public interface IAuthenticationService
    {
        /// <summary>
        /// Register new user
        /// </summary>
        Task<AuthResponse> RegisterAsync(RegisterRequest request);

        /// <summary>
        /// Login user and return JWT token
        /// </summary>
        Task<AuthResponse> LoginAsync(LoginRequest request);
    }
}