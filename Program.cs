using Microsoft.EntityFrameworkCore;
using GraphQLSimple.Data;
using GraphQLSimple.GraphQL;
using HotChocolate.AspNetCore;

var builder = WebApplication.CreateBuilder(args);

// Add EF Core
builder.Services.AddDbContext<LibraryContext>(options =>
    options.UseSqlite("Data Source=library.db"));

// Add GraphQL
builder.Services
    .AddGraphQLServer()
    .AddQueryType<Query>()
    .AddMutationType<Mutation>()
    .AddProjections()
    .AddFiltering()
    .AddSorting();

var app = builder.Build();

// Ensure database is created
using (var scope = app.Services.CreateScope())
{
    var context = scope.ServiceProvider.GetRequiredService<LibraryContext>();
    context.Database.EnsureCreated();
}

app.MapGraphQL();

// Enable GraphQL IDE in development
if (app.Environment.IsDevelopment())
{
    app.MapGraphQL("/graphql").WithOptions(new GraphQLServerOptions
    {
        Tool = { Enable = true }
    });
}

app.Run();
