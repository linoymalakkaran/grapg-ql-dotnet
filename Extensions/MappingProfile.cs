using AutoMapper;
using GraphQLSimple.Models;
using GraphQLSimple.GraphQL.Types;

namespace GraphQLSimple.Extensions
{
    public class MappingProfile : Profile
    {
        public MappingProfile()
        {
            // Book mappings
            CreateMap<CreateBookInput, Book>()
                .ForMember(dest => dest.Id, opt => opt.Ignore())
                .ForMember(dest => dest.CreatedAt, opt => opt.Ignore())
                .ForMember(dest => dest.UpdatedAt, opt => opt.Ignore())
                .ForMember(dest => dest.Author, opt => opt.Ignore())
                .ForMember(dest => dest.Category, opt => opt.Ignore())
                .ForMember(dest => dest.Reviews, opt => opt.Ignore())
                .ForMember(dest => dest.BookTags, opt => opt.Ignore())
                .ForMember(dest => dest.Borrowings, opt => opt.Ignore())
                .ForMember(dest => dest.IsAvailable, opt => opt.MapFrom(src => src.CopiesAvailable > 0));

            CreateMap<UpdateBookInput, Book>()
                .ForMember(dest => dest.Id, opt => opt.Ignore())
                .ForMember(dest => dest.CreatedAt, opt => opt.Ignore())
                .ForMember(dest => dest.UpdatedAt, opt => opt.Ignore())
                .ForMember(dest => dest.Author, opt => opt.Ignore())
                .ForMember(dest => dest.Category, opt => opt.Ignore())
                .ForMember(dest => dest.Reviews, opt => opt.Ignore())
                .ForMember(dest => dest.BookTags, opt => opt.Ignore())
                .ForMember(dest => dest.Borrowings, opt => opt.Ignore())
                .ForAllMembers(opts => opts.Condition((src, dest, srcMember) => srcMember != null));

            // Author mappings
            CreateMap<CreateAuthorInput, Author>()
                .ForMember(dest => dest.Id, opt => opt.Ignore())
                .ForMember(dest => dest.CreatedAt, opt => opt.Ignore())
                .ForMember(dest => dest.UpdatedAt, opt => opt.Ignore())
                .ForMember(dest => dest.Books, opt => opt.Ignore());

            CreateMap<UpdateAuthorInput, Author>()
                .ForMember(dest => dest.Id, opt => opt.Ignore())
                .ForMember(dest => dest.CreatedAt, opt => opt.Ignore())
                .ForMember(dest => dest.UpdatedAt, opt => opt.Ignore())
                .ForMember(dest => dest.Books, opt => opt.Ignore())
                .ForAllMembers(opts => opts.Condition((src, dest, srcMember) => srcMember != null));

            // User mappings
            CreateMap<CreateUserInput, User>()
                .ForMember(dest => dest.Id, opt => opt.Ignore())
                .ForMember(dest => dest.CreatedAt, opt => opt.Ignore())
                .ForMember(dest => dest.UpdatedAt, opt => opt.Ignore())
                .ForMember(dest => dest.Reviews, opt => opt.Ignore())
                .ForMember(dest => dest.Borrowings, opt => opt.Ignore());

            CreateMap<UpdateUserInput, User>()
                .ForMember(dest => dest.Id, opt => opt.Ignore())
                .ForMember(dest => dest.CreatedAt, opt => opt.Ignore())
                .ForMember(dest => dest.UpdatedAt, opt => opt.Ignore())
                .ForMember(dest => dest.Reviews, opt => opt.Ignore())
                .ForMember(dest => dest.Borrowings, opt => opt.Ignore())
                .ForAllMembers(opts => opts.Condition((src, dest, srcMember) => srcMember != null));

            // Review mappings
            CreateMap<CreateReviewInput, Review>()
                .ForMember(dest => dest.Id, opt => opt.Ignore())
                .ForMember(dest => dest.CreatedAt, opt => opt.Ignore())
                .ForMember(dest => dest.UpdatedAt, opt => opt.Ignore())
                .ForMember(dest => dest.Book, opt => opt.Ignore())
                .ForMember(dest => dest.User, opt => opt.Ignore());

            CreateMap<UpdateReviewInput, Review>()
                .ForMember(dest => dest.Id, opt => opt.Ignore())
                .ForMember(dest => dest.CreatedAt, opt => opt.Ignore())
                .ForMember(dest => dest.UpdatedAt, opt => opt.Ignore())
                .ForMember(dest => dest.Book, opt => opt.Ignore())
                .ForMember(dest => dest.User, opt => opt.Ignore())
                .ForAllMembers(opts => opts.Condition((src, dest, srcMember) => srcMember != null));

            // Category mappings
            CreateMap<CreateCategoryInput, Category>()
                .ForMember(dest => dest.Id, opt => opt.Ignore())
                .ForMember(dest => dest.CreatedAt, opt => opt.Ignore())
                .ForMember(dest => dest.Books, opt => opt.Ignore())
                .ForMember(dest => dest.ParentCategory, opt => opt.Ignore())
                .ForMember(dest => dest.SubCategories, opt => opt.Ignore());

            // Models.Tag mappings
            CreateMap<CreateTagInput, Models.Tag>()
                .ForMember(dest => dest.Id, opt => opt.Ignore())
                .ForMember(dest => dest.CreatedAt, opt => opt.Ignore())
                .ForMember(dest => dest.BookTags, opt => opt.Ignore());

            CreateMap<UpdateTagInput, Models.Tag>()
                .ForMember(dest => dest.Id, opt => opt.Ignore())
                .ForMember(dest => dest.CreatedAt, opt => opt.Ignore())
                .ForMember(dest => dest.BookTags, opt => opt.Ignore())
                .ForAllMembers(opts => opts.Condition((src, dest, srcMember) => srcMember != null));

            // Borrowing mappings
            CreateMap<CreateBorrowingInput, Borrowing>()
                .ForMember(dest => dest.Id, opt => opt.Ignore())
                .ForMember(dest => dest.Book, opt => opt.Ignore())
                .ForMember(dest => dest.User, opt => opt.Ignore());

            CreateMap<UpdateBorrowingInput, Borrowing>()
                .ForMember(dest => dest.Id, opt => opt.Ignore())
                .ForMember(dest => dest.Book, opt => opt.Ignore())
                .ForMember(dest => dest.User, opt => opt.Ignore())
                .ForAllMembers(opts => opts.Condition((src, dest, srcMember) => srcMember != null));
        }
    }
}
