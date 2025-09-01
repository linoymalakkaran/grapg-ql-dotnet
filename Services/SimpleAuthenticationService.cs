using System.Security.Claims;
using GraphQLSimple.Models;
using GraphQLSimple.Services;
using GraphQLSimple.GraphQL.Types;
using HotChocolate.Authorization;
using HotChocolate;

namespace GraphQLSimple.GraphQL.Auth
{
    /// <summary>
    /// Simple authentication service for demo purposes
    /// In production, you would use proper JWT tokens and password hashing
    /// </summary>
    public interface IAuthenticationService
    {
        Task<AuthResult> LoginAsync(string email, string password);
        Task<AuthResult> RegisterAsync(CreateUserInput input, string password);
        Task<User?> GetCurrentUserAsync(ClaimsPrincipal principal);
    }

    public class SimpleAuthenticationService : IAuthenticationService
    {
        private readonly IUserService _userService;
        
        // Simple in-memory storage for demo (use proper database in production)
        private static readonly Dictionary<string, string> UserPasswords = new()
        {
            {"admin@library.com", "admin123"},
            {"john@example.com", "password123"},
            {"librarian@library.com", "librarian123"}
        };

        public SimpleAuthenticationService(IUserService userService)
        {
            _userService = userService;
        }

        public async Task<AuthResult> LoginAsync(string email, string password)
        {
            var user = await _userService.GetByEmailAsync(email);
            
            if (user == null || !user.IsActive)
            {
                return new AuthResult 
                { 
                    Success = false, 
                    Message = "Invalid email or password" 
                };
            }

            // Simple password check (use proper password hashing in production)
            if (!UserPasswords.TryGetValue(email.ToLower(), out var storedPassword) || 
                storedPassword != password)
            {
                return new AuthResult 
                { 
                    Success = false, 
                    Message = "Invalid email or password" 
                };
            }

            return new AuthResult
            {
                Success = true,
                User = user,
                Message = "Login successful"
            };
        }

        public async Task<AuthResult> RegisterAsync(CreateUserInput input, string password)
        {
            var existingUser = await _userService.GetByEmailAsync(input.Email);
            if (existingUser != null)
            {
                return new AuthResult 
                { 
                    Success = false, 
                    Message = "User with this email already exists" 
                };
            }

            var user = await _userService.CreateAsync(input);
            
            // Store password (use proper hashing in production)
            UserPasswords[input.Email.ToLower()] = password;

            return new AuthResult
            {
                Success = true,
                User = user,
                Message = "Registration successful"
            };
        }

        public async Task<User?> GetCurrentUserAsync(ClaimsPrincipal principal)
        {
            var emailClaim = principal.FindFirst(ClaimTypes.Email);
            if (emailClaim?.Value != null)
            {
                return await _userService.GetByEmailAsync(emailClaim.Value);
            }
            return null;
        }
    }

    /// <summary>
    /// Authentication mutations
    /// </summary>
    public class AuthMutation
    {
        /// <summary>
        /// Login with email and password
        /// </summary>
        public async Task<AuthPayload> Login(
            string email,
            string password,
            [Service] IAuthenticationService authService)
        {
            var result = await authService.LoginAsync(email, password);
            
            if (!result.Success || result.User == null)
            {
                throw new GraphQLException(result.Message);
            }

            return new AuthPayload
            {
                User = result.User,
                Message = result.Message,
                Success = true
            };
        }

        /// <summary>
        /// Register a new user
        /// </summary>
        public async Task<AuthPayload> Register(
            CreateUserInput input,
            string password,
            [Service] IAuthenticationService authService)
        {
            var result = await authService.RegisterAsync(input, password);
            
            if (!result.Success || result.User == null)
            {
                throw new GraphQLException(result.Message);
            }

            return new AuthPayload
            {
                User = result.User,
                Message = result.Message,
                Success = true
            };
        }

        /// <summary>
        /// Get current authenticated user
        /// </summary>
        [Authorize]
        public async Task<User?> Me(
            ClaimsPrincipal claimsPrincipal,
            [Service] IAuthenticationService authService)
        {
            return await authService.GetCurrentUserAsync(claimsPrincipal);
        }
    }

    /// <summary>
    /// Authentication result
    /// </summary>
    public class AuthResult
    {
        public bool Success { get; set; }
        public User? User { get; set; }
        public string Message { get; set; } = string.Empty;
    }

    /// <summary>
    /// Authentication response payload
    /// </summary>
    public class AuthPayload
    {
        public User User { get; set; } = null!;
        public string Message { get; set; } = string.Empty;
        public bool Success { get; set; }
    }
}

namespace GraphQLSimple.Extensions
{
    /// <summary>
    /// Authorization extensions and helpers
    /// </summary>
    public static class AuthorizationExtensions
    {
        /// <summary>
        /// Check if user has specific permission
        /// </summary>
        public static bool HasPermission(this ClaimsPrincipal user, string permission)
        {
            return user.HasClaim("Permission", permission);
        }

        /// <summary>
        /// Check if user can manage books (is librarian)
        /// </summary>
        public static bool CanManageBooks(this ClaimsPrincipal user)
        {
            return user.IsInRole("Librarian") || user.HasPermission("CanManageBooks");
        }

        /// <summary>
        /// Check if user can manage other users
        /// </summary>
        public static bool CanManageUsers(this ClaimsPrincipal user)
        {
            return user.IsInRole("Librarian") || user.HasPermission("CanManageUsers");
        }

        /// <summary>
        /// Get user ID from claims
        /// </summary>
        public static int? GetUserId(this ClaimsPrincipal user)
        {
            var userIdClaim = user.FindFirst(ClaimTypes.NameIdentifier);
            if (userIdClaim?.Value != null && int.TryParse(userIdClaim.Value, out int userId))
            {
                return userId;
            }
            return null;
        }
    }
}
