using FluentValidation;
using GraphQLSimple.GraphQL.Types;
using GraphQLSimple.Data;

namespace GraphQLSimple.Extensions
{
    public class CreateBookInputValidator : AbstractValidator<CreateBookInput>
    {
        private readonly LibraryContext _context;

        public CreateBookInputValidator(LibraryContext context)
        {
            _context = context;

            RuleFor(x => x.Title)
                .NotEmpty().WithMessage("Title is required")
                .MaximumLength(200).WithMessage("Title must not exceed 200 characters");

            RuleFor(x => x.ISBN)
                .NotEmpty().WithMessage("ISBN is required")
                .Must(BeValidISBN).WithMessage("Invalid ISBN format")
                .Must(BeUniqueISBN).WithMessage("ISBN already exists");

            RuleFor(x => x.Description)
                .MaximumLength(2000).WithMessage("Description must not exceed 2000 characters");

            RuleFor(x => x.Pages)
                .GreaterThan(0).WithMessage("Pages must be greater than 0")
                .LessThanOrEqualTo(10000).WithMessage("Pages must not exceed 10000");

            RuleFor(x => x.Price)
                .GreaterThan(0).WithMessage("Price must be greater than 0")
                .LessThanOrEqualTo(9999.99m).WithMessage("Price must not exceed $9999.99");

            RuleFor(x => x.Publisher)
                .NotEmpty().WithMessage("Publisher is required")
                .MaximumLength(100).WithMessage("Publisher must not exceed 100 characters");

            RuleFor(x => x.Language)
                .NotEmpty().WithMessage("Language is required")
                .MaximumLength(50).WithMessage("Language must not exceed 50 characters");

            RuleFor(x => x.CopiesTotal)
                .GreaterThanOrEqualTo(0).WithMessage("Copies total must be 0 or greater")
                .LessThanOrEqualTo(1000).WithMessage("Copies total must not exceed 1000");

            RuleFor(x => x.CopiesAvailable)
                .GreaterThanOrEqualTo(0).WithMessage("Copies available must be 0 or greater")
                .LessThanOrEqualTo(x => x.CopiesTotal).WithMessage("Copies available cannot exceed total copies");

            RuleFor(x => x.AuthorId)
                .Must(AuthorExists).WithMessage("Author does not exist");

            RuleFor(x => x.CategoryId)
                .Must(CategoryExists).WithMessage("Category does not exist");

            RuleFor(x => x.PublishedDate)
                .Must(BeValidPublishDate).WithMessage("Published date cannot be in the future");
        }

        private bool BeValidISBN(string isbn)
        {
            if (string.IsNullOrWhiteSpace(isbn)) return false;
            
            var cleaned = isbn.Replace("-", "").Replace(" ", "");
            return cleaned.Length == 10 || cleaned.Length == 13;
        }

        private bool BeUniqueISBN(string isbn)
        {
            var cleaned = isbn.Replace("-", "").Replace(" ", "");
            return !_context.Books.Any(b => b.ISBN == cleaned);
        }

        private bool AuthorExists(int authorId)
        {
            return _context.Authors.Any(a => a.Id == authorId && a.IsActive);
        }

        private bool CategoryExists(int categoryId)
        {
            return _context.Categories.Any(c => c.Id == categoryId && c.IsActive);
        }

        private bool BeValidPublishDate(DateTime date)
        {
            return date <= DateTime.Now;
        }
    }

    public class CreateAuthorInputValidator : AbstractValidator<CreateAuthorInput>
    {
        public CreateAuthorInputValidator()
        {
            RuleFor(x => x.FirstName)
                .NotEmpty().WithMessage("First name is required")
                .MaximumLength(100).WithMessage("First name must not exceed 100 characters");

            RuleFor(x => x.LastName)
                .NotEmpty().WithMessage("Last name is required")
                .MaximumLength(100).WithMessage("Last name must not exceed 100 characters");

            RuleFor(x => x.DateOfBirth)
                .Must(BeValidBirthDate).WithMessage("Date of birth cannot be in the future or more than 150 years ago");

            RuleFor(x => x.Biography)
                .MaximumLength(2000).WithMessage("Biography must not exceed 2000 characters");

            RuleFor(x => x.Nationality)
                .MaximumLength(100).WithMessage("Nationality must not exceed 100 characters");

            RuleFor(x => x.Email)
                .EmailAddress().When(x => !string.IsNullOrEmpty(x.Email))
                .WithMessage("Invalid email format");
        }

        private bool BeValidBirthDate(DateTime date)
        {
            var minDate = DateTime.Now.AddYears(-150);
            return date >= minDate && date <= DateTime.Now;
        }
    }

    public class CreateUserInputValidator : AbstractValidator<CreateUserInput>
    {
        private readonly LibraryContext _context;

        public CreateUserInputValidator(LibraryContext context)
        {
            _context = context;

            RuleFor(x => x.FirstName)
                .NotEmpty().WithMessage("First name is required")
                .MaximumLength(100).WithMessage("First name must not exceed 100 characters");

            RuleFor(x => x.LastName)
                .NotEmpty().WithMessage("Last name is required")
                .MaximumLength(100).WithMessage("Last name must not exceed 100 characters");

            RuleFor(x => x.Email)
                .NotEmpty().WithMessage("Email is required")
                .EmailAddress().WithMessage("Invalid email format")
                .Must(BeUniqueEmail).WithMessage("Email already exists");

            RuleFor(x => x.PhoneNumber)
                .Matches(@"^\+?[\d\s\-\(\)]+$").When(x => !string.IsNullOrEmpty(x.PhoneNumber))
                .WithMessage("Invalid phone number format");

            RuleFor(x => x.DateOfBirth)
                .Must(BeValidBirthDate).WithMessage("Date of birth must be valid and not in the future");

            RuleFor(x => x.Address)
                .MaximumLength(200).WithMessage("Address must not exceed 200 characters");
        }

        private bool BeUniqueEmail(string email)
        {
            return !_context.Users.Any(u => u.Email.ToLower() == email.ToLower());
        }

        private bool BeValidBirthDate(DateTime date)
        {
            var minDate = DateTime.Now.AddYears(-120);
            return date >= minDate && date <= DateTime.Now;
        }
    }

    public class CreateReviewInputValidator : AbstractValidator<CreateReviewInput>
    {
        private readonly LibraryContext _context;

        public CreateReviewInputValidator(LibraryContext context)
        {
            _context = context;

            RuleFor(x => x.BookId)
                .Must(BookExists).WithMessage("Book does not exist");

            RuleFor(x => x.UserId)
                .Must(UserExists).WithMessage("User does not exist");

            RuleFor(x => x.Rating)
                .InclusiveBetween(1, 5).WithMessage("Rating must be between 1 and 5");

            RuleFor(x => x.Comment)
                .MaximumLength(1000).WithMessage("Comment must not exceed 1000 characters");

            RuleFor(x => x)
                .Must(NotHaveExistingReview).WithMessage("User has already reviewed this book");
        }

        private bool BookExists(int bookId)
        {
            return _context.Books.Any(b => b.Id == bookId);
        }

        private bool UserExists(int userId)
        {
            return _context.Users.Any(u => u.Id == userId && u.IsActive);
        }

        private bool NotHaveExistingReview(CreateReviewInput input)
        {
            return !_context.Reviews.Any(r => r.BookId == input.BookId && r.UserId == input.UserId);
        }
    }

    public class CreateBorrowingInputValidator : AbstractValidator<CreateBorrowingInput>
    {
        private readonly LibraryContext _context;

        public CreateBorrowingInputValidator(LibraryContext context)
        {
            _context = context;

            RuleFor(x => x.BookId)
                .Must(BookExistsAndAvailable).WithMessage("Book does not exist or is not available");

            RuleFor(x => x.UserId)
                .Must(UserExistsAndActive).WithMessage("User does not exist or is not active")
                .Must(UserNotExceededBorrowingLimit).WithMessage("User has reached the maximum borrowing limit");

            RuleFor(x => x.DueDate)
                .GreaterThan(DateTime.Now).WithMessage("Due date must be in the future")
                .LessThanOrEqualTo(DateTime.Now.AddDays(30)).WithMessage("Due date cannot be more than 30 days from now");

            RuleFor(x => x.Notes)
                .MaximumLength(500).WithMessage("Notes must not exceed 500 characters");
        }

        private bool BookExistsAndAvailable(int bookId)
        {
            var book = _context.Books.FirstOrDefault(b => b.Id == bookId);
            return book != null && book.IsAvailable && book.CopiesAvailable > 0;
        }

        private bool UserExistsAndActive(int userId)
        {
            return _context.Users.Any(u => u.Id == userId && u.IsActive);
        }

        private bool UserNotExceededBorrowingLimit(int userId)
        {
            var activeBorrowings = _context.Borrowings.Count(b => b.UserId == userId && b.Status == Models.BorrowingStatus.Active);
            return activeBorrowings < 5; // Maximum 5 active borrowings per user
        }
    }

    public class CreateCategoryInputValidator : AbstractValidator<CreateCategoryInput>
    {
        private readonly LibraryContext _context;

        public CreateCategoryInputValidator(LibraryContext context)
        {
            _context = context;

            RuleFor(x => x.Name)
                .NotEmpty().WithMessage("Category name is required")
                .MaximumLength(100).WithMessage("Category name must not exceed 100 characters")
                .Must(BeUniqueName).WithMessage("Category name already exists");

            RuleFor(x => x.Description)
                .MaximumLength(500).WithMessage("Description must not exceed 500 characters");

            RuleFor(x => x.ParentCategoryId)
                .Must(ParentCategoryExists).When(x => x.ParentCategoryId.HasValue)
                .WithMessage("Parent category does not exist");
        }

        private bool BeUniqueName(string name)
        {
            return !_context.Categories.Any(c => c.Name.ToLower() == name.ToLower());
        }

        private bool ParentCategoryExists(int? parentId)
        {
            return !parentId.HasValue || _context.Categories.Any(c => c.Id == parentId.Value && c.IsActive);
        }
    }

    public class CreateTagInputValidator : AbstractValidator<CreateTagInput>
    {
        private readonly LibraryContext _context;

        public CreateTagInputValidator(LibraryContext context)
        {
            _context = context;

            RuleFor(x => x.Name)
                .NotEmpty().WithMessage("Tag name is required")
                .MaximumLength(50).WithMessage("Tag name must not exceed 50 characters")
                .Must(BeUniqueName).WithMessage("Tag name already exists");

            RuleFor(x => x.Description)
                .MaximumLength(200).WithMessage("Description must not exceed 200 characters");

            RuleFor(x => x.Color)
                .Matches(@"^#[0-9A-Fa-f]{6}$").WithMessage("Color must be a valid hex color code");
        }

        private bool BeUniqueName(string name)
        {
            return !_context.Tags.Any(t => t.Name.ToLower() == name.ToLower());
        }
    }
}
