using GraphQLSimple.Models;
using GraphQLSimple.Services;
using GraphQLSimple.GraphQL.Types;
using GraphQLSimple.Extensions;
using HotChocolate.Authorization;
using HotChocolate;
using System.Security.Claims;

namespace GraphQLSimple.GraphQL
{
    /// <summary>
    /// Authorized mutations that require authentication and/or specific roles
    /// </summary>
    public partial class Mutation
    {
        /// <summary>
        /// Create a new book (Librarian only)
        /// </summary>
        [Authorize(Roles = new[] { "Librarian" })]
        public async Task<Book> CreateBookAuthorized(
            CreateBookInput input,
            [Service] BookService bookService,
            ClaimsPrincipal claimsPrincipal)
        {
            // Additional authorization check
            if (!claimsPrincipal.CanManageBooks())
            {
                throw new GraphQLException("You don't have permission to create books");
            }

            return await bookService.CreateAsync(input);
        }

        /// <summary>
        /// Update book (Librarian only)
        /// </summary>
        [Authorize(Roles = new[] { "Librarian" })]
        public async Task<Book?> UpdateBookAuthorized(
            UpdateBookInput input,
            [Service] BookService bookService,
            ClaimsPrincipal claimsPrincipal)
        {
            if (!claimsPrincipal.CanManageBooks())
            {
                throw new GraphQLException("You don't have permission to update books");
            }

            return await bookService.UpdateAsync(input);
        }

        /// <summary>
        /// Delete book (Librarian only)
        /// </summary>
        [Authorize(Roles = new[] { "Librarian" })]
        public async Task<bool> DeleteBookAuthorized(
            int id,
            [Service] BookService bookService,
            ClaimsPrincipal claimsPrincipal)
        {
            if (!claimsPrincipal.CanManageBooks())
            {
                throw new GraphQLException("You don't have permission to delete books");
            }

            return await bookService.DeleteAsync(id);
        }

        /// <summary>
        /// Borrow a book (Authenticated users only)
        /// </summary>
        [Authorize]
        public async Task<Borrowing?> BorrowBookAuthorized(
            int bookId,
            [Service] BorrowingService borrowingService,
            ClaimsPrincipal claimsPrincipal)
        {
            var userId = claimsPrincipal.GetUserId();
            if (!userId.HasValue)
            {
                throw new GraphQLException("User not found");
            }

            var input = new CreateBorrowingInput
            {
                BookId = bookId,
                UserId = userId.Value,
                BorrowedDate = DateTime.UtcNow,
                DueDate = DateTime.UtcNow.AddDays(14), // 2 weeks loan
                Status = BorrowingStatus.Active
            };

            return await borrowingService.CreateAsync(input);
        }

        /// <summary>
        /// Return a book (User can only return their own books)
        /// </summary>
        [Authorize]
        public async Task<Borrowing?> ReturnBookAuthorized(
            int borrowingId,
            [Service] BorrowingService borrowingService,
            ClaimsPrincipal claimsPrincipal)
        {
            var userId = claimsPrincipal.GetUserId();
            if (!userId.HasValue)
            {
                throw new GraphQLException("User not found");
            }

            // Check if user owns this borrowing or is a librarian
            var borrowing = await borrowingService.GetByIdAsync(borrowingId);
            if (borrowing == null)
            {
                throw new GraphQLException("Borrowing not found");
            }

            if (borrowing.UserId != userId.Value && !claimsPrincipal.IsInRole("Librarian"))
            {
                throw new GraphQLException("You can only return your own books");
            }

            return await borrowingService.ReturnBookAsync(borrowingId);
        }

        /// <summary>
        /// Create a review (Authenticated users only)
        /// </summary>
        [Authorize]
        public async Task<Review> CreateReviewAuthorized(
            CreateReviewInput input,
            [Service] IReviewService reviewService,
            ClaimsPrincipal claimsPrincipal)
        {
            var userId = claimsPrincipal.GetUserId();
            if (!userId.HasValue)
            {
                throw new GraphQLException("User not found");
            }

            // Override the user ID with the authenticated user's ID
            var reviewInput = new CreateReviewInput
            {
                BookId = input.BookId,
                UserId = userId.Value,
                Rating = input.Rating,
                Comment = input.Comment,
                IsVerifiedPurchase = input.IsVerifiedPurchase
            };

            return await reviewService.CreateAsync(reviewInput);
        }

        /// <summary>
        /// Update user profile (Users can only update their own profile)
        /// </summary>
        [Authorize]
        public async Task<User?> UpdateMyProfile(
            UpdateUserProfileInput input,
            [Service] UserService userService,
            ClaimsPrincipal claimsPrincipal)
        {
            var userId = claimsPrincipal.GetUserId();
            if (!userId.HasValue)
            {
                throw new GraphQLException("User not found");
            }

            var updateInput = new UpdateUserInput
            {
                Id = userId.Value,
                FirstName = input.FirstName,
                LastName = input.LastName,
                Email = input.Email,
                PhoneNumber = input.PhoneNumber,
                DateOfBirth = input.DateOfBirth,
                Address = input.Address
                // Note: Not allowing UserType or IsActive changes through this method
            };

            return await userService.UpdateAsync(updateInput);
        }

        /// <summary>
        /// Update any user (Librarian only)
        /// </summary>
        [Authorize(Roles = new[] { "Librarian" })]
        public async Task<User?> UpdateUserAuthorized(
            UpdateUserInput input,
            [Service] UserService userService,
            ClaimsPrincipal claimsPrincipal)
        {
            if (!claimsPrincipal.CanManageUsers())
            {
                throw new GraphQLException("You don't have permission to manage users");
            }

            return await userService.UpdateAsync(input);
        }
    }

    /// <summary>
    /// Authorized queries that require authentication and/or specific roles
    /// </summary>
    public partial class Query
    {
        /// <summary>
        /// Get user's own borrowings
        /// </summary>
        [Authorize]
        public async Task<IEnumerable<Borrowing>> MyBorrowings(
            [Service] BorrowingService borrowingService,
            ClaimsPrincipal claimsPrincipal)
        {
            var userId = claimsPrincipal.GetUserId();
            if (!userId.HasValue)
            {
                throw new GraphQLException("User not found");
            }

            return await borrowingService.GetByUserIdAsync(userId.Value);
        }

        /// <summary>
        /// Get user's own reviews
        /// </summary>
        [Authorize]
        public async Task<IEnumerable<Review>> MyReviews(
            [Service] IReviewService reviewService,
            ClaimsPrincipal claimsPrincipal)
        {
            var userId = claimsPrincipal.GetUserId();
            if (!userId.HasValue)
            {
                throw new GraphQLException("User not found");
            }

            return await reviewService.GetByUserIdAsync(userId.Value);
        }

        /// <summary>
        /// Get all users (Librarian only)
        /// </summary>
        [Authorize(Roles = new[] { "Librarian" })]
        public async Task<IQueryable<User>> GetAllUsers(
            [Service] UserService userService,
            ClaimsPrincipal claimsPrincipal)
        {
            if (!claimsPrincipal.CanManageUsers())
            {
                throw new GraphQLException("You don't have permission to view all users");
            }

            return await userService.GetAllAsync();
        }

        /// <summary>
        /// Get library statistics (Librarian only)
        /// </summary>
        [Authorize(Roles = new[] { "Librarian" })]
        public async Task<LibraryStatistics> GetLibraryStats(
            [Service] BookService bookService,
            [Service] UserService userService,
            [Service] BorrowingService borrowingService,
            ClaimsPrincipal claimsPrincipal)
        {
            if (!claimsPrincipal.CanManageBooks())
            {
                throw new GraphQLException("You don't have permission to view statistics");
            }

            var books = await bookService.GetAllAsync();
            var users = await userService.GetAllAsync();
            var borrowings = await borrowingService.GetAllAsync();

            return new LibraryStatistics
            {
                TotalBooks = await books.CountAsync(),
                TotalUsers = await users.CountAsync(),
                ActiveBorrowings = await borrowings.CountAsync(b => b.Status == BorrowingStatus.Active),
                OverdueBorrowings = await borrowings.CountAsync(b => 
                    b.Status == BorrowingStatus.Active && b.DueDate < DateTime.UtcNow),
                GeneratedAt = DateTime.UtcNow
            };
        }
    }

    /// <summary>
    /// Input type for updating user profile (limited fields)
    /// </summary>
    public class UpdateUserProfileInput
    {
        public string? FirstName { get; set; }
        public string? LastName { get; set; }
        public string? Email { get; set; }
        public string? PhoneNumber { get; set; }
        public DateTime? DateOfBirth { get; set; }
        public string? Address { get; set; }
    }

    /// <summary>
    /// Library statistics for admin dashboard
    /// </summary>
    public class LibraryStatistics
    {
        public int TotalBooks { get; set; }
        public int TotalUsers { get; set; }
        public int ActiveBorrowings { get; set; }
        public int OverdueBorrowings { get; set; }
        public DateTime GeneratedAt { get; set; }
    }

    /// <summary>
    /// Review service interface (you'll need to implement this)
    /// </summary>
    public interface IReviewService
    {
        Task<Review> CreateAsync(CreateReviewInput input);
        Task<IEnumerable<Review>> GetByUserIdAsync(int userId);
        Task<IEnumerable<Review>> GetByBookIdAsync(int bookId);
    }
}
