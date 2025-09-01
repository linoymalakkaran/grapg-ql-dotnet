using System.ComponentModel.DataAnnotations;
using GraphQLSimple.Models;
using HotChocolate.Types;

namespace GraphQLSimple.GraphQL.Types
{
    // Book Input Types
    public class CreateBookInput
    {
        [Required]
        [StringLength(200)]
        public string Title { get; set; } = string.Empty;
        
        [Required]
        [StringLength(13)]
        [GraphQLType(typeof(ISBNType))]
        public string ISBN { get; set; } = string.Empty;
        
        [StringLength(2000)]
        public string Description { get; set; } = string.Empty;
        
        public DateTime PublishedDate { get; set; }
        
        [Range(1, 10000)]
        [GraphQLType(typeof(PositiveIntType))]
        public int Pages { get; set; }
        
        [Range(0.01, 9999.99)]
        public decimal Price { get; set; }
        
        [StringLength(100)]
        public string Publisher { get; set; } = string.Empty;
        
        [StringLength(50)]
        public string Language { get; set; } = "English";
        
        [Range(0, 1000)]
        [GraphQLType(typeof(PositiveIntType))]
        public int CopiesTotal { get; set; }
        
        [Range(0, 1000)]
        [GraphQLType(typeof(PositiveIntType))]
        public int CopiesAvailable { get; set; }
        
        public int AuthorId { get; set; }
        public int CategoryId { get; set; }
        public List<int> TagIds { get; set; } = new();
    }

    public class UpdateBookInput
    {
        public int Id { get; set; }
        
        [StringLength(200)]
        public string? Title { get; set; }
        
        [StringLength(13)]
        [GraphQLType(typeof(ISBNType))]
        public string? ISBN { get; set; }
        
        [StringLength(2000)]
        public string? Description { get; set; }
        
        public DateTime? PublishedDate { get; set; }
        
        [Range(1, 10000)]
        [GraphQLType(typeof(PositiveIntType))]
        public int? Pages { get; set; }
        
        [Range(0.01, 9999.99)]
        public decimal? Price { get; set; }
        
        [StringLength(100)]
        public string? Publisher { get; set; }
        
        [StringLength(50)]
        public string? Language { get; set; }
        
        [Range(0, 1000)]
        [GraphQLType(typeof(PositiveIntType))]
        public int? CopiesTotal { get; set; }
        
        [Range(0, 1000)]
        [GraphQLType(typeof(PositiveIntType))]
        public int? CopiesAvailable { get; set; }
        
        public int? AuthorId { get; set; }
        public int? CategoryId { get; set; }
        public List<int>? TagIds { get; set; }
    }

    // Author Input Types
    public class CreateAuthorInput
    {
        [Required]
        [StringLength(100)]
        public string FirstName { get; set; } = string.Empty;
        
        [Required]
        [StringLength(100)]
        public string LastName { get; set; } = string.Empty;
        
        public DateTime DateOfBirth { get; set; }
        
        [StringLength(2000)]
        public string? Biography { get; set; }
        
        [StringLength(100)]
        public string? Nationality { get; set; }
        
        [EmailAddress]
        [GraphQLType(typeof(EmailType))]
        public string? Email { get; set; }
    }

    public class UpdateAuthorInput
    {
        public int Id { get; set; }
        
        [StringLength(100)]
        public string? FirstName { get; set; }
        
        [StringLength(100)]
        public string? LastName { get; set; }
        
        public DateTime? DateOfBirth { get; set; }
        
        [StringLength(2000)]
        public string? Biography { get; set; }
        
        [StringLength(100)]
        public string? Nationality { get; set; }
        
        [EmailAddress]
        [GraphQLType(typeof(EmailType))]
        public string? Email { get; set; }
        
        public bool? IsActive { get; set; }
    }

    // User Input Types
    public class CreateUserInput
    {
        [Required]
        [StringLength(100)]
        public string FirstName { get; set; } = string.Empty;
        
        [Required]
        [StringLength(100)]
        public string LastName { get; set; } = string.Empty;
        
        [Required]
        [EmailAddress]
        [GraphQLType(typeof(EmailType))]
        public string Email { get; set; } = string.Empty;
        
        // Password hash - for internal use, not exposed in GraphQL schema
        [GraphQLIgnore]
        public string PasswordHash { get; set; } = string.Empty;
        
        [Phone]
        [GraphQLType(typeof(PhoneType))]
        public string? PhoneNumber { get; set; }
        
        public DateTime DateOfBirth { get; set; }
        
        public UserType UserType { get; set; } = UserType.Member;
        
        [StringLength(200)]
        public string? Address { get; set; }
    }

    public class UpdateUserInput
    {
        public int Id { get; set; }
        
        [StringLength(100)]
        public string? FirstName { get; set; }
        
        [StringLength(100)]
        public string? LastName { get; set; }
        
        [EmailAddress]
        [GraphQLType(typeof(EmailType))]
        public string? Email { get; set; }
        
        // Password hash - for internal use, not exposed in GraphQL schema
        [GraphQLIgnore]
        public string? PasswordHash { get; set; }
        
        [Phone]
        [GraphQLType(typeof(PhoneType))]
        public string? PhoneNumber { get; set; }
        
        public DateTime? DateOfBirth { get; set; }
        
        public UserType? UserType { get; set; }
        
        [StringLength(200)]
        public string? Address { get; set; }
        
        public bool? IsActive { get; set; }
    }

    // Review Input Types
    public class CreateReviewInput
    {
        public int BookId { get; set; }
        public int UserId { get; set; }
        
        [Range(1, 5)]
        [GraphQLType(typeof(PositiveIntType))]
        public int Rating { get; set; }
        
        [StringLength(1000)]
        public string? Comment { get; set; }
        
        public bool IsVerifiedPurchase { get; set; } = false;
    }

    public class UpdateReviewInput
    {
        public int Id { get; set; }
        
        [Range(1, 5)]
        public int? Rating { get; set; }
        
        [StringLength(1000)]
        public string? Comment { get; set; }
    }

    // Category Input Types
    public class CreateCategoryInput
    {
        [Required]
        [StringLength(100)]
        public string Name { get; set; } = string.Empty;
        
        [StringLength(500)]
        public string Description { get; set; } = string.Empty;
        
        public int? ParentCategoryId { get; set; }
    }

    public class UpdateCategoryInput
    {
        public int Id { get; set; }
        
        [StringLength(100)]
        public string? Name { get; set; }
        
        [StringLength(500)]
        public string? Description { get; set; }
        
        public int? ParentCategoryId { get; set; }
        
        public bool? IsActive { get; set; }
    }

    // Borrowing Input Types
    public class CreateBorrowingInput
    {
        public int BookId { get; set; }
        public int UserId { get; set; }
        public DateTime DueDate { get; set; }
        
        [StringLength(500)]
        public string? Notes { get; set; }
    }

    public class UpdateBorrowingInput
    {
        public int Id { get; set; }
        public DateTime? ReturnedDate { get; set; }
        public BorrowingStatus? Status { get; set; }
        
        [Range(0.0, 1000.0)]
        public decimal? FineAmount { get; set; }
        
        [StringLength(500)]
        public string? Notes { get; set; }
    }

    // Tag Input Types
    public class CreateTagInput
    {
        [Required]
        [StringLength(50)]
        public string Name { get; set; } = string.Empty;
        
        [StringLength(200)]
        public string Description { get; set; } = string.Empty;
        
        public string Color { get; set; } = "#007bff";
    }

    public class UpdateTagInput
    {
        public int Id { get; set; }
        
        [StringLength(50)]
        public string? Name { get; set; }
        
        [StringLength(200)]
        public string? Description { get; set; }
        
        public string? Color { get; set; }
        
        public bool? IsActive { get; set; }
    }

    // Sort Input Types (Filter inputs are auto-generated by HotChocolate)
    public enum BookSortField
    {
        Title,
        PublishedDate,
        Price,
        Rating,
        Pages,
        CreatedAt
    }

    public enum AuthorSortField
    {
        FirstName,
        LastName,
        DateOfBirth,
        BookCount,
        CreatedAt
    }

    public enum SortDirection
    {
        Ascending,
        Descending
    }

    public class BookSortInput
    {
        public BookSortField Field { get; set; }
        public SortDirection Direction { get; set; } = SortDirection.Ascending;
    }

    public class AuthorSortInput
    {
        public AuthorSortField Field { get; set; }
        public SortDirection Direction { get; set; } = SortDirection.Ascending;
    }
}
