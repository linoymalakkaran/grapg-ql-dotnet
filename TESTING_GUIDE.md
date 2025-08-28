# 🧪 GraphQL Endpoints Testing Guide

## 🚀 How to Test GraphQL Endpoints

### **Method 1: Using the Built-in GraphQL IDE (Recommended)**

#### **Step 1: Access the GraphQL IDE**
1. Make sure your server is running: `dotnet run --urls "http://localhost:5000"`
2. Open your browser and go to: **http://localhost:5000/graphql**
3. You'll see the **Banana Cake Pop** GraphQL IDE

#### **Step 2: Explore the Schema**
- Click the **"Schema"** tab to see all available types, queries, mutations, and subscriptions
- Use the **"Schema Explorer"** to browse available fields
- The schema is automatically documented

---

### **Method 2: Using PowerShell/Command Line**

#### **Basic Query Test**
```powershell
$headers = @{ "Content-Type" = "application/json" }
$body = @{
    query = "{ authors { id fullName email } }"
} | ConvertTo-Json

Invoke-RestMethod -Uri "http://localhost:5000/graphql" -Method POST -Headers $headers -Body $body
```

#### **Query with Variables**
```powershell
$headers = @{ "Content-Type" = "application/json" }
$body = @{
    query = "query GetBook($id: Int!) { book(id: $id) { title author { fullName } } }"
    variables = @{ id = 1 }
} | ConvertTo-Json -Depth 3

Invoke-RestMethod -Uri "http://localhost:5000/graphql" -Method POST -Headers $headers -Body $body
```

---

### **Method 3: Using curl**

#### **Simple Query**
```bash
curl -X POST \
  -H "Content-Type: application/json" \
  -d '{"query":"{ authors { id fullName } }"}' \
  http://localhost:5000/graphql
```

#### **Mutation Example**
```bash
curl -X POST \
  -H "Content-Type: application/json" \
  -d '{
    "query": "mutation { createAuthor(input: { firstName: \"Test\", lastName: \"Author\", email: \"test@example.com\" }) { author { id fullName } errors { message } } }"
  }' \
  http://localhost:5000/graphql
```

---

### **Method 4: Using Postman**

#### **Setup:**
1. Create a new **POST** request
2. URL: `http://localhost:5000/graphql`
3. Headers: `Content-Type: application/json`
4. Body: Raw JSON

#### **Example Request Body:**
```json
{
  "query": "{ books { id title author { fullName } averageRating } }"
}
```

---

## 🔍 **Test Scenarios**

### **1. Basic Queries**

#### **Get All Authors**
```graphql
{
  authors {
    id
    fullName
    email
    bio
    birthDate
    nationality
  }
}
```

#### **Get Books with Pagination**
```graphql
{
  books(first: 5) {
    nodes {
      id
      title
      isbn
      publishedDate
      averageRating
      reviewCount
      author {
        fullName
      }
      category {
        name
      }
    }
    pageInfo {
      hasNextPage
      hasPreviousPage
      startCursor
      endCursor
    }
  }
}
```

#### **Search Books with Filtering**
```graphql
{
  books(
    where: { 
      title: { contains: "Programming" }
      isAvailable: { eq: true }
    }
    order: { publishedDate: DESC }
    first: 10
  ) {
    nodes {
      title
      publishedDate
      isAvailable
      author {
        fullName
      }
    }
  }
}
```

### **2. Complex Queries with DataLoaders**

#### **Authors with their Books (Tests DataLoader)**
```graphql
{
  authors {
    id
    fullName
    books {
      title
      publishedDate
      reviews {
        rating
        comment
        user {
          fullName
        }
      }
    }
  }
}
```

#### **Users with Borrowing History**
```graphql
{
  users {
    id
    fullName
    email
    borrowings {
      borrowedDate
      returnDate
      isReturned
      book {
        title
        author {
          fullName
        }
      }
    }
  }
}
```

### **3. Mutations**

#### **Create a New Author**
```graphql
mutation {
  createAuthor(input: {
    firstName: "Jane"
    lastName: "Doe"
    email: "jane.doe@example.com"
    bio: "A talented writer"
    birthDate: "1980-05-15"
    nationality: "American"
  }) {
    author {
      id
      fullName
      email
    }
    errors {
      message
      code
    }
  }
}
```

#### **Create a New Book**
```graphql
mutation {
  createBook(input: {
    title: "Advanced GraphQL Testing"
    isbn: "978-1234567890"
    authorId: 1
    categoryId: 1
    publishedDate: "2024-01-15"
    description: "A comprehensive guide to GraphQL testing"
    pages: 300
    language: "English"
    copiesTotal: 10
    copiesAvailable: 10
  }) {
    book {
      id
      title
      author {
        fullName
      }
      category {
        name
      }
    }
    errors {
      message
    }
  }
}
```

#### **Add a Book Review**
```graphql
mutation {
  addReview(input: {
    bookId: 1
    userId: 1
    rating: 5
    comment: "Excellent book! Highly recommended."
  }) {
    review {
      id
      rating
      comment
      createdAt
      book {
        title
        averageRating
      }
      user {
        fullName
      }
    }
  }
}
```

#### **Borrow a Book**
```graphql
mutation {
  borrowBook(input: {
    bookId: 1
    userId: 1
  }) {
    borrowing {
      id
      borrowedDate
      dueDate
      book {
        title
        copiesAvailable
      }
      user {
        fullName
      }
    }
    errors {
      message
    }
  }
}
```

### **4. Subscriptions (Real-time)**

#### **Subscribe to New Books**
```graphql
subscription {
  onBookAdded {
    id
    title
    author {
      fullName
    }
    publishedDate
  }
}
```

#### **Subscribe to New Reviews**
```graphql
subscription {
  onReviewAdded {
    id
    rating
    comment
    book {
      title
    }
    user {
      fullName
    }
  }
}
```

### **5. Health and Schema Endpoints**

#### **Health Check**
- **URL:** `http://localhost:5000/health`
- **Method:** GET
- **Response:** JSON with health status

#### **Schema SDL**
- **URL:** `http://localhost:5000/schema`
- **Method:** GET
- **Response:** GraphQL Schema Definition Language

---

## 🛠️ **Testing Best Practices**

### **1. Start with Simple Queries**
- Test basic queries first (authors, books)
- Verify data is returned correctly
- Check response structure

### **2. Test DataLoader Performance**
- Run queries that fetch related data
- Check browser DevTools Network tab
- Should see single database queries, not N+1

### **3. Test Error Handling**
- Try invalid queries
- Test with non-existent IDs
- Verify proper error messages

### **4. Test Filtering and Sorting**
- Use various filter combinations
- Test different sorting orders
- Verify pagination works

### **5. Test Mutations**
- Create new records
- Update existing records
- Verify validation rules

### **6. Test Subscriptions**
- Open subscription in one tab
- Trigger mutations in another tab
- Verify real-time updates

---

## 🎯 **Quick Testing Checklist**

- [ ] Server starts without errors
- [ ] GraphQL IDE loads at `/graphql`
- [ ] Schema is properly displayed
- [ ] Basic queries return data
- [ ] DataLoaders prevent N+1 queries
- [ ] Filtering and sorting work
- [ ] Mutations create/update data
- [ ] Subscriptions receive real-time updates
- [ ] Error handling works properly
- [ ] Health check returns OK

---

## 📊 **Expected Response Format**

### **Successful Query Response**
```json
{
  "data": {
    "authors": [
      {
        "id": 1,
        "fullName": "John Doe",
        "email": "john.doe@example.com"
      }
    ]
  }
}
```

### **Error Response**
```json
{
  "errors": [
    {
      "message": "Cannot query field 'invalidField' on type 'Author'.",
      "locations": [
        {
          "line": 2,
          "column": 3
        }
      ]
    }
  ]
}
```

---

## 🚀 **Ready to Test!**

Your GraphQL API is now ready for comprehensive testing. Start with the **GraphQL IDE** for the best experience, then move to programmatic testing with your preferred tools!
