using AutoMapper;
using FluentValidation;
using GraphQLSimple.Extensions;
using GraphQLSimple.Models;
using GraphQLSimple.Services;
using GraphQLSimple.GraphQL.Types;
using HotChocolate.Subscriptions;

namespace GraphQLSimple.GraphQL
{
    public class Mutation
    {
        // Book Mutations
        public async Task<Book> CreateBook(
            CreateBookInput input,
            [Service] BookService bookService,
            [Service] IValidator<CreateBookInput> validator,
            [Service] ITopicEventSender eventSender)
        {
            var validationResult = await validator.ValidateAsync(input);
            if (!validationResult.IsValid)
            {
                throw new GraphQLSimple.Extensions.ValidationException(validationResult.Errors.First().ErrorMessage);
            }

            var book = await bookService.CreateAsync(input);
            
            // Trigger subscription event
            await eventSender.SendAsync("BookCreated", book);

            return book;
        }

        public async Task<Book?> UpdateBook(
            UpdateBookInput input,
            [Service] BookService bookService,
            [Service] IValidator<UpdateBookInput> validator,
            [Service] ITopicEventSender eventSender)
        {
            var validationResult = await validator.ValidateAsync(input);
            if (!validationResult.IsValid)
            {
                throw new GraphQLSimple.Extensions.ValidationException(validationResult.Errors.First().ErrorMessage);
            }

            var book = await bookService.UpdateAsync(input);
            
            // Trigger subscription event if update was successful
            if (book != null)
            {
                await eventSender.SendAsync("BookUpdated", book);
            }

            return book;
        }

        public async Task<bool> DeleteBook(
            int id,
            [Service] BookService bookService)
        {
            return await bookService.DeleteAsync(id);
        }

        // Author Mutations
        public async Task<Author> CreateAuthor(
            CreateAuthorInput input,
            [Service] AuthorService authorService,
            [Service] IValidator<CreateAuthorInput> validator)
        {
            var validationResult = await validator.ValidateAsync(input);
            if (!validationResult.IsValid)
            {
                throw new GraphQLSimple.Extensions.ValidationException(validationResult.Errors.First().ErrorMessage);
            }

            return await authorService.CreateAsync(input);
        }

        public async Task<Author?> UpdateAuthor(
            UpdateAuthorInput input,
            [Service] AuthorService authorService,
            [Service] IValidator<UpdateAuthorInput> validator)
        {
            var validationResult = await validator.ValidateAsync(input);
            if (!validationResult.IsValid)
            {
                throw new GraphQLSimple.Extensions.ValidationException(validationResult.Errors.First().ErrorMessage);
            }

            return await authorService.UpdateAsync(input);
        }

        public async Task<bool> DeleteAuthor(
            int id,
            [Service] AuthorService authorService)
        {
            return await authorService.DeleteAsync(id);
        }

        // User Mutations
        public async Task<User> CreateUser(
            CreateUserInput input,
            [Service] IValidator<CreateUserInput> validator)
        {
            var validationResult = await validator.ValidateAsync(input);
            if (!validationResult.IsValid)
            {
                throw new GraphQLSimple.Extensions.ValidationException(validationResult.Errors.First().ErrorMessage);
            }

            // For this demo, we'll create a simple user creation method
            var user = new User
            {
                FirstName = input.FirstName,
                LastName = input.LastName,
                Email = input.Email,
                PhoneNumber = input.PhoneNumber,
                DateOfBirth = input.DateOfBirth,
                UserType = input.UserType,
                Address = input.Address
            };

            return user;
        }

        // Borrowing Mutations
        public async Task<Borrowing?> BorrowBook(
            CreateBorrowingInput input,
            [Service] BorrowingService borrowingService,
            [Service] IValidator<CreateBorrowingInput> validator,
            [Service] ITopicEventSender eventSender)
        {
            var validationResult = await validator.ValidateAsync(input);
            if (!validationResult.IsValid)
            {
                throw new GraphQLSimple.Extensions.ValidationException(validationResult.Errors.First().ErrorMessage);
            }

            var borrowing = await borrowingService.CreateAsync(input);
            
            if (borrowing != null)
            {
                await eventSender.SendAsync("BookBorrowed", borrowing);
            }

            return borrowing;
        }

        public async Task<Borrowing?> ReturnBook(
            int borrowingId,
            [Service] BorrowingService borrowingService,
            [Service] ITopicEventSender eventSender)
        {
            var borrowing = await borrowingService.ReturnBookAsync(borrowingId);
            
            if (borrowing != null)
            {
                await eventSender.SendAsync("BookReturned", borrowing);
            }

            return borrowing;
        }

        // Review Mutations
        public async Task<Review> CreateReview(
            CreateReviewInput input,
            [Service] IValidator<CreateReviewInput> validator,
            [Service] ITopicEventSender eventSender)
        {
            var validationResult = await validator.ValidateAsync(input);
            if (!validationResult.IsValid)
            {
                throw new GraphQLSimple.Extensions.ValidationException(validationResult.Errors.First().ErrorMessage);
            }

            var review = new Review
            {
                BookId = input.BookId,
                UserId = input.UserId,
                Rating = input.Rating,
                Comment = input.Comment,
                IsVerifiedPurchase = input.IsVerifiedPurchase
            };

            await eventSender.SendAsync("ReviewCreated", review);

            return review;
        }
    }
}