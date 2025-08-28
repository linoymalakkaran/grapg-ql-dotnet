using Microsoft.EntityFrameworkCore;
using GraphQLSimple.Data;
using GraphQLSimple.Models;
using GraphQLSimple.Services;
using GraphQLSimple.GraphQL.Types;
using GraphQLSimple.GraphQL.DataLoaders;
using HotChocolate.Data;
using HotChocolate.Types;

namespace GraphQLSimple.GraphQL
{
    [QueryType]
    public class Query
    {
        // Book Queries
        [UseProjection]
        [UseFiltering]
        [UseSorting]
        public async Task<IQueryable<Book>> GetBooks(IBookService bookService)
        {
            return await bookService.GetAllAsync();
        }

        public async Task<Book?> GetBook(int id, IBookService bookService)
        {
            return await bookService.GetByIdAsync(id);
        }

        public async Task<IEnumerable<Book>> GetBooksByIds(
            int[] ids, 
            IBookService bookService)
        {
            var books = new List<Book>();
            foreach (var id in ids)
            {
                var book = await bookService.GetByIdAsync(id);
                if (book != null) books.Add(book);
            }
            return books;
        }

        // Author Queries
        [UseProjection]
        [UseFiltering]
        [UseSorting]
        public async Task<IQueryable<Author>> GetAuthors(IAuthorService authorService)
        {
            return await authorService.GetAllAsync();
        }

        public async Task<Author?> GetAuthor(int id, IAuthorService authorService)
        {
            return await authorService.GetByIdAsync(id);
        }

        // User Queries
        [UseProjection]
        [UseFiltering]
        [UseSorting]
        public IQueryable<User> GetUsers(LibraryContext context)
        {
            return context.Users
                .Include(u => u.Reviews)
                    .ThenInclude(r => r.Book)
                .Include(u => u.Borrowings)
                    .ThenInclude(b => b.Book);
        }

        public async Task<User?> GetUser(int id, LibraryContext context)
        {
            return await context.Users
                .Include(u => u.Reviews)
                    .ThenInclude(r => r.Book)
                .Include(u => u.Borrowings)
                    .ThenInclude(b => b.Book)
                        .ThenInclude(book => book.Author)
                .FirstOrDefaultAsync(u => u.Id == id);
        }

        // Review Queries
        [UseProjection]
        [UseFiltering]
        [UseSorting]
        public IQueryable<Review> GetReviews(LibraryContext context)
        {
            return context.Reviews
                .Include(r => r.Book)
                    .ThenInclude(b => b.Author)
                .Include(r => r.User);
        }

        public async Task<Review?> GetReview(int id, LibraryContext context)
        {
            return await context.Reviews
                .Include(r => r.Book)
                    .ThenInclude(b => b.Author)
                .Include(r => r.User)
                .FirstOrDefaultAsync(r => r.Id == id);
        }

        public async Task<IEnumerable<Review>> GetReviewsByBook(
            int bookId, 
            LibraryContext context)
        {
            return await context.Reviews
                .Include(r => r.User)
                .Where(r => r.BookId == bookId)
                .ToListAsync();
        }

        public async Task<IEnumerable<Review>> GetReviewsByUser(
            int userId, 
            LibraryContext context)
        {
            return await context.Reviews
                .Include(r => r.Book)
                    .ThenInclude(b => b.Author)
                .Where(r => r.UserId == userId)
                .ToListAsync();
        }

        // Category Queries
        [UseProjection]
        [UseFiltering]
        [UseSorting]
        public IQueryable<Category> GetCategories(LibraryContext context)
        {
            return context.Categories
                .Include(c => c.Books)
                .Include(c => c.SubCategories)
                .Include(c => c.ParentCategory);
        }

        public async Task<Category?> GetCategory(int id, LibraryContext context)
        {
            return await context.Categories
                .Include(c => c.Books)
                    .ThenInclude(b => b.Author)
                .Include(c => c.SubCategories)
                .Include(c => c.ParentCategory)
                .FirstOrDefaultAsync(c => c.Id == id);
        }

        public async Task<IEnumerable<Category>> GetTopLevelCategories(LibraryContext context)
        {
            return await context.Categories
                .Include(c => c.SubCategories)
                .Where(c => c.ParentCategoryId == null && c.IsActive)
                .ToListAsync();
        }

        // Borrowing Queries
        [UseProjection]
        [UseFiltering]
        [UseSorting]
        public async Task<IQueryable<Borrowing>> GetBorrowings(IBorrowingService borrowingService)
        {
            return await borrowingService.GetAllAsync();
        }

        public async Task<Borrowing?> GetBorrowing(int id, IBorrowingService borrowingService)
        {
            return await borrowingService.GetByIdAsync(id);
        }

        public async Task<IQueryable<Borrowing>> GetBorrowingsByUser(
            int userId, 
            IBorrowingService borrowingService)
        {
            return await borrowingService.GetByUserAsync(userId);
        }

        public async Task<IQueryable<Borrowing>> GetBorrowingsByBook(
            int bookId, 
            IBorrowingService borrowingService)
        {
            return await borrowingService.GetByBookAsync(bookId);
        }

        public async Task<IQueryable<Borrowing>> GetOverdueBorrowings(IBorrowingService borrowingService)
        {
            return await borrowingService.GetOverdueAsync();
        }

        // Tag Queries
        [UseProjection]
        [UseFiltering]
        [UseSorting]
        public IQueryable<Models.Tag> GetTags(LibraryContext context)
        {
            return context.Tags
                .Include(t => t.BookTags)
                    .ThenInclude(bt => bt.Book);
        }

        public async Task<Models.Tag?> GetTag(int id, LibraryContext context)
        {
            return await context.Tags
                .Include(t => t.BookTags)
                    .ThenInclude(bt => bt.Book)
                        .ThenInclude(b => b.Author)
                .FirstOrDefaultAsync(t => t.Id == id);
        }

        // Statistics Queries
        public async Task<int> GetTotalBooks(LibraryContext context)
        {
            return await context.Books.CountAsync();
        }

        public async Task<int> GetTotalAuthors(LibraryContext context)
        {
            return await context.Authors.Where(a => a.IsActive).CountAsync();
        }

        public async Task<int> GetTotalUsers(LibraryContext context)
        {
            return await context.Users.Where(u => u.IsActive).CountAsync();
        }

        public async Task<int> GetActiveBorrowings(LibraryContext context)
        {
            return await context.Borrowings.Where(b => b.Status == BorrowingStatus.Active).CountAsync();
        }

        public async Task<int> GetOverdueBorrowingsCount(LibraryContext context)
        {
            return await context.Borrowings
                .Where(b => b.Status == BorrowingStatus.Active && b.DueDate < DateTime.UtcNow)
                .CountAsync();
        }

        public async Task<double> GetAverageBookRating(int bookId, LibraryContext context)
        {
            var ratings = await context.Reviews
                .Where(r => r.BookId == bookId)
                .Select(r => r.Rating)
                .ToListAsync();

            return ratings.Any() ? ratings.Average() : 0.0;
        }

        public async Task<IEnumerable<Book>> GetPopularBooks(int count, LibraryContext context)
        {
            return await context.Books
                .Include(b => b.Author)
                .Include(b => b.Reviews)
                .Where(b => b.Reviews.Any())
                .OrderByDescending(b => b.Reviews.Average(r => r.Rating))
                .ThenByDescending(b => b.Reviews.Count)
                .Take(count)
                .ToListAsync();
        }

        public async Task<IEnumerable<Book>> GetRecentBooks(int count, LibraryContext context)
        {
            return await context.Books
                .Include(b => b.Author)
                .Include(b => b.Category)
                .OrderByDescending(b => b.CreatedAt)
                .Take(count)
                .ToListAsync();
        }

        public async Task<IEnumerable<Author>> GetProlificAuthors(int count, LibraryContext context)
        {
            return await context.Authors
                .Include(a => a.Books)
                .Where(a => a.IsActive && a.Books.Any())
                .OrderByDescending(a => a.Books.Count)
                .Take(count)
                .ToListAsync();
        }

        // Advanced Search
        public async Task<IEnumerable<Book>> SearchBooksFullText(
            string searchTerm, 
            LibraryContext context)
        {
            return await context.Books
                .Include(b => b.Author)
                .Include(b => b.Category)
                .Include(b => b.BookTags)
                    .ThenInclude(bt => bt.Tag)
                .Where(b => 
                    b.Title.Contains(searchTerm) ||
                    b.Description.Contains(searchTerm) ||
                    b.Author.FirstName.Contains(searchTerm) ||
                    b.Author.LastName.Contains(searchTerm) ||
                    b.Publisher.Contains(searchTerm) ||
                    b.BookTags.Any(bt => bt.Tag.Name.Contains(searchTerm)))
                .ToListAsync();
        }

        // Health Check
        public async Task<string> HealthCheck(LibraryContext context)
        {
            try
            {
                await context.Books.AnyAsync();
                return "GraphQL API is healthy";
            }
            catch (Exception ex)
            {
                throw new GraphQLException($"Health check failed: {ex.Message}");
            }
        }
    }
}
