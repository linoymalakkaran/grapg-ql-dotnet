using System.ComponentModel.DataAnnotations;

namespace GraphQLSimple.Models
{
    public class Book
    {
        public int Id { get; set; }
        
        [Required]
        [StringLength(200)]
        public string Title { get; set; } = string.Empty;
        
        [Required]
        [StringLength(13)]
        public string ISBN { get; set; } = string.Empty;
        
        [StringLength(2000)]
        public string Description { get; set; } = string.Empty;
        
        public DateTime PublishedDate { get; set; }
        
        [Range(1, 10000)]
        public int Pages { get; set; }
        
        [Range(0.01, 9999.99)]
        public decimal Price { get; set; }
        
        [StringLength(100)]
        public string Publisher { get; set; } = string.Empty;
        
        [StringLength(50)]
        public string Language { get; set; } = "English";
        
        public bool IsAvailable { get; set; } = true;
        
        [Range(0, 1000)]
        public int CopiesTotal { get; set; }
        
        [Range(0, 1000)]
        public int CopiesAvailable { get; set; }
        
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? UpdatedAt { get; set; }
        
        // Foreign Keys
        public int AuthorId { get; set; }
        public int CategoryId { get; set; }
        
        // Navigation Properties
        public Author Author { get; set; } = null!;
        public Category Category { get; set; } = null!;
        public List<Review> Reviews { get; set; } = new();
        public List<Borrowing> Borrowings { get; set; } = new();
        public List<BookTag> BookTags { get; set; } = new();
        
        // Computed Properties
        public double AverageRating => Reviews.Any() ? Reviews.Average(r => r.Rating) : 0.0;
        public int ReviewCount => Reviews.Count;
        public bool HasAvailableCopies => CopiesAvailable > 0;
    }

    public class Author
    {
        public int Id { get; set; }
        
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
        public string? Email { get; set; }
        
        public bool IsActive { get; set; } = true;
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? UpdatedAt { get; set; }
        
        // Navigation Properties
        public List<Book> Books { get; set; } = new();
        
        // Computed Properties
        public string FullName => $"{FirstName} {LastName}";
        public int BookCount => Books.Count;
        public int Age => DateTime.Now.Year - DateOfBirth.Year;
    }

    public class Review
    {
        public int Id { get; set; }
        
        public int BookId { get; set; }
        public Book Book { get; set; } = null!;
        
        public int UserId { get; set; }
        public User User { get; set; } = null!;
        
        [Range(1, 5)]
        public int Rating { get; set; }
        
        [StringLength(1000)]
        public string? Comment { get; set; }
        
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? UpdatedAt { get; set; }
        
        public bool IsVerifiedPurchase { get; set; } = false;
        public int HelpfulVotes { get; set; } = 0;
    }

    public class Category
    {
        public int Id { get; set; }
        
        [Required]
        [StringLength(100)]
        public string Name { get; set; } = string.Empty;
        
        [StringLength(500)]
        public string Description { get; set; } = string.Empty;
        
        public int? ParentCategoryId { get; set; }
        public Category? ParentCategory { get; set; }
        
        public bool IsActive { get; set; } = true;
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        
        // Navigation Properties
        public List<Book> Books { get; set; } = new();
        public List<Category> SubCategories { get; set; } = new();
        
        // Computed Properties
        public int BookCount => Books.Count;
    }

    public class User
    {
        public int Id { get; set; }
        
        [Required]
        [StringLength(100)]
        public string FirstName { get; set; } = string.Empty;
        
        [Required]
        [StringLength(100)]
        public string LastName { get; set; } = string.Empty;
        
        [Required]
        [EmailAddress]
        public string Email { get; set; } = string.Empty;
        
        // Password hash - not exposed in GraphQL
        [Required]
        [StringLength(500)]
        public string PasswordHash { get; set; } = string.Empty;
        
        [Phone]
        public string? PhoneNumber { get; set; }
        
        public DateTime DateOfBirth { get; set; }
        public DateTime MembershipDate { get; set; } = DateTime.UtcNow;
        public bool IsActive { get; set; } = true;
        public UserType UserType { get; set; } = UserType.Member;
        
        [StringLength(200)]
        public string? Address { get; set; }
        
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? UpdatedAt { get; set; }
        
        // Navigation Properties
        public List<Review> Reviews { get; set; } = new();
        public List<Borrowing> Borrowings { get; set; } = new();
        
        // Computed Properties
        public string FullName => $"{FirstName} {LastName}";
        public int Age => DateTime.Now.Year - DateOfBirth.Year;
        public int ActiveBorrowings => Borrowings.Count(b => b.ReturnedDate == null);
    }

    public class Borrowing
    {
        public int Id { get; set; }
        
        public int BookId { get; set; }
        public Book Book { get; set; } = null!;
        
        public int UserId { get; set; }
        public User User { get; set; } = null!;
        
        public DateTime BorrowedDate { get; set; } = DateTime.UtcNow;
        public DateTime DueDate { get; set; }
        public DateTime? ReturnedDate { get; set; }
        
        [Range(0.0, 1000.0)]
        public decimal? FineAmount { get; set; }
        
        public BorrowingStatus Status { get; set; } = BorrowingStatus.Active;
        
        [StringLength(500)]
        public string? Notes { get; set; }
        
        // Computed Properties
        public bool IsOverdue => DueDate < DateTime.Now && ReturnedDate == null;
        public int DaysOverdue => IsOverdue ? (DateTime.Now - DueDate).Days : 0;
    }

    public class Tag
    {
        public int Id { get; set; }
        
        [Required]
        [StringLength(50)]
        public string Name { get; set; } = string.Empty;
        
        [StringLength(200)]
        public string Description { get; set; } = string.Empty;
        
        public string Color { get; set; } = "#007bff";
        public bool IsActive { get; set; } = true;
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        
        // Navigation Properties
        public List<BookTag> BookTags { get; set; } = new();
        
        // Computed Properties
        public int BookCount => BookTags.Count;
    }

    public class BookTag
    {
        public int BookId { get; set; }
        public Book Book { get; set; } = null!;
        
        public int TagId { get; set; }
        public Tag Tag { get; set; } = null!;
        
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }

    // Enums
    public enum UserType
    {
        Member = 1,
        Librarian = 2,
        Admin = 3
    }

    public enum BorrowingStatus
    {
        Active = 1,
        Returned = 2,
        Overdue = 3,
        Lost = 4
    }

    // Custom Scalar Types for GraphQL
    public readonly struct ISBN
    {
        public string Value { get; }

        public ISBN(string value)
        {
            if (string.IsNullOrWhiteSpace(value))
                throw new ArgumentException("ISBN cannot be null or empty", nameof(value));
            
            Value = value.Replace("-", "").Replace(" ", "");
            
            if (Value.Length != 10 && Value.Length != 13)
                throw new ArgumentException("ISBN must be 10 or 13 digits", nameof(value));
        }

        public static implicit operator string(ISBN isbn) => isbn.Value;
        public static implicit operator ISBN(string value) => new(value);
        
        public override string ToString() => Value;
    }

    public readonly struct EmailAddress
    {
        public string Value { get; }

        public EmailAddress(string value)
        {
            if (string.IsNullOrWhiteSpace(value))
                throw new ArgumentException("Email cannot be null or empty", nameof(value));
            
            if (!System.Text.RegularExpressions.Regex.IsMatch(value, 
                @"^[a-zA-Z0-9._%+-]+@[a-zA-Z0-9.-]+\.[a-zA-Z]{2,}$"))
                throw new ArgumentException("Invalid email format", nameof(value));
            
            Value = value.ToLowerInvariant();
        }

        public static implicit operator string(EmailAddress email) => email.Value;
        public static implicit operator EmailAddress(string value) => new(value);
        
        public override string ToString() => Value;
    }
}
