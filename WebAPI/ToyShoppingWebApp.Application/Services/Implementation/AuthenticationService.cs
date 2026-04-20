namespace ToyShoppingWebApp.Application.Services.Implementations
{
    using System.IdentityModel.Tokens.Jwt;
    using System.Security.Cryptography;
    using System.Text;
    using Microsoft.Extensions.Configuration;
    using Microsoft.IdentityModel.Tokens;
    using ToyShoppingWebApp.Application.DTOs;
    using ToyShoppingWebApp.Application.Repositories.Interfaces;
    using ToyShoppingWebApp.Application.Services.Interfaces;
    using ToyShoppingWebApp.Domain.Entities;

    public class AuthenticationService : IAuthenticationService
    {
        private readonly IUserRepository _userRepository;
        private readonly IConfiguration _configuration;

        public AuthenticationService(
            IUserRepository userRepository,
            IConfiguration configuration)
        {
            _userRepository = userRepository;
            _configuration = configuration;
        }

        public async Task<AuthResponse> RegisterAsync(RegisterRequest request)
        {
            // Validate input
            if (string.IsNullOrWhiteSpace(request.Email) || string.IsNullOrWhiteSpace(request.Password))
                return new AuthResponse
                {
                    Success = false,
                    Message = "Email and password are required"
                };

            if (request.Password != request.ConfirmPassword)
                return new AuthResponse
                {
                    Success = false,
                    Message = "Passwords do not match"
                };

            if (request.Password.Length < 8)
                return new AuthResponse
                {
                    Success = false,
                    Message = "Password must be at least 8 characters"
                };

            // Check if user already exists
            var existingUser = await _userRepository.GetUserByEmailAsync(request.Email);
            if (existingUser != null)
                return new AuthResponse
                {
                    Success = false,
                    Message = "User with this email already exists"
                };

            // Hash password
            var passwordHash = HashPassword(request.Password);

            // Create user
            var user = new User
            {
                Name = request.Name,
                Email = request.Email,
                PasswordHash = passwordHash,
                Role = "Customer", // New users are customers by default
                IsActive = true,
                CreatedDate = DateTime.UtcNow
            };

            var createdUser = await _userRepository.CreateUserAsync(user);

            // Generate JWT token
            var token = GenerateJwtToken(createdUser);

            return new AuthResponse
            {
                Success = true,
                Message = "User registered successfully",
                Token = token,
                User = new UserDto
                {
                    Id = createdUser.Id,
                    Email = createdUser.Email,
                    Role = createdUser.Role
                }
            };
        }

        public async Task<AuthResponse> LoginAsync(LoginRequest request)
        {
            // Validate input
            if (string.IsNullOrWhiteSpace(request.Email) || string.IsNullOrWhiteSpace(request.Password))
                return new AuthResponse
                {
                    Success = false,
                    Message = "Email and password are required"
                };

            // Find user by email
            var user = await _userRepository.GetUserByEmailAsync(request.Email);
            if (user == null)
                return new AuthResponse
                {
                    Success = false,
                    Message = "Invalid email or password"
                };

            // Verify password
            if (!VerifyPassword(request.Password, user.PasswordHash))
                return new AuthResponse
                {
                    Success = false,
                    Message = "Invalid email or password"
                };

            // Generate JWT token
            var token = GenerateJwtToken(user);

            return new AuthResponse
            {
                Success = true,
                Message = "Login successful",
                Token = token,
                User = new UserDto
                {
                    Id = user.Id,
                    Email = user.Email,
                    Role = user.Role
                }
            };
        }

        /// <summary>
        /// Hash password using PBKDF2 (secure, industry-standard)
        /// </summary>
        private string HashPassword(string password)
        {
            using (var rng = new Rfc2898DeriveBytes(password, 16, 10000, HashAlgorithmName.SHA256))
            {
                var salt = rng.Salt;
                var hash = rng.GetBytes(32);

                // Combine salt + hash for storage
                var hashWithSalt = new byte[salt.Length + hash.Length];
                Array.Copy(salt, 0, hashWithSalt, 0, salt.Length);
                Array.Copy(hash, 0, hashWithSalt, salt.Length, hash.Length);

                return Convert.ToBase64String(hashWithSalt);
            }
        }

        /// <summary>
        /// Verify password against hash
        /// </summary>
        private bool VerifyPassword(string password, string hash)
        {
            var hashWithSalt = Convert.FromBase64String(hash);
            var salt = new byte[16];
            Array.Copy(hashWithSalt, 0, salt, 0, 16);

            using (var rng = new Rfc2898DeriveBytes(password, salt, 10000, HashAlgorithmName.SHA256))
            {
                var computedHash = rng.GetBytes(32);
                for (int i = 0; i < 32; i++)
                {
                    if (hashWithSalt[i + 16] != computedHash[i])
                        return false;
                }
            }

            return true;
        }

        /// <summary>
        /// Generate JWT token
        /// </summary>
        private string GenerateJwtToken(User user)
        {
            var jwtSettings = _configuration.GetSection("JwtSettings");
            var secret = jwtSettings["Secret"];
            var issuer = jwtSettings["Issuer"];
            var audience = jwtSettings["Audience"];
            var expiryMinutes = int.Parse(jwtSettings["ExpiryMinutes"] ?? "60");

            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secret!));
            var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var claims = new[]
            {
                new System.Security.Claims.Claim(System.Security.Claims.ClaimTypes.NameIdentifier, user.Id.ToString()),
                new System.Security.Claims.Claim(System.Security.Claims.ClaimTypes.Email, user.Email),
                new System.Security.Claims.Claim(System.Security.Claims.ClaimTypes.Role, user.Role)
            };

            var token = new JwtSecurityToken(
                issuer: issuer,
                audience: audience,
                claims: claims,
                expires: DateTime.UtcNow.AddMinutes(expiryMinutes),
                signingCredentials: credentials
            );

            return new JwtSecurityTokenHandler().WriteToken(token);
        }
    }
}