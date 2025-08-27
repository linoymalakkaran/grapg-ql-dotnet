using Microsoft.EntityFrameworkCore;
using GraphQLSimple.Data;
using GraphQLSimple.Models;
using GraphQLSimple.GraphQL.Types;

namespace GraphQLSimple.Services
{
    public interface IBookService
    {
        Task<Book?> GetByIdAsync(int id);
        Task<IQueryable<Book>> GetAllAsync();
        Task<Book> CreateAsync(CreateBookInput input);
        Task<Book?> UpdateAsync(UpdateBookInput input);
        Task<bool> DeleteAsync(int id);
        Task<IQueryable<Book>> SearchAsync(BookFilterInput filter);
        Task<bool> UpdateAvailabilityAsync(int bookId, int copiesChange);
    }

    public class BookService : IBookService
    {
        private readonly LibraryContext _context;

        public BookService(LibraryContext context)
        {
            _context = context;
        }

        public async Task<Book?> GetByIdAsync(int id)
        {
            return await _context.Books
                .Include(b => b.Author)
                .Include(b => b.Category)
                .Include(b => b.Reviews)
                .Include(b => b.BookTags)
                    .ThenInclude(bt => bt.Tag)
                .FirstOrDefaultAsync(b => b.Id == id);
        }

        public Task<IQueryable<Book>> GetAllAsync()
        {
            var query = _context.Books
                .Include(b => b.Author)
                .Include(b => b.Category)
                .Include(b => b.Reviews)
                .Include(b => b.BookTags)
                    .ThenInclude(bt => bt.Tag);
            
            return Task.FromResult(query.AsQueryable());
        }

        public async Task<Book> CreateAsync(CreateBookInput input)
        {
            var book = new Book
            {
                Title = input.Title,
                ISBN = input.ISBN,
                Description = input.Description,
                PublishedDate = input.PublishedDate,
                Pages = input.Pages,
                Price = input.Price,
                Publisher = input.Publisher,
                Language = input.Language,
                CopiesTotal = input.CopiesTotal,
                CopiesAvailable = input.CopiesAvailable,
                AuthorId = input.AuthorId,
                CategoryId = input.CategoryId,
                IsAvailable = input.CopiesAvailable > 0,
                CreatedAt = DateTime.UtcNow
            };

            _context.Books.Add(book);
            await _context.SaveChangesAsync();

            // Add tags if provided
            if (input.TagIds.Any())
            {
                var bookTags = input.TagIds.Select(tagId => new BookTag
                {
                    BookId = book.Id,
                    TagId = tagId,
                    CreatedAt = DateTime.UtcNow
                }).ToList();

                _context.BookTags.AddRange(bookTags);
                await _context.SaveChangesAsync();
            }

            return await GetByIdAsync(book.Id) ?? book;
        }

        public async Task<Book?> UpdateAsync(UpdateBookInput input)
        {
            var book = await _context.Books.FindAsync(input.Id);
            if (book == null) return null;

            if (input.Title != null) book.Title = input.Title;
            if (input.ISBN != null) book.ISBN = input.ISBN;
            if (input.Description != null) book.Description = input.Description;
            if (input.PublishedDate.HasValue) book.PublishedDate = input.PublishedDate.Value;
            if (input.Pages.HasValue) book.Pages = input.Pages.Value;
            if (input.Price.HasValue) book.Price = input.Price.Value;
            if (input.Publisher != null) book.Publisher = input.Publisher;
            if (input.Language != null) book.Language = input.Language;
            if (input.CopiesTotal.HasValue) book.CopiesTotal = input.CopiesTotal.Value;
            if (input.CopiesAvailable.HasValue) 
            {
                book.CopiesAvailable = input.CopiesAvailable.Value;
                book.IsAvailable = input.CopiesAvailable.Value > 0;
            }
            if (input.AuthorId.HasValue) book.AuthorId = input.AuthorId.Value;
            if (input.CategoryId.HasValue) book.CategoryId = input.CategoryId.Value;

            book.UpdatedAt = DateTime.UtcNow;

            // Update tags if provided
            if (input.TagIds != null)
            {
                var existingTags = _context.BookTags.Where(bt => bt.BookId == book.Id);
                _context.BookTags.RemoveRange(existingTags);

                var newTags = input.TagIds.Select(tagId => new BookTag
                {
                    BookId = book.Id,
                    TagId = tagId,
                    CreatedAt = DateTime.UtcNow
                }).ToList();

                _context.BookTags.AddRange(newTags);
            }

            await _context.SaveChangesAsync();
            return await GetByIdAsync(book.Id);
        }

        public async Task<bool> DeleteAsync(int id)
        {
            var book = await _context.Books.FindAsync(id);
            if (book == null) return false;

            _context.Books.Remove(book);
            await _context.SaveChangesAsync();
            return true;
        }

        public Task<IQueryable<Book>> SearchAsync(BookFilterInput filter)
        {
            var query = _context.Books
                .Include(b => b.Author)
                .Include(b => b.Category)
                .Include(b => b.Reviews)
                .Include(b => b.BookTags)
                    .ThenInclude(bt => bt.Tag)
                .AsQueryable();

            if (!string.IsNullOrEmpty(filter.Title))
                query = query.Where(b => b.Title.Contains(filter.Title));

            if (!string.IsNullOrEmpty(filter.ISBN))
                query = query.Where(b => b.ISBN == filter.ISBN);

            if (filter.AuthorId.HasValue)
                query = query.Where(b => b.AuthorId == filter.AuthorId);

            if (filter.CategoryId.HasValue)
                query = query.Where(b => b.CategoryId == filter.CategoryId);

            if (filter.IsAvailable.HasValue)
                query = query.Where(b => b.IsAvailable == filter.IsAvailable);

            if (filter.MinPrice.HasValue)
                query = query.Where(b => b.Price >= filter.MinPrice);

            if (filter.MaxPrice.HasValue)
                query = query.Where(b => b.Price <= filter.MaxPrice);

            if (filter.PublishedAfter.HasValue)
                query = query.Where(b => b.PublishedDate >= filter.PublishedAfter);

            if (filter.PublishedBefore.HasValue)
                query = query.Where(b => b.PublishedDate <= filter.PublishedBefore);

            if (!string.IsNullOrEmpty(filter.Language))
                query = query.Where(b => b.Language == filter.Language);

            if (filter.MinPages.HasValue)
                query = query.Where(b => b.Pages >= filter.MinPages);

            if (filter.MaxPages.HasValue)
                query = query.Where(b => b.Pages <= filter.MaxPages);

            if (filter.TagIds != null && filter.TagIds.Any())
                query = query.Where(b => b.BookTags.Any(bt => filter.TagIds.Contains(bt.TagId)));

            if (filter.MinRating.HasValue)
                query = query.Where(b => b.Reviews.Any() && b.Reviews.Average(r => r.Rating) >= filter.MinRating);

            return Task.FromResult(query);
        }

        public async Task<bool> UpdateAvailabilityAsync(int bookId, int copiesChange)
        {
            var book = await _context.Books.FindAsync(bookId);
            if (book == null) return false;

            book.CopiesAvailable += copiesChange;
            
            if (book.CopiesAvailable < 0)
                book.CopiesAvailable = 0;
            
            if (book.CopiesAvailable > book.CopiesTotal)
                book.CopiesAvailable = book.CopiesTotal;

            book.IsAvailable = book.CopiesAvailable > 0;
            book.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();
            return true;
        }
    }
}
