using System.Security.Claims;
using GraphQLSimple.Models;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Text;

namespace GraphQLSimple.Services
{
    /// <summary>
    /// JWT Token Service for authentication
    /// </summary>
    public interface IJwtTokenService
    {
        string GenerateToken(User user);
        ClaimsPrincipal? ValidateToken(string token);
        Task<User?> GetUserFromTokenAsync(string token);
    }

    public class JwtTokenService : IJwtTokenService
    {
        private readonly IConfiguration _configuration;
        private readonly IUserService _userService;
        private readonly string _secretKey;
        private readonly string _issuer;
        private readonly string _audience;

        public JwtTokenService(IConfiguration configuration, IUserService userService)
        {
            _configuration = configuration;
            _userService = userService;
            _secretKey = _configuration["JWT:SecretKey"] ?? "your-super-secret-key-that-is-at-least-32-characters-long";
            _issuer = _configuration["JWT:Issuer"] ?? "GraphQLLibrary";
            _audience = _configuration["JWT:Audience"] ?? "GraphQLLibraryUsers";
        }

        public string GenerateToken(User user)
        {
            var tokenHandler = new JwtSecurityTokenHandler();
            var key = Encoding.ASCII.GetBytes(_secretKey);
            
            var claims = new List<Claim>
            {
                new(ClaimTypes.NameIdentifier, user.Id.ToString()),
                new(ClaimTypes.Name, user.FullName),
                new(ClaimTypes.Email, user.Email),
                new("UserType", user.UserType.ToString()),
                new("IsActive", user.IsActive.ToString())
            };

            // Add role-based claims
            if (user.UserType == UserType.Librarian)
            {
                claims.Add(new Claim(ClaimTypes.Role, "Librarian"));
                claims.Add(new Claim("Permission", "CanManageBooks"));
                claims.Add(new Claim("Permission", "CanManageUsers"));
                claims.Add(new Claim("Permission", "CanViewReports"));
            }
            else
            {
                claims.Add(new Claim(ClaimTypes.Role, "Member"));
                claims.Add(new Claim("Permission", "CanBorrowBooks"));
                claims.Add(new Claim("Permission", "CanWriteReviews"));
            }

            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(claims),
                Expires = DateTime.UtcNow.AddHours(24), // Token expires in 24 hours
                Issuer = _issuer,
                Audience = _audience,
                SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature)
            };

            var token = tokenHandler.CreateToken(tokenDescriptor);
            return tokenHandler.WriteToken(token);
        }

        public ClaimsPrincipal? ValidateToken(string token)
        {
            try
            {
                var tokenHandler = new JwtSecurityTokenHandler();
                var key = Encoding.ASCII.GetBytes(_secretKey);

                var validationParameters = new TokenValidationParameters
                {
                    ValidateIssuerSigningKey = true,
                    IssuerSigningKey = new SymmetricSecurityKey(key),
                    ValidateIssuer = true,
                    ValidIssuer = _issuer,
                    ValidateAudience = true,
                    ValidAudience = _audience,
                    ValidateLifetime = true,
                    ClockSkew = TimeSpan.Zero
                };

                var principal = tokenHandler.ValidateToken(token, validationParameters, out SecurityToken validatedToken);
                return principal;
            }
            catch
            {
                return null;
            }
        }

        public async Task<User?> GetUserFromTokenAsync(string token)
        {
            var principal = ValidateToken(token);
            if (principal?.FindFirst(ClaimTypes.NameIdentifier)?.Value is string userIdStr &&
                int.TryParse(userIdStr, out int userId))
            {
                return await _userService.GetByIdAsync(userId);
            }
            return null;
        }
    }
}

namespace GraphQLSimple.GraphQL.Auth
{
    /// <summary>
    /// Authentication mutations for login/logout
    /// </summary>
    public class AuthMutation
    {
        /// <summary>
        /// Login a user with email and password
        /// </summary>
        public async Task<AuthPayload> Login(
            string email,
            string password,
            [Service] IUserService userService,
            [Service] IJwtTokenService jwtService)
        {
            // In a real application, you would validate the password against a hashed version
            // For this demo, we'll do a simple check
            var user = await userService.GetByEmailAsync(email);
            
            if (user == null || !user.IsActive)
            {
                throw new GraphQLException("Invalid email or password");
            }

            // In production: await passwordService.VerifyPassword(password, user.PasswordHash)
            // For demo purposes, we'll accept any password for existing users
            
            var token = jwtService.GenerateToken(user);
            
            return new AuthPayload
            {
                Token = token,
                User = user,
                ExpiresAt = DateTime.UtcNow.AddHours(24)
            };
        }

        /// <summary>
        /// Register a new user
        /// </summary>
        public async Task<AuthPayload> Register(
            CreateUserInput input,
            string password,
            [Service] IUserService userService,
            [Service] IJwtTokenService jwtService)
        {
            // Check if user already exists
            var existingUser = await userService.GetByEmailAsync(input.Email);
            if (existingUser != null)
            {
                throw new GraphQLException("User with this email already exists");
            }

            // Create new user
            var user = await userService.CreateAsync(input);
            
            // Generate token
            var token = jwtService.GenerateToken(user);
            
            return new AuthPayload
            {
                Token = token,
                User = user,
                ExpiresAt = DateTime.UtcNow.AddHours(24)
            };
        }

        /// <summary>
        /// Refresh an existing token
        /// </summary>
        public async Task<AuthPayload> RefreshToken(
            [Service] IJwtTokenService jwtService,
            ClaimsPrincipal claimsPrincipal)
        {
            var userIdClaim = claimsPrincipal.FindFirst(ClaimTypes.NameIdentifier);
            if (userIdClaim == null || !int.TryParse(userIdClaim.Value, out int userId))
            {
                throw new GraphQLException("Invalid token");
            }

            // Get fresh user data
            var userService = default(IUserService); // You'd inject this
            var user = await userService!.GetByIdAsync(userId);
            
            if (user == null || !user.IsActive)
            {
                throw new GraphQLException("User not found or inactive");
            }

            var newToken = jwtService.GenerateToken(user);
            
            return new AuthPayload
            {
                Token = newToken,
                User = user,
                ExpiresAt = DateTime.UtcNow.AddHours(24)
            };
        }
    }

    /// <summary>
    /// Authentication response payload
    /// </summary>
    public class AuthPayload
    {
        public string Token { get; set; } = string.Empty;
        public User User { get; set; } = null!;
        public DateTime ExpiresAt { get; set; }
    }
}

namespace GraphQLSimple.GraphQL.Auth
{
    /// <summary>
    /// Custom authorization attributes for GraphQL
    /// </summary>
    public class RequireAuthenticationAttribute : Attribute
    {
    }

    public class RequireRoleAttribute : Attribute
    {
        public string Role { get; }
        
        public RequireRoleAttribute(string role)
        {
            Role = role;
        }
    }

    public class RequirePermissionAttribute : Attribute
    {
        public string Permission { get; }
        
        public RequirePermissionAttribute(string permission)
        {
            Permission = permission;
        }
    }
}

namespace GraphQLSimple.Extensions
{
    /// <summary>
    /// GraphQL Authorization Service
    /// </summary>
    public interface IGraphQLAuthorizationService
    {
        Task<bool> IsAuthorizedAsync(ClaimsPrincipal user, string permission);
        Task<bool> HasRoleAsync(ClaimsPrincipal user, string role);
        Task<bool> CanAccessResourceAsync(ClaimsPrincipal user, string resource, int resourceId);
    }

    public class GraphQLAuthorizationService : IGraphQLAuthorizationService
    {
        public Task<bool> IsAuthorizedAsync(ClaimsPrincipal user, string permission)
        {
            var hasPermission = user.HasClaim("Permission", permission);
            return Task.FromResult(hasPermission);
        }

        public Task<bool> HasRoleAsync(ClaimsPrincipal user, string role)
        {
            var hasRole = user.IsInRole(role);
            return Task.FromResult(hasRole);
        }

        public async Task<bool> CanAccessResourceAsync(ClaimsPrincipal user, string resource, int resourceId)
        {
            // Custom business logic for resource access
            switch (resource.ToLower())
            {
                case "borrowing":
                    // Users can only access their own borrowings
                    var userIdStr = user.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                    if (int.TryParse(userIdStr, out int userId))
                    {
                        // You would check if the borrowing belongs to this user
                        // For demo, we'll allow if it's the same user or if they're a librarian
                        return userId == resourceId || user.IsInRole("Librarian");
                    }
                    break;
                
                case "user":
                    // Users can access their own profile, librarians can access any
                    var currentUserIdStr = user.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                    if (int.TryParse(currentUserIdStr, out int currentUserId))
                    {
                        return currentUserId == resourceId || user.IsInRole("Librarian");
                    }
                    break;
            }
            
            return await Task.FromResult(false);
        }
    }
}
