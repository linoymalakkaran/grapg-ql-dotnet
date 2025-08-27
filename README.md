# GraphQL Learning Project with .NET 8

This project demonstrates a comprehensive GraphQL API implementation using .NET 8, HotChocolate, and Entity Framework Core with SQLite.

## Project Structure

```
GraphQLSimple/
├── Models/          # Data models (Book, Author, Review)
├── Data/            # Entity Framework DbContext
├── GraphQL/         # GraphQL Types (Query, Mutation)
└── Program.cs       # Application startup
```

## Features

- ✅ **GraphQL API** with HotChocolate
- ✅ **Entity Framework Core** with SQLite
- ✅ **Seed Data** for testing
- ✅ **CRUD Operations** via GraphQL
- ✅ **Relationships** between entities
- ✅ **Filtering and Sorting** capabilities

## Getting Started

1. **Clone and Run**:
   ```bash
   git clone <your-repo>
   cd GraphQLSimple
   dotnet restore
   dotnet run
   ```

2. **Access GraphQL Playground**:
   Navigate to `http://localhost:5208/graphql` in your browser

## Sample GraphQL Queries

### 1. Get All Books with Authors
```graphql
query {
  books {
    id
    title
    isbn
    pages
    publishedDate
    author {
      id
      firstName
      lastName
    }
    reviews {
      id
      reviewerName
      rating
      comment
    }
  }
}
```

### 2. Get Specific Author with Books
```graphql
query {
  author(id: 1) {
    id
    firstName
    lastName
    dateOfBirth
    biography
    books {
      id
      title
      isbn
    }
  }
}
```

### 3. Get All Authors
```graphql
query {
  authors {
    id
    firstName
    lastName
    books {
      title
    }
  }
}
```

### 4. Get All Reviews
```graphql
query {
  reviews {
    id
    reviewerName
    rating
    comment
    createdAt
    book {
      title
      author {
        firstName
        lastName
      }
    }
  }
}
```

## Sample Mutations

### 1. Add New Author
```graphql
mutation {
  addAuthor(
    firstName: "Agatha"
    lastName: "Christie"
    dateOfBirth: "1890-09-15T00:00:00"
    biography: "British crime novelist"
  ) {
    id
    firstName
    lastName
  }
}
```

### 2. Add New Book
```graphql
mutation {
  addBook(
    title: "Murder on the Orient Express"
    isbn: "978-0007119318"
    publishedDate: "1934-01-01T00:00:00"
    pages: 256
    authorId: 3
  ) {
    id
    title
    author {
      firstName
      lastName
    }
  }
}
```

### 3. Add New Review
```graphql
mutation {
  addReview(
    bookId: 1
    reviewerName: "John Doe"
    rating: 5
    comment: "Absolutely fantastic!"
  ) {
    id
    reviewerName
    rating
    book {
      title
    }
  }
}
```

## Advanced Queries with Filtering

HotChocolate automatically provides filtering capabilities:

```graphql
query {
  books(where: { 
    author: { 
      firstName: { eq: "J.K." } 
    } 
  }) {
    title
    author {
      firstName
      lastName
    }
  }
}
```

## Technology Stack

- **.NET 8** - Latest .NET framework
- **HotChocolate 15.1.9** - GraphQL server
- **Entity Framework Core 9.0** - ORM
- **SQLite** - Database (portable, no setup required)

## Database Schema

The application creates the following tables:
- **Authors** (Id, FirstName, LastName, DateOfBirth, Biography)
- **Books** (Id, Title, ISBN, PublishedDate, Pages, AuthorId)
- **Reviews** (Id, BookId, ReviewerName, Rating, Comment, CreatedAt)

## Sample Data Included

The application comes with sample data:
- **Authors**: J.K. Rowling, George Orwell
- **Books**: Harry Potter and the Philosopher's Stone, 1984
- **Reviews**: Several reviews for the sample books

## Next Steps for Learning

1. **Try the sample queries** in the GraphQL playground
2. **Experiment with filtering** using `where` clauses
3. **Add more complex relationships** between entities
4. **Implement authentication** and authorization
5. **Add subscriptions** for real-time updates
6. **Add custom scalars** for specialized data types
7. **Implement DataLoaders** to solve N+1 query problems

## Troubleshooting

- Make sure you're running from the `GraphQLSimple` directory
- The SQLite database file (`library.db`) will be created automatically
- If you get port conflicts, the application will use a different port automatically

Happy GraphQL learning! 🚀
