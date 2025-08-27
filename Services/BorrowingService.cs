using Microsoft.EntityFrameworkCore;
using GraphQLSimple.Data;
using GraphQLSimple.Models;
using GraphQLSimple.GraphQL.Types;

namespace GraphQLSimple.Services
{
    public interface IBorrowingService
    {
        Task<Borrowing?> GetByIdAsync(int id);
        Task<IQueryable<Borrowing>> GetAllAsync();
        Task<Borrowing?> CreateAsync(CreateBorrowingInput input);
        Task<Borrowing?> UpdateAsync(UpdateBorrowingInput input);
        Task<Borrowing?> ReturnBookAsync(int borrowingId, DateTime? returnDate = null);
        Task<bool> DeleteAsync(int id);
        Task<IQueryable<Borrowing>> GetByUserAsync(int userId);
        Task<IQueryable<Borrowing>> GetByBookAsync(int bookId);
        Task<IQueryable<Borrowing>> GetOverdueAsync();
        Task<decimal> CalculateFineAsync(int borrowingId);
    }

    public class BorrowingService : IBorrowingService
    {
        private readonly LibraryContext _context;
        private readonly IBookService _bookService;

        public BorrowingService(LibraryContext context, IBookService bookService)
        {
            _context = context;
            _bookService = bookService;
        }

        public async Task<Borrowing?> GetByIdAsync(int id)
        {
            return await _context.Borrowings
                .Include(b => b.Book)
                    .ThenInclude(book => book.Author)
                .Include(b => b.User)
                .FirstOrDefaultAsync(b => b.Id == id);
        }

        public Task<IQueryable<Borrowing>> GetAllAsync()
        {
            var query = _context.Borrowings
                .Include(b => b.Book)
                    .ThenInclude(book => book.Author)
                .Include(b => b.User);
            
            return Task.FromResult(query.AsQueryable());
        }

        public async Task<Borrowing?> CreateAsync(CreateBorrowingInput input)
        {
            // Check if book is available
            var book = await _context.Books.FindAsync(input.BookId);
            if (book == null || book.CopiesAvailable <= 0)
                return null;

            // Check if user has reached borrowing limit (e.g., 5 books)
            var activeBorrowings = await _context.Borrowings
                .CountAsync(b => b.UserId == input.UserId && b.Status == BorrowingStatus.Active);
            
            if (activeBorrowings >= 5) // Maximum 5 active borrowings per user
                return null;

            var borrowing = new Borrowing
            {
                BookId = input.BookId,
                UserId = input.UserId,
                BorrowedDate = DateTime.UtcNow,
                DueDate = input.DueDate,
                Status = BorrowingStatus.Active,
                Notes = input.Notes
            };

            _context.Borrowings.Add(borrowing);
            
            // Decrease available copies
            await _bookService.UpdateAvailabilityAsync(input.BookId, -1);
            
            await _context.SaveChangesAsync();

            return await GetByIdAsync(borrowing.Id);
        }

        public async Task<Borrowing?> UpdateAsync(UpdateBorrowingInput input)
        {
            var borrowing = await _context.Borrowings.FindAsync(input.Id);
            if (borrowing == null) return null;

            if (input.ReturnedDate.HasValue) borrowing.ReturnedDate = input.ReturnedDate.Value;
            if (input.Status.HasValue) borrowing.Status = input.Status.Value;
            if (input.FineAmount.HasValue) borrowing.FineAmount = input.FineAmount.Value;
            if (input.Notes != null) borrowing.Notes = input.Notes;

            await _context.SaveChangesAsync();
            return await GetByIdAsync(borrowing.Id);
        }

        public async Task<Borrowing?> ReturnBookAsync(int borrowingId, DateTime? returnDate = null)
        {
            var borrowing = await _context.Borrowings.FindAsync(borrowingId);
            if (borrowing == null || borrowing.Status == BorrowingStatus.Returned)
                return null;

            var actualReturnDate = returnDate ?? DateTime.UtcNow;
            borrowing.ReturnedDate = actualReturnDate;
            borrowing.Status = BorrowingStatus.Returned;

            // Calculate fine if overdue
            if (actualReturnDate > borrowing.DueDate)
            {
                var daysLate = (actualReturnDate - borrowing.DueDate).Days;
                borrowing.FineAmount = daysLate * 0.50m; // $0.50 per day fine
            }

            // Increase available copies
            await _bookService.UpdateAvailabilityAsync(borrowing.BookId, 1);

            await _context.SaveChangesAsync();
            return await GetByIdAsync(borrowing.Id);
        }

        public async Task<bool> DeleteAsync(int id)
        {
            var borrowing = await _context.Borrowings.FindAsync(id);
            if (borrowing == null) return false;

            // If book is still borrowed, return it first
            if (borrowing.Status == BorrowingStatus.Active)
            {
                await ReturnBookAsync(id);
            }

            _context.Borrowings.Remove(borrowing);
            await _context.SaveChangesAsync();
            return true;
        }

        public Task<IQueryable<Borrowing>> GetByUserAsync(int userId)
        {
            var query = _context.Borrowings
                .Include(b => b.Book)
                    .ThenInclude(book => book.Author)
                .Where(b => b.UserId == userId);
            
            return Task.FromResult(query);
        }

        public Task<IQueryable<Borrowing>> GetByBookAsync(int bookId)
        {
            var query = _context.Borrowings
                .Include(b => b.User)
                .Where(b => b.BookId == bookId);
            
            return Task.FromResult(query);
        }

        public Task<IQueryable<Borrowing>> GetOverdueAsync()
        {
            var query = _context.Borrowings
                .Include(b => b.Book)
                    .ThenInclude(book => book.Author)
                .Include(b => b.User)
                .Where(b => b.Status == BorrowingStatus.Active && b.DueDate < DateTime.UtcNow);
            
            return Task.FromResult(query);
        }

        public async Task<decimal> CalculateFineAsync(int borrowingId)
        {
            var borrowing = await _context.Borrowings.FindAsync(borrowingId);
            if (borrowing == null || borrowing.Status != BorrowingStatus.Active)
                return 0;

            if (DateTime.UtcNow <= borrowing.DueDate)
                return 0;

            var daysLate = (DateTime.UtcNow - borrowing.DueDate).Days;
            return daysLate * 0.50m; // $0.50 per day fine
        }
    }
}
