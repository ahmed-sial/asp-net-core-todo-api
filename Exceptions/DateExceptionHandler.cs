using Microsoft.AspNetCore.Diagnostics;

namespace TodoApi.Exceptions
{
    /// <summary>
    /// Handles exceptions of type <see cref="DateException"/> and generates a structured 400 Bad Request response.
    /// </summary>
    public class DateExceptionHandler : IExceptionHandler
    {
        /// <summary>
        /// Attempts to handle the exception and return a custom response if the exception is of type <see cref="DateException"/>.
        /// </summary>
        /// <param name="httpContext">The current HTTP context of the request.</param>
        /// <param name="exception">The exception that was thrown during request execution.</param>
        /// <param name="cancellationToken">A token to observe while waiting for the task to complete.</param>
        /// <returns>
        /// <c>true</c> if the exception was handled; otherwise, <c>false</c>.
        /// </returns>
        public async ValueTask<bool> TryHandleAsync(HttpContext httpContext, Exception exception, CancellationToken cancellationToken)
        {
            // If the exception is not of type DateException, skip handling
            if (exception is not DateException)
                return false;

            // Create a structured error response
            var res = new ApiErrorResponse
            {
                StatusCode = StatusCodes.Status400BadRequest,
                Title = "Invalid Date Format",
                ErrorMessage = exception.Message,
            };

            // Set the response status code and write the error as JSON
            httpContext.Response.StatusCode = res.StatusCode;
            await httpContext.Response.WriteAsJsonAsync(res, cancellationToken);

            return true;
        }
    }
    /// <summary>
    /// Represents an exception that is thrown when an invalid or improperly formatted date is encountered in the API.
    /// </summary>
    public class DateException : Exception
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="DateException"/> class.
        /// </summary>
        public DateException() { }
        /// <summary>
        /// Initializes a new instance of the <see cref="DateException"/> class with a specified error message.
        /// </summary>
        /// <param name="message">The message that describes the error.</param>
        public DateException(string message)
            : base(message) { }
        /// <summary>
        /// Initializes a new instance of the <see cref="DateException"/> class with a specified 
        /// error message and a reference to the inner exception that is the cause of this exception.
        /// </summary>
        /// <param name="message">The message that describes the error.</param>
        /// <param name="innerException">The exception that is the cause of the current exception.</param>
        public DateException(string message, Exception innerException)
            : base(message, innerException) { }
    }

}
