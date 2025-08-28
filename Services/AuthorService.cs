using Microsoft.EntityFrameworkCore;
using GraphQLSimple.Data;
using GraphQLSimple.Models;
using GraphQLSimple.GraphQL.Types;

namespace GraphQLSimple.Services
{
    public interface IAuthorService
    {
        Task<Author?> GetByIdAsync(int id);
        Task<IQueryable<Author>> GetAllAsync();
        Task<Author> CreateAsync(CreateAuthorInput input);
        Task<Author?> UpdateAsync(UpdateAuthorInput input);
        Task<bool> DeleteAsync(int id);
    }

    public class AuthorService : IAuthorService
    {
        private readonly LibraryContext _context;

        public AuthorService(LibraryContext context)
        {
            _context = context;
        }

        public async Task<Author?> GetByIdAsync(int id)
        {
            return await _context.Authors
                .Include(a => a.Books)
                    .ThenInclude(b => b.Reviews)
                .FirstOrDefaultAsync(a => a.Id == id);
        }

        public Task<IQueryable<Author>> GetAllAsync()
        {
            var query = _context.Authors
                .Include(a => a.Books)
                    .ThenInclude(b => b.Reviews);
            
            return Task.FromResult(query.AsQueryable());
        }

        public async Task<Author> CreateAsync(CreateAuthorInput input)
        {
            var author = new Author
            {
                FirstName = input.FirstName,
                LastName = input.LastName,
                DateOfBirth = input.DateOfBirth,
                Biography = input.Biography,
                Nationality = input.Nationality,
                Email = input.Email,
                IsActive = true,
                CreatedAt = DateTime.UtcNow
            };

            _context.Authors.Add(author);
            await _context.SaveChangesAsync();

            return await GetByIdAsync(author.Id) ?? author;
        }

        public async Task<Author?> UpdateAsync(UpdateAuthorInput input)
        {
            var author = await _context.Authors.FindAsync(input.Id);
            if (author == null) return null;

            if (input.FirstName != null) author.FirstName = input.FirstName;
            if (input.LastName != null) author.LastName = input.LastName;
            if (input.DateOfBirth.HasValue) author.DateOfBirth = input.DateOfBirth.Value;
            if (input.Biography != null) author.Biography = input.Biography;
            if (input.Nationality != null) author.Nationality = input.Nationality;
            if (input.Email != null) author.Email = input.Email;
            if (input.IsActive.HasValue) author.IsActive = input.IsActive.Value;

            author.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();
            return await GetByIdAsync(author.Id);
        }

        public async Task<bool> DeleteAsync(int id)
        {
            var author = await _context.Authors.FindAsync(id);
            if (author == null) return false;

            // Check if author has books
            var hasBooks = await _context.Books.AnyAsync(b => b.AuthorId == id);
            if (hasBooks)
            {
                // Soft delete - mark as inactive
                author.IsActive = false;
                author.UpdatedAt = DateTime.UtcNow;
                await _context.SaveChangesAsync();
            }
            else
            {
                // Hard delete if no books
                _context.Authors.Remove(author);
                await _context.SaveChangesAsync();
            }

            return true;
        }
    }
}
