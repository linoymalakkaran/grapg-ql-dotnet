using HotChocolate;
using Serilog;

namespace GraphQLSimple.Extensions
{
    public class GraphQLErrorFilter : IErrorFilter
    {
        private readonly Serilog.ILogger _logger = Log.ForContext<GraphQLErrorFilter>();

        public IError OnError(IError error)
        {
            // Log the error
            _logger.Error(error.Exception, "GraphQL Error: {Message}", error.Message);

            // Return error with or without exception details based on environment
            return error.Exception switch
            {
                ValidationException validationEx => error
                    .WithMessage(validationEx.Message),
                UnauthorizedAccessException => error
                    .WithMessage("Unauthorized access"),
                ArgumentException argEx => error
                    .WithMessage(argEx.Message),
                _ => error
            };
        }
    }

    public class ValidationException : Exception
    {
        public ValidationException(string message) : base(message)
        {
        }

        public ValidationException(string message, Exception innerException) : base(message, innerException)
        {
        }
    }
}
