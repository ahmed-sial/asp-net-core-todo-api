using System.ComponentModel.DataAnnotations;
using TodoApi.Attributes;

namespace TodoApi.Models;
/// <summary>
/// Holds the information of a Todo Task.
/// </summary>
public partial class TodoTask
{
    /// <summary>
    /// Enum representing the priority of the TodoTask.
    /// </summary>
    public enum TaskPriority { Low, Medium, High, Critical }
    /// <summary>
    /// Enum representing the category of the TodoTask.
    /// </summary>
    public enum TaskCategory { Work, Personal, Family, Health, Hobbies, Chores, Shopping, Learning, Other }
    /// <summary>
    /// Unique identifier for the TodoTask. 
    /// </summary>
    [Key]
    public int Id { get; set; }
    /// <summary>
    /// Name of the TodoTask.
    /// </summary>
    [Required]
    [StringLength(100, MinimumLength = 2, ErrorMessage = "The name should be between 2 to 100 character long.")]
    public string Name { get; set; } = null!;
    /// <summary>
    /// Description of the TodoTask.
    /// </summary>
    [StringLength(255, MinimumLength = 0, ErrorMessage = "The description should be upto 255 character long.")]
    public string? Description { get; set; }
    /// <summary>
    /// Status of the TodoTask wheather completed or pending.
    /// </summary>
    public bool Status { get; set; }

    /// <summary>
    /// Date the task was created.
    /// </summary>
    [DisplayFormat(DataFormatString = "{yyyy-MM-dd}")]
    public DateOnly CreatedAt { get; set; }
    /// <summary>
    /// Date when the task was last updated.
    /// </summary>
    [DisplayFormat(DataFormatString = "{yyyy-MM-dd}")]
    public DateOnly UpdateAt { get; set; }

    /// <summary>
    /// Priority of the TodoTask (Low, Medium, High or Critical).
    /// </summary>
    [Required]
    public TaskPriority Priority { get; set; }
    /// <summary>
    /// Category of the TodoTask (Work, Personal, Family, Health, Hobbies, Chores, Shopping, Learning or Other).
    /// </summary>
    [Required]
    public TaskCategory Category { get; set; }
    /// <summary>
    /// Due Date of the TodoTask.
    /// </summary>
    [Required]
    [DisplayFormat(DataFormatString = "{yyyy-MM-dd}")]
    [FutureDate]
    public DateOnly DueDate { get; set; }
    /// <summary>
    /// Value indicating whether the task is overdue.
    /// </summary>
    public bool IsOverdue { get; set; }
}    
