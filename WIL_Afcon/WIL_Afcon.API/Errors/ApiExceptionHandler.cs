using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using MongoDB.Driver;

namespace WIL_Afcon.API.Errors
{
    // Last line of defence for exceptions the controllers do not handle.
    // Every failure goes back as an RFC 7807 problem+json body, so callers
    // always get the same error shape and never see stack traces or
    // database details.
    public class ApiExceptionHandler : IExceptionHandler
    {
        private readonly IProblemDetailsService _problemDetails;
        private readonly ILogger<ApiExceptionHandler> _logger;

        public ApiExceptionHandler(
            IProblemDetailsService problemDetails,
            ILogger<ApiExceptionHandler> logger)
        {
            _problemDetails = problemDetails;
            _logger = logger;
        }

        public async ValueTask<bool> TryHandleAsync(
            HttpContext httpContext,
            Exception exception,
            CancellationToken cancellationToken)
        {
            var problem = exception switch
            {
                // A unique index (device user ID, email, employee number...)
                // rejected the write: the record clashes with an existing one.
                MongoWriteException { WriteError.Category: ServerErrorCategory.DuplicateKey } => new ProblemDetails
                {
                    Status = StatusCodes.Status409Conflict,
                    Title = "Duplicate record",
                    Detail = "A record with the same unique value (for example device user ID, email or employee number) already exists."
                },

                BadHttpRequestException badRequest => new ProblemDetails
                {
                    Status = badRequest.StatusCode,
                    Title = "Bad request",
                    Detail = "The request could not be read."
                },

                _ => new ProblemDetails
                {
                    Status = StatusCodes.Status500InternalServerError,
                    Title = "Server error",
                    Detail = "An unexpected error occurred. Please try again later."
                }
            };

            if (problem.Status >= StatusCodes.Status500InternalServerError)
            {
                _logger.LogError(exception,
                    "Unhandled exception on {Method} {Path}",
                    httpContext.Request.Method, httpContext.Request.Path);
            }
            else
            {
                _logger.LogWarning(
                    "Request to {Method} {Path} failed with {Status}: {Message}",
                    httpContext.Request.Method, httpContext.Request.Path,
                    problem.Status, exception.Message);
            }

            httpContext.Response.StatusCode = problem.Status!.Value;

            return await _problemDetails.TryWriteAsync(new ProblemDetailsContext
            {
                HttpContext = httpContext,
                ProblemDetails = problem,
                Exception = exception
            });
        }
    }
}
