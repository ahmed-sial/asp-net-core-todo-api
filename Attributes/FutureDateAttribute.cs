using System.ComponentModel.DataAnnotations;

namespace TodoApi.Attributes
{
    // Custom class for validating if a date is in the future.
    public class FutureDateAttribute : ValidationAttribute
    {
        /// <summary>
        /// Constructor to set the default error message. 
        /// </summary>
        public FutureDateAttribute() 
        {
            ErrorMessage = "The date must be in future.";
        }
        /// <summary>
        /// Method to validate the date. 
        /// </summary>
        /// <param name="value">The date value passed from request.</param>
        /// <returns><c>true</c>if date is in future; otherwise, <c>false</c>.</returns>
        public override bool IsValid(object? value)
        {
            if (value is DateOnly dt)
            {
                return dt >= DateOnly.FromDateTime(DateTime.Now);
            }
            return false;
        }
    }
}
