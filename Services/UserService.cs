using Microsoft.EntityFrameworkCore;
using GraphQLSimple.Data;
using GraphQLSimple.Models;
using GraphQLSimple.GraphQL.Types;

namespace GraphQLSimple.Services
{
    public interface IUserService
    {
        Task<User?> GetByIdAsync(int id);
        Task<User?> GetByEmailAsync(string email);
        Task<IQueryable<User>> GetAllAsync();
        Task<User> CreateAsync(CreateUserInput input);
        Task<User?> UpdateAsync(UpdateUserInput input);
        Task<bool> DeleteAsync(int id);
        Task<bool> ActivateUserAsync(int id);
        Task<bool> DeactivateUserAsync(int id);
    }

    public class UserService : IUserService
    {
        private readonly LibraryContext _context;

        public UserService(LibraryContext context)
        {
            _context = context;
        }

        public async Task<User?> GetByIdAsync(int id)
        {
            return await _context.Users
                .Include(u => u.Reviews)
                .Include(u => u.Borrowings)
                .FirstOrDefaultAsync(u => u.Id == id);
        }

        public async Task<User?> GetByEmailAsync(string email)
        {
            return await _context.Users
                .Include(u => u.Reviews)
                .Include(u => u.Borrowings)
                .FirstOrDefaultAsync(u => u.Email.ToLower() == email.ToLower());
        }

        public async Task<IQueryable<User>> GetAllAsync()
        {
            return _context.Users
                .Include(u => u.Reviews)
                .Include(u => u.Borrowings);
        }

        public async Task<User> CreateAsync(CreateUserInput input)
        {
            var user = new User
            {
                FirstName = input.FirstName,
                LastName = input.LastName,
                Email = input.Email.ToLower(), // Normalize email
                PhoneNumber = input.PhoneNumber,
                DateOfBirth = input.DateOfBirth,
                UserType = input.UserType,
                Address = input.Address,
                IsActive = true,
                CreatedAt = DateTime.UtcNow
            };

            _context.Users.Add(user);
            await _context.SaveChangesAsync();
            return user;
        }

        public async Task<User?> UpdateAsync(UpdateUserInput input)
        {
            var user = await _context.Users.FindAsync(input.Id);
            if (user == null) return null;

            // Update only provided fields
            if (input.FirstName != null) user.FirstName = input.FirstName;
            if (input.LastName != null) user.LastName = input.LastName;
            if (input.Email != null) user.Email = input.Email.ToLower();
            if (input.PhoneNumber != null) user.PhoneNumber = input.PhoneNumber;
            if (input.DateOfBirth.HasValue) user.DateOfBirth = input.DateOfBirth.Value;
            if (input.UserType.HasValue) user.UserType = input.UserType.Value;
            if (input.Address != null) user.Address = input.Address;
            if (input.IsActive.HasValue) user.IsActive = input.IsActive.Value;
            
            user.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();
            return user;
        }

        public async Task<bool> DeleteAsync(int id)
        {
            var user = await _context.Users.FindAsync(id);
            if (user == null) return false;

            // Soft delete - just deactivate the user
            user.IsActive = false;
            user.UpdatedAt = DateTime.UtcNow;
            
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> ActivateUserAsync(int id)
        {
            var user = await _context.Users.FindAsync(id);
            if (user == null) return false;

            user.IsActive = true;
            user.UpdatedAt = DateTime.UtcNow;
            
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> DeactivateUserAsync(int id)
        {
            var user = await _context.Users.FindAsync(id);
            if (user == null) return false;

            user.IsActive = false;
            user.UpdatedAt = DateTime.UtcNow;
            
            await _context.SaveChangesAsync();
            return true;
        }
    }
}
