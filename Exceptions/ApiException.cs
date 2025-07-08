using Microsoft.AspNetCore.Diagnostics;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;

namespace TodoApi.Exceptions
{
    /// <summary>
    /// Generic exception handler for API errors.
    /// </summary>
    /// <remarks>This class implements the IExceptionHandler interface to handle various common exceptions that may occur.</remarks>
    public class ApiException : IExceptionHandler
    {
        /// <summary>
        /// Handles exceptions that occur during API requests by transforming them into structured HTTP responses.
        /// </summary>
        /// <param name="httpContext">The HTTP context for the current request, containing request and response details.</param>
        /// <param name="exception">The exception that was thrown during the processing of the request.</param>
        /// <param name="cancellationToken">A token that can be used to cancel the operation if requested.</param>
        /// <returns>A ValueTask indicating whether the exception was handled.</returns>
        public async ValueTask<bool> TryHandleAsync(HttpContext httpContext, Exception exception, CancellationToken cancellationToken)
        {
            ApiErrorResponse? res = null;
            switch (exception)
            {
                case NotImplementedException:
                    res = new ApiErrorResponse
                    {
                        StatusCode = StatusCodes.Status501NotImplemented,
                        Title = "Not Implemented",
                        ErrorMessage = exception.Message
                    };
                    break;

                case DbUpdateConcurrencyException:
                    res = new ApiErrorResponse
                    {
                        StatusCode = StatusCodes.Status409Conflict,
                        Title = "Database Concurrency Error",
                        ErrorMessage = exception.Message
                    };
                    break;

                case DbUpdateException:
                    res = new ApiErrorResponse
                    {
                        StatusCode = StatusCodes.Status500InternalServerError,
                        Title = "Database Update Error",
                        ErrorMessage = exception.Message
                    };
                    break;

                case SqlException:
                    res = new ApiErrorResponse
                    {
                        StatusCode = StatusCodes.Status500InternalServerError,
                        Title = "SQL Server Error",
                        ErrorMessage = exception.Message
                    };
                    break;

                case ArgumentNullException:
                    res = new ApiErrorResponse
                    {
                        StatusCode = StatusCodes.Status400BadRequest,
                        Title = "Null Argument Error",
                        ErrorMessage = exception.Message
                    };
                    break;

                case ArgumentException:
                    res = new ApiErrorResponse
                    {
                        StatusCode = StatusCodes.Status400BadRequest,
                        Title = "Invalid Argument Error",
                        ErrorMessage = exception.Message
                    };
                    break;
                case NullReferenceException:
                    res = new ApiErrorResponse
                    {
                        StatusCode = StatusCodes.Status400BadRequest,
                        Title = "Null Reference Error",
                        ErrorMessage = exception.Message
                    };
                    break;
            }
            if (res is not null)
            {
                // Write the error response to the HTTP context.
                httpContext.Response.StatusCode = res.StatusCode;
                // Set the content type to application/json.
                await httpContext.Response.WriteAsJsonAsync(res, cancellationToken);
                // Indicate that the exception was handled and no further processing is needed.
                return true;
            }
            // If no specific handling was defined for the exception, return false to allow other handlers to process it.
            return false;
        }
    }
}
