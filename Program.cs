using Microsoft.EntityFrameworkCore;
using GraphQLSimple.Data;
using GraphQLSimple.GraphQL;
using GraphQLSimple.Services;
using GraphQLSimple.Extensions;
using GraphQLSimple.GraphQL.Types;
using GraphQLSimple.GraphQL.DataLoaders;
using HotChocolate.AspNetCore;
using HotChocolate.Data;
using FluentValidation;
using Serilog;
using System.Reflection;
using HotChocolate.Subscriptions;

// Configure Serilog
Log.Logger = new LoggerConfiguration()
    .WriteTo.Console()
    .CreateLogger();

var builder = WebApplication.CreateBuilder(args);

// Add Serilog
builder.Host.UseSerilog();

// Add Entity Framework with connection pooling
builder.Services.AddPooledDbContextFactory<LibraryContext>(options =>
    options.UseSqlite(builder.Configuration.GetConnectionString("DefaultConnection") ?? "Data Source=library.db")
           .EnableSensitiveDataLogging(builder.Environment.IsDevelopment())
           .EnableDetailedErrors(builder.Environment.IsDevelopment()));

// Add regular DbContext for services that need it
builder.Services.AddDbContext<LibraryContext>(options =>
    options.UseSqlite(builder.Configuration.GetConnectionString("DefaultConnection") ?? "Data Source=library.db")
           .EnableSensitiveDataLogging(builder.Environment.IsDevelopment())
           .EnableDetailedErrors(builder.Environment.IsDevelopment()));

// Add AutoMapper
builder.Services.AddAutoMapper(config => {
    config.AddProfile<MappingProfile>();
});

// Add FluentValidation
builder.Services.AddValidatorsFromAssembly(Assembly.GetExecutingAssembly());

// Add HTTP Context Accessor for middleware
builder.Services.AddHttpContextAccessor();

// Add Services
builder.Services.AddScoped<BookService>();
builder.Services.AddScoped<AuthorService>();
builder.Services.AddScoped<BorrowingService>();

// Add GraphQL Server with all advanced features
builder.Services
    .AddGraphQLServer()
    .AddQueryType<Query>()
    .AddMutationType<Mutation>()
    .AddSubscriptionType<Subscription>()
    .AddType<ISBNType>()
    .AddType<EmailType>()
    .AddType<PhoneType>()
    .AddType<URLType>()
    .AddType<PositiveIntType>()
    .AddProjections()
    .AddFiltering()
    .AddSorting()
    .AddDataLoader<AuthorByIdDataLoader>()
    .AddDataLoader<BooksByAuthorIdDataLoader>()
    .AddDataLoader<CategoryByIdDataLoader>()
    .AddDataLoader<ReviewsByBookIdDataLoader>()
    .AddDataLoader<UserByIdDataLoader>()
    .AddDataLoader<TagsByBookIdDataLoader>()
    .AddDataLoader<BorrowingsByBookIdDataLoader>()
    .AddDataLoader<BorrowingsByUserIdDataLoader>()
    .AddDataLoader<ReviewsByUserIdDataLoader>()
    .AddInMemorySubscriptions()
    .AddErrorFilter<GraphQLErrorFilter>()
    .ModifyRequestOptions(opt => opt.IncludeExceptionDetails = builder.Environment.IsDevelopment());

// Add CORS
builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(policy =>
    {
        policy.AllowAnyOrigin()
              .AllowAnyHeader()
              .AllowAnyMethod();
    });
});

// Add health checks
builder.Services.AddHealthChecks()
    .AddDbContextCheck<LibraryContext>();

var app = builder.Build();

// Use CORS
app.UseCors();

// Use Serilog request logging
app.UseSerilogRequestLogging();

// Configure GraphQL endpoint
app.MapGraphQL("/graphql")
   .WithOptions(new GraphQLServerOptions
   {
       Tool = { Enable = builder.Environment.IsDevelopment() }
   });

// Add WebSocket support for subscriptions
app.UseWebSockets();

// Add health check endpoint
app.MapHealthChecks("/health");

// Ensure database is created and seeded
using (var scope = app.Services.CreateScope())
{
    var context = scope.ServiceProvider.GetRequiredService<LibraryContext>();
    
    if (await context.Database.EnsureCreatedAsync())
    {
        Log.Information("Database created and seeded successfully");
    }
    else
    {
        Log.Information("Database already exists");
    }
}

// In development, show GraphQL schema SDL
if (app.Environment.IsDevelopment())
{
    app.MapGet("/schema", async (HotChocolate.Execution.IRequestExecutorResolver executorResolver) =>
    {
        var executor = await executorResolver.GetRequestExecutorAsync();
        return executor.Schema.Print();
    });
}

Log.Information("🚀 GraphQL Library Management System started!");
Log.Information("📚 GraphQL Endpoint: /graphql");
Log.Information("🎮 GraphQL IDE: /graphql (Development only)");
Log.Information("📊 Health Check: /health");
Log.Information("📋 Schema SDL: /schema (Development only)");

app.Run();
