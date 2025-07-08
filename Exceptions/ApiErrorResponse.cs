namespace TodoApi.Exceptions
{
    /// <summary>
    /// Represents an error response returned by an API.
    /// </summary>
    /// <remarks>This class encapsulates details about an error that occurred during an API request, 
    /// including the HTTP status code, a short title describing the error, and a detailed error message. It is
    /// typically used to provide structured error information to API clients.</remarks>
    public class ApiErrorResponse
    {
        /// <summary>
        /// Gets or sets the HTTP status code associated with the response.
        /// </summary>
        public int StatusCode { get; set; }
        /// <summary>
        /// Gets or sets the title of the item.
        /// </summary>
        public string Title { get; set; } = string.Empty;
        /// <summary>
        /// Gets or sets the error message providing details about the error.
        /// </summary>
        public string ErrorMessage { get; set; } = string.Empty;
    }
}
