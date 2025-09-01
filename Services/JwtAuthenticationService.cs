using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.IdentityModel.Tokens;
using Microsoft.EntityFrameworkCore;
using GraphQLSimple.Models;
using GraphQLSimple.Data;
using GraphQLSimple.GraphQL.Types;
using BC = BCrypt.Net.BCrypt;

namespace GraphQLSimple.Services
{
    public interface IJwtAuthenticationService
    {
        Task<JwtAuthResult> LoginAsync(string email, string password);
        Task<JwtAuthResult> RegisterAsync(RegisterUserInput input);
        Task<bool> ForgotPasswordAsync(string email);
        Task<bool> ResetPasswordAsync(string token, string newPassword);
        Task<bool> ChangePasswordAsync(int userId, string currentPassword, string newPassword);
        Task<bool> RefreshTokenAsync(string refreshToken, int userId);
        string GenerateJwtToken(User user, List<string> roles, List<string> permissions);
        string GenerateRefreshToken();
        Task<bool> ValidateRefreshTokenAsync(string refreshToken, int userId);
        Task RevokeRefreshTokenAsync(string refreshToken);
    }

    public class JwtAuthenticationService : IJwtAuthenticationService
    {
        private readonly IUserService _userService;
        private readonly IConfiguration _configuration;
        private readonly LibraryContext _context;
        private readonly ILogger<JwtAuthenticationService> _logger;

        public JwtAuthenticationService(
            IUserService userService, 
            IConfiguration configuration,
            LibraryContext context,
            ILogger<JwtAuthenticationService> logger)
        {
            _userService = userService;
            _configuration = configuration;
            _context = context;
            _logger = logger;
        }

        public async Task<JwtAuthResult> LoginAsync(string email, string password)
        {
            try
            {
                var user = await _userService.GetByEmailAsync(email);
                if (user == null)
                {
                    _logger.LogWarning("Login attempt with non-existent email: {Email}", email);
                    return new JwtAuthResult 
                    { 
                        Success = false, 
                        Message = "Invalid email or password",
                        ErrorCode = "INVALID_CREDENTIALS"
                    };
                }

                if (!user.IsActive)
                {
                    _logger.LogWarning("Login attempt for inactive user: {Email}", email);
                    return new JwtAuthResult 
                    { 
                        Success = false, 
                        Message = "Account is deactivated. Please contact administrator.",
                        ErrorCode = "ACCOUNT_DEACTIVATED"
                    };
                }

                if (!BC.Verify(password, user.PasswordHash))
                {
                    _logger.LogWarning("Invalid password attempt for user: {Email}", email);
                    await LogFailedLoginAttempt(user.Id);
                    return new JwtAuthResult 
                    { 
                        Success = false, 
                        Message = "Invalid email or password",
                        ErrorCode = "INVALID_CREDENTIALS"
                    };
                }

                // Check if account is locked due to too many failed attempts
                if (await IsAccountLockedAsync(user.Id))
                {
                    return new JwtAuthResult 
                    { 
                        Success = false, 
                        Message = "Account is temporarily locked due to too many failed login attempts. Try again later.",
                        ErrorCode = "ACCOUNT_LOCKED"
                    };
                }

                var roles = GetUserRoles(user);
                var permissions = GetUserPermissions(user, roles);
                
                var jwtToken = GenerateJwtToken(user, roles, permissions);
                var refreshToken = GenerateRefreshToken();

                await SaveRefreshTokenAsync(user.Id, refreshToken);
                await ClearFailedLoginAttemptsAsync(user.Id);

                _logger.LogInformation("Successful login for user: {Email}", email);

                return new JwtAuthResult
                {
                    Success = true,
                    Message = "Login successful",
                    Token = jwtToken,
                    RefreshToken = refreshToken,
                    User = user,
                    ExpiresAt = DateTime.UtcNow.AddHours(24)
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during login for user: {Email}", email);
                return new JwtAuthResult 
                { 
                    Success = false, 
                    Message = "An error occurred during login",
                    ErrorCode = "INTERNAL_ERROR"
                };
            }
        }

        public async Task<JwtAuthResult> RegisterAsync(RegisterUserInput input)
        {
            try
            {
                // Check if user already exists
                var existingUser = await _userService.GetByEmailAsync(input.Email);
                if (existingUser != null)
                {
                    return new JwtAuthResult 
                    { 
                        Success = false, 
                        Message = "User with this email already exists",
                        ErrorCode = "USER_EXISTS"
                    };
                }

                // Hash password
                var passwordHash = BC.HashPassword(input.Password);

                var createUserInput = new CreateUserInput
                {
                    FirstName = input.FullName.Split(' ').FirstOrDefault() ?? "",
                    LastName = string.Join(" ", input.FullName.Split(' ').Skip(1)),
                    Email = input.Email,
                    PasswordHash = passwordHash,
                    UserType = input.UserType ?? UserType.Member,
                    PhoneNumber = input.PhoneNumber,
                    Address = input.Address,
                    DateOfBirth = input.DateOfBirth ?? DateTime.Now.AddYears(-18)
                };

                var user = await _userService.CreateAsync(createUserInput);
                
                var roles = GetUserRoles(user);
                var permissions = GetUserPermissions(user, roles);
                
                var jwtToken = GenerateJwtToken(user, roles, permissions);
                var refreshToken = GenerateRefreshToken();

                await SaveRefreshTokenAsync(user.Id, refreshToken);

                _logger.LogInformation("New user registered: {Email}", input.Email);

                return new JwtAuthResult
                {
                    Success = true,
                    Message = "Registration successful",
                    Token = jwtToken,
                    RefreshToken = refreshToken,
                    User = user,
                    ExpiresAt = DateTime.UtcNow.AddHours(24)
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during registration for user: {Email}", input.Email);
                return new JwtAuthResult 
                { 
                    Success = false, 
                    Message = "An error occurred during registration",
                    ErrorCode = "INTERNAL_ERROR"
                };
            }
        }

        public async Task<bool> ForgotPasswordAsync(string email)
        {
            try
            {
                var user = await _userService.GetByEmailAsync(email);
                if (user == null)
                {
                    // Don't reveal if user exists or not
                    _logger.LogWarning("Password reset requested for non-existent email: {Email}", email);
                    return true; // Return true to avoid revealing user existence
                }

                var resetToken = GeneratePasswordResetToken();
                await SavePasswordResetTokenAsync(user.Id, resetToken);

                // TODO: Send email with reset token
                // await _emailService.SendPasswordResetEmailAsync(user.Email, resetToken);
                
                _logger.LogInformation("Password reset token generated for user: {Email}", email);
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during password reset for email: {Email}", email);
                return false;
            }
        }

        public async Task<bool> ResetPasswordAsync(string token, string newPassword)
        {
            try
            {
                var userId = await ValidatePasswordResetTokenAsync(token);
                if (userId == null)
                {
                    return false;
                }

                var passwordHash = BC.HashPassword(newPassword);
                
                // Update user password
                var user = await _userService.GetByIdAsync(userId.Value);
                if (user == null) return false;

                var updateInput = new UpdateUserInput
                {
                    Id = user.Id,
                    FirstName = user.FirstName,
                    LastName = user.LastName,
                    Email = user.Email,
                    PasswordHash = passwordHash,
                    UserType = user.UserType,
                    IsActive = user.IsActive,
                    PhoneNumber = user.PhoneNumber,
                    Address = user.Address,
                    DateOfBirth = user.DateOfBirth
                };

                await _userService.UpdateAsync(updateInput);
                await InvalidatePasswordResetTokenAsync(token);
                await RevokeAllUserRefreshTokensAsync(userId.Value);

                _logger.LogInformation("Password reset successful for user ID: {UserId}", userId);
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during password reset with token: {Token}", token);
                return false;
            }
        }

        public async Task<bool> ChangePasswordAsync(int userId, string currentPassword, string newPassword)
        {
            try
            {
                var user = await _userService.GetByIdAsync(userId);
                if (user == null || !BC.Verify(currentPassword, user.PasswordHash))
                {
                    return false;
                }

                var passwordHash = BC.HashPassword(newPassword);
                
                var updateInput = new UpdateUserInput
                {
                    Id = user.Id,
                    FirstName = user.FirstName,
                    LastName = user.LastName,
                    Email = user.Email,
                    PasswordHash = passwordHash,
                    UserType = user.UserType,
                    IsActive = user.IsActive,
                    PhoneNumber = user.PhoneNumber,
                    Address = user.Address,
                    DateOfBirth = user.DateOfBirth
                };

                await _userService.UpdateAsync(updateInput);

                _logger.LogInformation("Password changed for user ID: {UserId}", userId);
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during password change for user ID: {UserId}", userId);
                return false;
            }
        }

        public async Task<bool> RefreshTokenAsync(string refreshToken, int userId)
        {
            return await ValidateRefreshTokenAsync(refreshToken, userId);
        }

        public string GenerateJwtToken(User user, List<string> roles, List<string> permissions)
        {
            var jwtSettings = _configuration.GetSection("JwtSettings");
            var secretKey = jwtSettings["SecretKey"] ?? throw new ArgumentNullException("JwtSettings:SecretKey");
            var key = Encoding.ASCII.GetBytes(secretKey);

            var claims = new List<Claim>
            {
                new(ClaimTypes.NameIdentifier, user.Id.ToString()),
                new(ClaimTypes.Name, user.FullName),
                new(ClaimTypes.Email, user.Email),
                new("UserType", user.UserType.ToString()),
                new("IsActive", user.IsActive.ToString())
            };

            // Add role claims
            foreach (var role in roles)
            {
                claims.Add(new Claim(ClaimTypes.Role, role));
            }

            // Add permission claims
            foreach (var permission in permissions)
            {
                claims.Add(new Claim("Permission", permission));
            }

            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(claims),
                Expires = DateTime.UtcNow.AddHours(int.Parse(jwtSettings["ExpiryInHours"] ?? "24")),
                SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature),
                Issuer = jwtSettings["Issuer"],
                Audience = jwtSettings["Audience"]
            };

            var tokenHandler = new JwtSecurityTokenHandler();
            var token = tokenHandler.CreateToken(tokenDescriptor);
            return tokenHandler.WriteToken(token);
        }

        public string GenerateRefreshToken()
        {
            return Guid.NewGuid().ToString() + DateTime.UtcNow.Ticks.ToString();
        }

        public async Task<bool> ValidateRefreshTokenAsync(string refreshToken, int userId)
        {
            var tokenRecord = await _context.RefreshTokens
                .FirstOrDefaultAsync(rt => rt.Token == refreshToken && rt.UserId == userId && rt.IsActive);

            return tokenRecord != null && tokenRecord.ExpiryDate > DateTime.UtcNow;
        }

        public async Task RevokeRefreshTokenAsync(string refreshToken)
        {
            var tokenRecord = await _context.RefreshTokens
                .FirstOrDefaultAsync(rt => rt.Token == refreshToken);

            if (tokenRecord != null)
            {
                tokenRecord.IsActive = false;
                tokenRecord.RevokedDate = DateTime.UtcNow;
                await _context.SaveChangesAsync();
            }
        }

        #region Private Helper Methods

        private List<string> GetUserRoles(User user)
        {
            return user.UserType switch
            {
                UserType.Admin => new List<string> { "Admin", "Librarian", "Member" },
                UserType.Librarian => new List<string> { "Librarian", "Member" },
                UserType.Member => new List<string> { "Member" },
                _ => new List<string> { "Member" }
            };
        }

        private List<string> GetUserPermissions(User user, List<string> roles)
        {
            var permissions = new List<string>();

            if (roles.Contains("Admin"))
            {
                permissions.AddRange(new[]
                {
                    "CanManageUsers", "CanManageBooks", "CanManageAuthors", 
                    "CanManageBorrowings", "CanViewReports", "CanManageSystem"
                });
            }
            else if (roles.Contains("Librarian"))
            {
                permissions.AddRange(new[]
                {
                    "CanManageBooks", "CanManageAuthors", "CanManageBorrowings", "CanViewReports"
                });
            }
            else if (roles.Contains("Member"))
            {
                permissions.AddRange(new[]
                {
                    "CanBorrowBooks", "CanReturnBooks", "CanCreateReviews", "CanViewBooks"
                });
            }

            return permissions;
        }

        private async Task LogFailedLoginAttempt(int userId)
        {
            var attempt = new LoginAttempt
            {
                UserId = userId,
                AttemptDate = DateTime.UtcNow,
                IsSuccessful = false,
                IpAddress = "Unknown" // TODO: Get actual IP address from HttpContext
            };

            _context.LoginAttempts.Add(attempt);
            await _context.SaveChangesAsync();
        }

        private async Task<bool> IsAccountLockedAsync(int userId)
        {
            var lockoutMinutes = 30; // Account locked for 30 minutes
            var maxAttempts = 5; // Max 5 failed attempts

            var recentFailedAttempts = await _context.LoginAttempts
                .CountAsync(la => la.UserId == userId && 
                                  !la.IsSuccessful && 
                                  la.AttemptDate > DateTime.UtcNow.AddMinutes(-lockoutMinutes));

            return recentFailedAttempts >= maxAttempts;
        }

        private async Task ClearFailedLoginAttemptsAsync(int userId)
        {
            var failedAttempts = await _context.LoginAttempts
                .Where(la => la.UserId == userId && !la.IsSuccessful)
                .ToListAsync();

            _context.LoginAttempts.RemoveRange(failedAttempts);
            await _context.SaveChangesAsync();
        }

        private async Task SaveRefreshTokenAsync(int userId, string refreshToken)
        {
            var tokenRecord = new RefreshToken
            {
                UserId = userId,
                Token = refreshToken,
                ExpiryDate = DateTime.UtcNow.AddDays(30),
                IsActive = true,
                CreatedDate = DateTime.UtcNow
            };

            _context.RefreshTokens.Add(tokenRecord);
            await _context.SaveChangesAsync();
        }

        private string GeneratePasswordResetToken()
        {
            return Guid.NewGuid().ToString() + DateTime.UtcNow.Ticks.ToString();
        }

        private async Task SavePasswordResetTokenAsync(int userId, string token)
        {
            var resetToken = new PasswordResetToken
            {
                UserId = userId,
                Token = token,
                ExpiryDate = DateTime.UtcNow.AddHours(2), // Valid for 2 hours
                IsUsed = false,
                CreatedDate = DateTime.UtcNow
            };

            _context.PasswordResetTokens.Add(resetToken);
            await _context.SaveChangesAsync();
        }

        private async Task<int?> ValidatePasswordResetTokenAsync(string token)
        {
            var resetToken = await _context.PasswordResetTokens
                .FirstOrDefaultAsync(prt => prt.Token == token && 
                                            !prt.IsUsed && 
                                            prt.ExpiryDate > DateTime.UtcNow);

            return resetToken?.UserId;
        }

        private async Task InvalidatePasswordResetTokenAsync(string token)
        {
            var resetToken = await _context.PasswordResetTokens
                .FirstOrDefaultAsync(prt => prt.Token == token);

            if (resetToken != null)
            {
                resetToken.IsUsed = true;
                resetToken.UsedDate = DateTime.UtcNow;
                await _context.SaveChangesAsync();
            }
        }

        private async Task RevokeAllUserRefreshTokensAsync(int userId)
        {
            var userTokens = await _context.RefreshTokens
                .Where(rt => rt.UserId == userId && rt.IsActive)
                .ToListAsync();

            foreach (var token in userTokens)
            {
                token.IsActive = false;
                token.RevokedDate = DateTime.UtcNow;
            }

            await _context.SaveChangesAsync();
        }

        #endregion
    }

    #region DTOs and Models

    public class JwtAuthResult
    {
        public bool Success { get; set; }
        public string Message { get; set; } = string.Empty;
        public string? ErrorCode { get; set; }
        public string? Token { get; set; }
        public string? RefreshToken { get; set; }
        public User? User { get; set; }
        public DateTime? ExpiresAt { get; set; }
    }

    public class RegisterUserInput
    {
        public string FullName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
        public string ConfirmPassword { get; set; } = string.Empty;
        public UserType? UserType { get; set; }
        public string? PhoneNumber { get; set; }
        public string? Address { get; set; }
        public DateTime? DateOfBirth { get; set; }
    }

    public class RefreshToken
    {
        public int Id { get; set; }
        public int UserId { get; set; }
        public string Token { get; set; } = string.Empty;
        public DateTime ExpiryDate { get; set; }
        public bool IsActive { get; set; }
        public DateTime CreatedDate { get; set; }
        public DateTime? RevokedDate { get; set; }
        public User User { get; set; } = null!;
    }

    public class PasswordResetToken
    {
        public int Id { get; set; }
        public int UserId { get; set; }
        public string Token { get; set; } = string.Empty;
        public DateTime ExpiryDate { get; set; }
        public bool IsUsed { get; set; }
        public DateTime CreatedDate { get; set; }
        public DateTime? UsedDate { get; set; }
        public User User { get; set; } = null!;
    }

    public class LoginAttempt
    {
        public int Id { get; set; }
        public int UserId { get; set; }
        public DateTime AttemptDate { get; set; }
        public bool IsSuccessful { get; set; }
        public string IpAddress { get; set; } = string.Empty;
        public User User { get; set; } = null!;
    }

    #endregion
}
