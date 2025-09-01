using GraphQLSimple.Services;
using GraphQLSimple.Models;
using GraphQLSimple.Extensions;
using HotChocolate;
using HotChocolate.Types;
using System.Security.Claims;
using HotChocolate.Authorization;
using FluentValidation;
using Microsoft.EntityFrameworkCore;

namespace GraphQLSimple.GraphQL.Auth
{
    /// <summary>
    /// JWT Authentication mutations for user registration, login, and password management
    /// </summary>
    [ExtendObjectType<GraphQLSimple.GraphQL.Mutation>]
    public class JwtAuthMutation
    {
        /// <summary>
        /// Register a new user account
        /// </summary>
        public async Task<JwtAuthResult> Register(
            RegisterUserInput input,
            [Service] IJwtAuthenticationService authService,
            [Service] IValidator<RegisterUserInput> validator)
        {
            var validationResult = await validator.ValidateAsync(input);
            if (!validationResult.IsValid)
            {
                return new JwtAuthResult
                {
                    Success = false,
                    Message = string.Join("; ", validationResult.Errors.Select(e => e.ErrorMessage)),
                    ErrorCode = "VALIDATION_ERROR"
                };
            }

            return await authService.RegisterAsync(input);
        }

        /// <summary>
        /// Login with email and password
        /// </summary>
        public async Task<JwtAuthResult> Login(
            string email,
            string password,
            [Service] IJwtAuthenticationService authService)
        {
            if (string.IsNullOrWhiteSpace(email) || string.IsNullOrWhiteSpace(password))
            {
                return new JwtAuthResult
                {
                    Success = false,
                    Message = "Email and password are required",
                    ErrorCode = "MISSING_CREDENTIALS"
                };
            }

            return await authService.LoginAsync(email, password);
        }

        /// <summary>
        /// Request password reset (sends reset token to email)
        /// </summary>
        public async Task<bool> ForgotPassword(
            string email,
            [Service] IJwtAuthenticationService authService)
        {
            if (string.IsNullOrWhiteSpace(email))
            {
                return false;
            }

            return await authService.ForgotPasswordAsync(email);
        }

        /// <summary>
        /// Reset password using reset token
        /// </summary>
        public async Task<bool> ResetPassword(
            string token,
            string newPassword,
            [Service] IJwtAuthenticationService authService,
            [Service] IValidator<ResetPasswordInput> validator)
        {
            if (string.IsNullOrWhiteSpace(token) || string.IsNullOrWhiteSpace(newPassword))
            {
                return false;
            }

            var input = new ResetPasswordInput { Token = token, NewPassword = newPassword };
            var validationResult = await validator.ValidateAsync(input);
            if (!validationResult.IsValid)
            {
                return false;
            }

            return await authService.ResetPasswordAsync(token, newPassword);
        }

        /// <summary>
        /// Change password for authenticated user
        /// </summary>
        [Authorize]
        public async Task<bool> ChangePassword(
            string currentPassword,
            string newPassword,
            ClaimsPrincipal claimsPrincipal,
            [Service] IJwtAuthenticationService authService,
            [Service] IValidator<ChangePasswordInput> validator)
        {
            var userId = claimsPrincipal.GetUserId();
            if (userId == null)
            {
                return false;
            }

            var input = new ChangePasswordInput { CurrentPassword = currentPassword, NewPassword = newPassword };
            var validationResult = await validator.ValidateAsync(input);
            if (!validationResult.IsValid)
            {
                return false;
            }

            return await authService.ChangePasswordAsync(userId.Value, currentPassword, newPassword);
        }

        /// <summary>
        /// Refresh JWT token using refresh token
        /// </summary>
        [Authorize]
        public async Task<JwtAuthResult> RefreshToken(
            string refreshToken,
            ClaimsPrincipal claimsPrincipal,
            [Service] IJwtAuthenticationService authService,
            [Service] IUserService userService)
        {
            var userId = claimsPrincipal.GetUserId();
            if (userId == null || string.IsNullOrWhiteSpace(refreshToken))
            {
                return new JwtAuthResult
                {
                    Success = false,
                    Message = "Invalid refresh token or user",
                    ErrorCode = "INVALID_REFRESH_TOKEN"
                };
            }

            var isValidRefreshToken = await authService.ValidateRefreshTokenAsync(refreshToken, userId.Value);
            if (!isValidRefreshToken)
            {
                return new JwtAuthResult
                {
                    Success = false,
                    Message = "Refresh token is invalid or expired",
                    ErrorCode = "INVALID_REFRESH_TOKEN"
                };
            }

            // Get fresh user data
            var user = await userService.GetByIdAsync(userId.Value);
            if (user == null || !user.IsActive)
            {
                return new JwtAuthResult
                {
                    Success = false,
                    Message = "User account is not active",
                    ErrorCode = "ACCOUNT_INACTIVE"
                };
            }

            // Generate new tokens
            var roles = GetUserRoles(user);
            var permissions = GetUserPermissions(user, roles);
            var jwtToken = authService.GenerateJwtToken(user, roles, permissions);
            var newRefreshToken = authService.GenerateRefreshToken();

            // Revoke old refresh token and save new one
            await authService.RevokeRefreshTokenAsync(refreshToken);
            // Note: SaveRefreshTokenAsync would need to be made public or we need a different approach

            return new JwtAuthResult
            {
                Success = true,
                Message = "Token refreshed successfully",
                Token = jwtToken,
                RefreshToken = newRefreshToken,
                User = user,
                ExpiresAt = DateTime.UtcNow.AddHours(24)
            };
        }

        /// <summary>
        /// Logout (revokes refresh token)
        /// </summary>
        [Authorize]
        public async Task<bool> Logout(
            string refreshToken,
            [Service] IJwtAuthenticationService authService)
        {
            if (string.IsNullOrWhiteSpace(refreshToken))
            {
                return true; // Consider it successful if no token to revoke
            }

            await authService.RevokeRefreshTokenAsync(refreshToken);
            return true;
        }

        #region Private Helper Methods

        private static List<string> GetUserRoles(User user)
        {
            return user.UserType switch
            {
                UserType.Admin => new List<string> { "Admin", "Librarian", "Member" },
                UserType.Librarian => new List<string> { "Librarian", "Member" },
                UserType.Member => new List<string> { "Member" },
                _ => new List<string> { "Member" }
            };
        }

        private static List<string> GetUserPermissions(User user, List<string> roles)
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

        #endregion
    }

    /// <summary>
    /// JWT Authentication queries for user profile and authentication status
    /// </summary>
    [ExtendObjectType<GraphQLSimple.GraphQL.Query>]
    public class JwtAuthQuery
    {
        /// <summary>
        /// Get current authenticated user's profile
        /// </summary>
        [Authorize]
        public async Task<User?> Me(
            ClaimsPrincipal claimsPrincipal,
            [Service] IUserService userService)
        {
            var userId = claimsPrincipal.GetUserId();
            if (userId == null)
            {
                return null;
            }

            return await userService.GetByIdAsync(userId.Value);
        }

        /// <summary>
        /// Check if current user has specific permission
        /// </summary>
        [Authorize]
        public bool HasPermission(
            string permission,
            ClaimsPrincipal claimsPrincipal)
        {
            return claimsPrincipal.HasClaim("Permission", permission);
        }

        /// <summary>
        /// Get current user's roles
        /// </summary>
        [Authorize]
        public List<string> MyRoles(ClaimsPrincipal claimsPrincipal)
        {
            return claimsPrincipal.FindAll(ClaimTypes.Role)
                .Select(c => c.Value)
                .ToList();
        }

        /// <summary>
        /// Get current user's permissions
        /// </summary>
        [Authorize]
        public List<string> MyPermissions(ClaimsPrincipal claimsPrincipal)
        {
            return claimsPrincipal.FindAll("Permission")
                .Select(c => c.Value)
                .ToList();
        }
    }

    #region Input Types and Validators

    public class ResetPasswordInput
    {
        public string Token { get; set; } = string.Empty;
        public string NewPassword { get; set; } = string.Empty;
    }

    public class ChangePasswordInput
    {
        public string CurrentPassword { get; set; } = string.Empty;
        public string NewPassword { get; set; } = string.Empty;
    }

    public class RegisterUserInputValidator : AbstractValidator<RegisterUserInput>
    {
        public RegisterUserInputValidator()
        {
            RuleFor(x => x.FullName)
                .NotEmpty()
                .Length(2, 100)
                .WithMessage("Full name must be between 2 and 100 characters");

            RuleFor(x => x.Email)
                .NotEmpty()
                .EmailAddress()
                .WithMessage("A valid email address is required");

            RuleFor(x => x.Password)
                .NotEmpty()
                .MinimumLength(8)
                .Matches(@"^(?=.*[a-z])(?=.*[A-Z])(?=.*\d)(?=.*[@$!%*?&])[A-Za-z\d@$!%*?&]")
                .WithMessage("Password must be at least 8 characters and contain uppercase, lowercase, number and special character");

            RuleFor(x => x.ConfirmPassword)
                .NotEmpty()
                .Equal(x => x.Password)
                .WithMessage("Password and confirmation password must match");

            RuleFor(x => x.PhoneNumber)
                .Matches(@"^\+?[\d\s\-\(\)]+$")
                .When(x => !string.IsNullOrEmpty(x.PhoneNumber))
                .WithMessage("Please enter a valid phone number");
        }
    }

    public class ResetPasswordInputValidator : AbstractValidator<ResetPasswordInput>
    {
        public ResetPasswordInputValidator()
        {
            RuleFor(x => x.Token)
                .NotEmpty()
                .WithMessage("Reset token is required");

            RuleFor(x => x.NewPassword)
                .NotEmpty()
                .MinimumLength(8)
                .Matches(@"^(?=.*[a-z])(?=.*[A-Z])(?=.*\d)(?=.*[@$!%*?&])[A-Za-z\d@$!%*?&]")
                .WithMessage("Password must be at least 8 characters and contain uppercase, lowercase, number and special character");
        }
    }

    public class ChangePasswordInputValidator : AbstractValidator<ChangePasswordInput>
    {
        public ChangePasswordInputValidator()
        {
            RuleFor(x => x.CurrentPassword)
                .NotEmpty()
                .WithMessage("Current password is required");

            RuleFor(x => x.NewPassword)
                .NotEmpty()
                .MinimumLength(8)
                .Matches(@"^(?=.*[a-z])(?=.*[A-Z])(?=.*\d)(?=.*[@$!%*?&])[A-Za-z\d@$!%*?&]")
                .WithMessage("New password must be at least 8 characters and contain uppercase, lowercase, number and special character");

            RuleFor(x => x.NewPassword)
                .NotEqual(x => x.CurrentPassword)
                .WithMessage("New password must be different from current password");
        }
    }

    #endregion
}
