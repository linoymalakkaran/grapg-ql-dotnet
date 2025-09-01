using System.Security.Claims;
using GraphQLSimple.Extensions;
using HotChocolate.AspNetCore;
using HotChocolate.Execution;
using Microsoft.Extensions.Logging;
using System.Diagnostics;
using Microsoft.AspNetCore.Http;
using HotChocolate.AspNetCore.Serialization;

namespace GraphQLSimple.Extensions
{
    /// <summary>
    /// Custom GraphQL middleware for request/response processing
    /// This middleware handles authentication, logging, performance monitoring, and error handling
    /// </summary>
    public class GraphQLMiddleware
    {
        private readonly Microsoft.AspNetCore.Http.RequestDelegate _next;
        private readonly ILogger<GraphQLMiddleware> _logger;

        public GraphQLMiddleware(Microsoft.AspNetCore.Http.RequestDelegate next, ILogger<GraphQLMiddleware> logger)
        {
            _next = next;
            _logger = logger;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            // Only process GraphQL requests
            if (!context.Request.Path.StartsWithSegments("/graphql"))
            {
                await _next(context);
                return;
            }

            var stopwatch = Stopwatch.StartNew();
            var requestId = Guid.NewGuid().ToString("N")[..8];

            try
            {
                // Log incoming request
                _logger.LogInformation("GraphQL Request [{RequestId}] started from {RemoteIpAddress}", 
                    requestId, context.Connection.RemoteIpAddress);

                // Add request headers for monitoring
                context.Items["RequestId"] = requestId;
                context.Items["StartTime"] = DateTime.UtcNow;

                // Add user context if authenticated
                if (context.User.Identity?.IsAuthenticated == true)
                {
                    var userId = context.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                    var userEmail = context.User.FindFirst(ClaimTypes.Email)?.Value;
                    
                    context.Items["UserId"] = userId;
                    context.Items["UserEmail"] = userEmail;
                    
                    _logger.LogInformation("GraphQL Request [{RequestId}] authenticated user: {UserEmail}", 
                        requestId, userEmail);
                }

                // Set request timeout (optional)
                using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(30));
                var combinedToken = CancellationTokenSource.CreateLinkedTokenSource(
                    context.RequestAborted, cts.Token).Token;

                // Process the GraphQL request
                await _next(context);

                stopwatch.Stop();

                // Log successful completion
                _logger.LogInformation("GraphQL Request [{RequestId}] completed in {ElapsedMilliseconds}ms", 
                    requestId, stopwatch.ElapsedMilliseconds);
            }
            catch (OperationCanceledException) when (context.RequestAborted.IsCancellationRequested)
            {
                stopwatch.Stop();
                _logger.LogWarning("GraphQL Request [{RequestId}] was cancelled after {ElapsedMilliseconds}ms", 
                    requestId, stopwatch.ElapsedMilliseconds);
                
                context.Response.StatusCode = 499; // Client Closed Request
                await context.Response.WriteAsync("Request was cancelled");
            }
            catch (Exception ex)
            {
                stopwatch.Stop();
                _logger.LogError(ex, "GraphQL Request [{RequestId}] failed after {ElapsedMilliseconds}ms", 
                    requestId, stopwatch.ElapsedMilliseconds);

                // Don't rethrow - let GraphQL handle the error
                await _next(context);
            }
        }
    }

    /// <summary>
    /// GraphQL request/response interceptor for additional processing
    /// </summary>
    public class GraphQLRequestInterceptor : DefaultHttpRequestInterceptor
    {
        private readonly ILogger<GraphQLRequestInterceptor> _logger;

        public GraphQLRequestInterceptor(ILogger<GraphQLRequestInterceptor> logger)
        {
            _logger = logger;
        }

        // Simplified interceptor - just logs request info
        public override ValueTask OnCreateAsync(HttpContext context, IRequestExecutor requestExecutor, 
            OperationRequestBuilder requestBuilder, CancellationToken cancellationToken)
        {
            var requestId = context.Items["RequestId"]?.ToString() ?? "unknown";
            
            // Log request creation
            _logger.LogDebug("GraphQL Request [{RequestId}] created", requestId);

            return base.OnCreateAsync(context, requestExecutor, requestBuilder, cancellationToken);
        }
    }

    /// <summary>
    /// Simple GraphQL logging interceptor
    /// </summary>
    public class GraphQLLoggingInterceptor
    {
        private readonly ILogger<GraphQLLoggingInterceptor> _logger;

        public GraphQLLoggingInterceptor(ILogger<GraphQLLoggingInterceptor> logger)
        {
            _logger = logger;
        }

        public void LogRequest(string requestId, string operationType, TimeSpan duration, bool hasErrors)
        {
            if (hasErrors)
            {
                _logger.LogWarning("GraphQL {OperationType} [{RequestId}] completed with errors in {Duration}ms",
                    operationType, requestId, duration.TotalMilliseconds);
            }
            else
            {
                _logger.LogInformation("GraphQL {OperationType} [{RequestId}] executed successfully in {Duration}ms",
                    operationType, requestId, duration.TotalMilliseconds);
            }
        }

        public void LogError(string requestId, Exception exception)
        {
            _logger.LogError(exception, "GraphQL Request [{RequestId}] encountered an error", requestId);
        }
    }

    /// <summary>
    /// GraphQL rate limiting middleware
    /// </summary>
    public class GraphQLRateLimitingMiddleware
    {
        private readonly Microsoft.AspNetCore.Http.RequestDelegate _next;
        private readonly ILogger<GraphQLRateLimitingMiddleware> _logger;
        private static readonly Dictionary<string, (DateTime LastRequest, int RequestCount)> _clientRequests = new();
        private static readonly object _lock = new();
        
        // Rate limiting: 100 requests per minute per IP
        private const int MaxRequestsPerMinute = 100;
        private static readonly TimeSpan TimeWindow = TimeSpan.FromMinutes(1);

        public GraphQLRateLimitingMiddleware(Microsoft.AspNetCore.Http.RequestDelegate next, ILogger<GraphQLRateLimitingMiddleware> logger)
        {
            _next = next;
            _logger = logger;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            if (!context.Request.Path.StartsWithSegments("/graphql"))
            {
                await _next(context);
                return;
            }

            var clientIp = context.Connection.RemoteIpAddress?.ToString() ?? "unknown";
            var now = DateTime.UtcNow;

            lock (_lock)
            {
                if (_clientRequests.TryGetValue(clientIp, out var clientData))
                {
                    // Reset counter if time window has passed
                    if (now - clientData.LastRequest > TimeWindow)
                    {
                        _clientRequests[clientIp] = (now, 1);
                    }
                    else
                    {
                        // Check if rate limit exceeded
                        if (clientData.RequestCount >= MaxRequestsPerMinute)
                        {
                            _logger.LogWarning("Rate limit exceeded for IP: {ClientIp}", clientIp);
                            context.Response.StatusCode = 429; // Too Many Requests
                            context.Response.WriteAsync("Rate limit exceeded. Please try again later.");
                            return;
                        }

                        _clientRequests[clientIp] = (clientData.LastRequest, clientData.RequestCount + 1);
                    }
                }
                else
                {
                    _clientRequests[clientIp] = (now, 1);
                }
            }

            // Clean up old entries periodically
            if (_clientRequests.Count > 1000)
            {
                CleanupOldEntries(now);
            }

            await _next(context);
        }

        private void CleanupOldEntries(DateTime now)
        {
            var keysToRemove = _clientRequests
                .Where(kvp => now - kvp.Value.LastRequest > TimeWindow)
                .Select(kvp => kvp.Key)
                .ToList();

            foreach (var key in keysToRemove)
            {
                _clientRequests.Remove(key);
            }
        }
    }

    /// <summary>
    /// Extension methods for registering GraphQL middleware
    /// </summary>
    public static class GraphQLMiddlewareExtensions
    {
        public static IServiceCollection AddCustomGraphQLMiddleware(this IServiceCollection services)
        {
            services.AddSingleton<GraphQLRequestInterceptor>();
            services.AddSingleton<GraphQLLoggingInterceptor>();
            return services;
        }

        public static IApplicationBuilder UseCustomGraphQLMiddleware(this IApplicationBuilder app)
        {
            return app.UseMiddleware<GraphQLMiddleware>();
        }

        public static IApplicationBuilder UseGraphQLRateLimiting(this IApplicationBuilder app)
        {
            return app.UseMiddleware<GraphQLRateLimitingMiddleware>();
        }
    }
}