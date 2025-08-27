using Microsoft.EntityFrameworkCore;
using GraphQLSimple.Data;
using GraphQLSimple.Models;

namespace GraphQLSimple.GraphQL
{
    public class Query
    {
        public async Task<IEnumerable<Author>> GetAuthors(LibraryContext context)
        {
            return await context.Authors.Include(a => a.Books).ToListAsync();
        }

        public async Task<IEnumerable<Book>> GetBooks(LibraryContext context)
        {
            return await context.Books
                .Include(b => b.Author)
                .Include(b => b.Reviews)
                .ToListAsync();
        }

        public async Task<IEnumerable<Review>> GetReviews(LibraryContext context)
        {
            return await context.Reviews.Include(r => r.Book).ToListAsync();
        }

        public async Task<Author?> GetAuthor(int id, LibraryContext context)
        {
            return await context.Authors
                .Include(a => a.Books)
                .FirstOrDefaultAsync(a => a.Id == id);
        }

        public async Task<Book?> GetBook(int id, LibraryContext context)
        {
            return await context.Books
                .Include(b => b.Author)
                .Include(b => b.Reviews)
                .FirstOrDefaultAsync(b => b.Id == id);
        }
    }
}
