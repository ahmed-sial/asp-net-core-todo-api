using Microsoft.AspNetCore.Diagnostics;

namespace TodoApi.Exceptions
{
    /// <summary>
    /// Handles exceptions of type <see cref="ResourceNotFoundException"/> and returns a 404 Not Found HTTP response.
    /// </summary>
    public class ResourceNotFoundExceptionHandler : IExceptionHandler
    {
        /// <summary>
        /// Attempts to handle the given exception if it is of type <see cref="ResourceNotFoundException"/>.
        /// Returns a structured JSON response with HTTP status code 404.
        /// </summary>
        /// <param name="httpContext">The HTTP context for the current request.</param>
        /// <param name="exception">The exception that was thrown during the request execution.</param>
        /// <param name="cancellationToken">A token to observe for cancellation.</param>
        /// <returns>
        /// <c>true</c> if the exception was handled and a response was written; otherwise, <c>false</c>.
        /// </returns>
        public async ValueTask<bool> TryHandleAsync(HttpContext httpContext, Exception exception, CancellationToken cancellationToken)
        {
            // If the exception is not of type ResourceNotFoundException, do not handle it
            if (exception is not ResourceNotFoundException)
                return false;

            // Create a structured error response with 404 status code
            var res = new ApiErrorResponse
            {
                StatusCode = StatusCodes.Status404NotFound,
                Title = "Resource Not Found",
                ErrorMessage = exception.Message,
            };

            // Set the response status code and write the error as JSON
            httpContext.Response.StatusCode = res.StatusCode;
            await httpContext.Response.WriteAsJsonAsync(res, cancellationToken);

            return true;
        }
    }
    /// <summary>
    /// Represents an exception that is thrown when a requested resource is not found in the system.
    /// Commonly used to return a 404 Not Found error in API responses.
    /// </summary>
    public class ResourceNotFoundException: Exception
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ResourceNotFoundException"/> class.
        /// </summary>
        public ResourceNotFoundException() { }
        /// <summary>
        /// Initializes a new instance of the <see cref="ResourceNotFoundException"/> class
        /// with a specified error message.
        /// </summary>
        /// <param name="message">The message that describes the error.</param>
        public ResourceNotFoundException(string message)
            : base(message) { }
        /// <summary>
        /// Initializes a new instance of the <see cref="ResourceNotFoundException"/> class
        /// with a specified error message and a reference to the inner exception that is the cause of this exception.
        /// </summary>
        /// <param name="message">The message that describes the error.</param>
        /// <param name="innerException">The exception that is the cause of the current exception.</param>
        public ResourceNotFoundException(string message, Exception innerException)
            : base(message, innerException) { }
    }

}
