using HotChocolate;
using HotChocolate.Resolvers;
using Microsoft.Extensions.Logging;
using System.Diagnostics;

namespace GraphQLSimple.Extensions
{
    /// <summary>
    /// Middleware for logging GraphQL requests and responses
    /// </summary>
    public class GraphQLLoggingMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<GraphQLLoggingMiddleware> _logger;

        public GraphQLLoggingMiddleware(RequestDelegate next, ILogger<GraphQLLoggingMiddleware> logger)
        {
            _next = next;
            _logger = logger;
        }

        public async Task InvokeAsync(IMiddlewareContext context)
        {
            var stopwatch = Stopwatch.StartNew();
            var operationName = context.Document.Definitions
                .OfType<HotChocolate.Language.OperationDefinitionNode>()
                .FirstOrDefault()?.Name?.Value ?? "Unknown";

            _logger.LogInformation("GraphQL operation started: {OperationName}", operationName);

            try
            {
                await _next(context);
                stopwatch.Stop();

                _logger.LogInformation(
                    "GraphQL operation completed: {OperationName} in {ElapsedMilliseconds}ms",
                    operationName,
                    stopwatch.ElapsedMilliseconds);
            }
            catch (Exception ex)
            {
                stopwatch.Stop();
                _logger.LogError(ex,
                    "GraphQL operation failed: {OperationName} in {ElapsedMilliseconds}ms - {ErrorMessage}",
                    operationName,
                    stopwatch.ElapsedMilliseconds,
                    ex.Message);
                throw;
            }
        }
    }

    /// <summary>
    /// Middleware for performance monitoring
    /// </summary>
    public class GraphQLPerformanceMiddleware
    {
        private readonly FieldDelegate _next;
        private readonly ILogger<GraphQLPerformanceMiddleware> _logger;

        public GraphQLPerformanceMiddleware(FieldDelegate next, ILogger<GraphQLPerformanceMiddleware> logger)
        {
            _next = next;
            _logger = logger;
        }

        public async Task InvokeAsync(IMiddlewareContext context)
        {
            var stopwatch = Stopwatch.StartNew();
            var fieldName = context.Selection.Field.Name;
            var parentType = context.Selection.Field.DeclaringType.Name;

            try
            {
                await _next(context);
                stopwatch.Stop();

                // Log slow fields (> 100ms)
                if (stopwatch.ElapsedMilliseconds > 100)
                {
                    _logger.LogWarning(
                        "Slow GraphQL field resolved: {ParentType}.{FieldName} took {ElapsedMilliseconds}ms",
                        parentType,
                        fieldName,
                        stopwatch.ElapsedMilliseconds);
                }
                else
                {
                    _logger.LogDebug(
                        "GraphQL field resolved: {ParentType}.{FieldName} in {ElapsedMilliseconds}ms",
                        parentType,
                        fieldName,
                        stopwatch.ElapsedMilliseconds);
                }
            }
            catch (Exception ex)
            {
                stopwatch.Stop();
                _logger.LogError(ex,
                    "GraphQL field resolution failed: {ParentType}.{FieldName} in {ElapsedMilliseconds}ms - {ErrorMessage}",
                    parentType,
                    fieldName,
                    stopwatch.ElapsedMilliseconds,
                    ex.Message);
                throw;
            }
        }
    }

    /// <summary>
    /// Middleware for authentication and authorization
    /// </summary>
    public class GraphQLAuthMiddleware
    {
        private readonly FieldDelegate _next;
        private readonly ILogger<GraphQLAuthMiddleware> _logger;

        public GraphQLAuthMiddleware(FieldDelegate next, ILogger<GraphQLAuthMiddleware> logger)
        {
            _next = next;
            _logger = logger;
        }

        public async Task InvokeAsync(IMiddlewareContext context)
        {
            // Check for authorization requirements
            var requiresAuth = context.Selection.Field.ContextData.ContainsKey("RequiresAuth");
            var requiresRole = context.Selection.Field.ContextData.TryGetValue("RequiresRole", out var roleValue);

            if (requiresAuth || requiresRole)
            {
                // In a real application, you would check JWT token or session
                // For demo purposes, we'll just log the requirement
                _logger.LogInformation(
                    "Field {FieldName} requires authentication{Role}",
                    context.Selection.Field.Name,
                    requiresRole ? $" and role: {roleValue}" : "");

                // Here you would implement actual authorization logic
                // For now, we'll just continue to the next middleware
            }

            await _next(context);
        }
    }

    /// <summary>
    /// Middleware for rate limiting
    /// </summary>
    public class GraphQLRateLimitMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<GraphQLRateLimitMiddleware> _logger;
        private static readonly Dictionary<string, List<DateTime>> _requestTracker = new();
        private static readonly object _lock = new();
        private const int MaxRequestsPerMinute = 60;

        public GraphQLRateLimitMiddleware(RequestDelegate next, ILogger<GraphQLRateLimitMiddleware> logger)
        {
            _next = next;
            _logger = logger;
        }

        public async Task InvokeAsync(IMiddlewareContext context)
        {
            var clientId = GetClientId(context);
            var now = DateTime.UtcNow;

            lock (_lock)
            {
                if (!_requestTracker.ContainsKey(clientId))
                {
                    _requestTracker[clientId] = new List<DateTime>();
                }

                var requests = _requestTracker[clientId];
                
                // Remove requests older than 1 minute
                requests.RemoveAll(r => (now - r).TotalMinutes > 1);

                if (requests.Count >= MaxRequestsPerMinute)
                {
                    _logger.LogWarning("Rate limit exceeded for client: {ClientId}", clientId);
                    throw new GraphQLException("Rate limit exceeded. Please try again later.");
                }

                requests.Add(now);
            }

            await _next(context);
        }

        private string GetClientId(IMiddlewareContext context)
        {
            // In a real application, you would use actual client identification
            // like IP address, user ID, API key, etc.
            return context.Services.GetService<IHttpContextAccessor>()?.HttpContext?.Connection?.RemoteIpAddress?.ToString() ?? "unknown";
        }
    }

    /// <summary>
    /// Middleware for caching
    /// </summary>
    public class GraphQLCacheMiddleware
    {
        private readonly FieldDelegate _next;
        private readonly ILogger<GraphQLCacheMiddleware> _logger;
        private static readonly Dictionary<string, (object Result, DateTime Expiry)> _cache = new();
        private static readonly object _lock = new();

        public GraphQLCacheMiddleware(FieldDelegate next, ILogger<GraphQLCacheMiddleware> logger)
        {
            _next = next;
            _logger = logger;
        }

        public async Task InvokeAsync(IMiddlewareContext context)
        {
            // Check if field should be cached
            var shouldCache = context.Selection.Field.ContextData.TryGetValue("CacheFor", out var cacheTimeValue);
            
            if (!shouldCache || cacheTimeValue is not TimeSpan cacheTime)
            {
                await _next(context);
                return;
            }

            var cacheKey = GenerateCacheKey(context);
            var now = DateTime.UtcNow;

            lock (_lock)
            {
                // Check if cached result exists and is still valid
                if (_cache.TryGetValue(cacheKey, out var cached) && cached.Expiry > now)
                {
                    _logger.LogDebug("Cache hit for key: {CacheKey}", cacheKey);
                    context.Result = cached.Result;
                    return;
                }
            }

            // Execute the field resolver
            await _next(context);

            // Cache the result
            if (context.Result != null)
            {
                lock (_lock)
                {
                    _cache[cacheKey] = (context.Result, now.Add(cacheTime));
                    _logger.LogDebug("Cached result for key: {CacheKey}", cacheKey);
                }
            }
        }

        private string GenerateCacheKey(IMiddlewareContext context)
        {
            var fieldName = context.Selection.Field.Name;
            var parentType = context.Selection.Field.DeclaringType.Name;
            var arguments = string.Join(",", context.ArgumentLiterals.Select(a => $"{a.Key}={a.Value}"));
            
            return $"{parentType}.{fieldName}({arguments})";
        }
    }
}
