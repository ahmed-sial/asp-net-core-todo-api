using Microsoft.AspNetCore.Diagnostics;

namespace TodoApi.Exceptions
{
    /// <summary>
    /// Exception handler for business rule violations.
    /// </summary>
    /// <remarks>This class implements the IExceptionHandler interface to handle exceptions related to business rule violations such as invalid data format.</remarks>
    public class BusinessRuleViolationExceptionHandler : IExceptionHandler
    {
        /// <summary>
        /// Handles business rule violations exceptions that occur during API requests by transforming them into structured HTTP responses.
        /// </summary>
        /// <param name="httpContext">The HTTP context for the current request, containing request and response details.</param>
        /// <param name="exception">The exception that was thrown during the processing of the request.</param>
        /// <param name="cancellationToken">A token that can be used to cancel the operation if requested.</param>
        public async ValueTask<bool> TryHandleAsync(HttpContext httpContext, Exception exception, CancellationToken cancellationToken)
        {
            // Check if the exception is a BusinessRuleViolationException
            if (exception is not BusinessRuleViolationException)
                // if not, return false to let other handlers process it
                return false;

            // Create a structured error response for the business rule violation
            var res = new ApiErrorResponse
            {
                StatusCode = StatusCodes.Status422UnprocessableEntity,
                Title = "Business Rule Violation Error",
                ErrorMessage = exception.Message,
            };
            // Set the response status code and write the error response as JSON
            httpContext.Response.StatusCode = res.StatusCode;
            await httpContext.Response.WriteAsJsonAsync(res, cancellationToken);
            // Indicate that the exception has been handled
            return true;
        }
    }
    /// <summary>
    /// Represents errors that occur when a business rule is violated in the application.
    /// Typically used for domain-specific logic errors that do not fall under system or validation exceptions.
    /// </summary>
    /// <remarks>This exception is thrown when an operation violates a business rule, such as attempting to create a task with an invalid due date or status.</remarks>
    public class BusinessRuleViolationException : Exception
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="BusinessRuleViolationException"/> class.
        /// </summary>
        public BusinessRuleViolationException() { }
        /// <summary>
        /// Initializes a new instance of the <see cref="BusinessRuleViolationException"/> class
        /// with a specified error message.
        /// </summary>
        /// <param name="message">The message that describes the error.</param>
        public BusinessRuleViolationException(string message)
            : base(message) { }
        /// <summary>
        /// Initializes a new instance of the <see cref="BusinessRuleViolationException"/> class
        /// with a specified error message and a reference to the inner exception that is the cause of this exception.
        /// </summary>
        /// <param name="message">The message that describes the error.</param>
        /// <param name="innerException">The exception that is the cause of the current exception.</param>
        public BusinessRuleViolationException(string message, Exception innerException)
            : base(message, innerException) { }
    }
}
