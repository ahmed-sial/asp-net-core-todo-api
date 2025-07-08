using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

namespace TodoApi.Models;

/// <summary>
/// Database context for the Todo API.
/// </summary>
/// <remarks>Handles all the operations with database to create, retrive, update and delete records from database.</remarks>
public partial class TodoApiContext : DbContext
{
    /// <summary>
    /// Default contructor that initializes new instance of <see cref="TodoApiContext"/> class.
    /// </summary>
    public TodoApiContext()
    {
    }

    /// <summary>
    /// Contructor that initializes new instance of <see cref="TodoApiContext"/> class.
    /// </summary>
    /// <param name="options"></param>
    public TodoApiContext(DbContextOptions<TodoApiContext> options)
        : base(options)
    {
    }

    // DbSet holding the databse table as variable
    public virtual DbSet<TodoTask> TodoTasks { get; set; }
    /// <summary>
    /// Configures the schema needed for the application's data model.
    /// This method is called by the Entity Framework runtime when the model for a derived context is being created.
    /// </summary>
    /// <param name="modelBuilder">
    /// Provides a simple API surface for configuring the model and its mappings to the database.
    /// </param>
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        var priorityConverter = new EnumToStringConverter<TodoTask.TaskPriority>();
        var categoryConverter = new EnumToStringConverter<TodoTask.TaskCategory>();

        modelBuilder.Entity<TodoTask>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__TodoTask__3213E83F241DAE54");

            entity.ToTable("TodoTask");

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.Category)
                .HasMaxLength(20)
                .IsUnicode(false)
                .HasColumnName("category")
                .HasConversion(
                    v => v.ToString(),
                    v => (TodoTask.TaskCategory)Enum.Parse(typeof(TodoTask.TaskCategory), v)
                );
            entity.Property(e => e.CreatedAt).HasColumnName("created_at");
            entity.Property(e => e.Description)
                .HasMaxLength(255)
                .IsUnicode(false)
                .HasDefaultValue("Nil")
                .HasColumnName("description");
            entity.Property(e => e.DueDate).HasColumnName("due_date");
            entity.Property(e => e.IsOverdue).HasColumnName("is_overdue");
            entity.Property(e => e.Name)
                .HasMaxLength(100)
                .IsUnicode(false)
                .HasColumnName("name");
            entity.Property(e => e.Priority)
                .HasMaxLength(8)
                .IsUnicode(false)
                .HasColumnName("priority")
                .HasConversion(
                    v => v.ToString(), // Convert enum to string when saving
                    v => (TodoTask.TaskPriority)Enum.Parse(typeof(TodoTask.TaskPriority), v) // Convert string to enum when reading
                );
            entity.Property(e => e.Status).HasColumnName("status");
            entity.Property(e => e.UpdateAt).HasColumnName("update_at");
        });

        OnModelCreatingPartial(modelBuilder);
    }
    /// <summary>
    /// Updates the <c>IsOverdue</c> flag for all <see cref="TodoTask"/> entities that are being added or modified,
    /// based on whether their <c>DueDate</c> is in the past and their <c>Status</c> is not completed.
    /// </summary>
    private void UpdateOverDueTasks()
    {
        // Get all tracked TodoTask entries that are either being added or modified
        var entries = ChangeTracker.Entries<TodoTask>()
            .Where(e => e.State == EntityState.Added || e.State == EntityState.Modified);

        foreach (var entry in entries)
        {
            var task = entry.Entity;

            // Mark the task as overdue if its due date is in the past and it is not completed
            task.IsOverdue = task.DueDate < DateOnly.FromDateTime(DateTime.Now) && !task.Status;
        }
    }

    /// <summary>
    /// Overrides the default <see cref="DbContext.SaveChanges()"/> method to ensure
    /// that overdue flags are updated before saving data to the database.
    /// </summary>
    /// <returns>The number of state entries written to the database.</returns>
    public override int SaveChanges()
    {
        // Automatically update overdue status before persisting changes
        UpdateOverDueTasks();
        return base.SaveChanges();
    }

    /// <summary>
    /// Overrides the default asynchronous <see cref="DbContext.SaveChangesAsync(CancellationToken)"/> method
    /// to ensure that overdue flags are updated before saving data to the database asynchronously.
    /// </summary>
    /// <param name="cancellationToken">A cancellation token to cancel the operation gracefully if requested.</param>
    /// <returns>A task that represents the asynchronous save operation. The task result contains
    /// the number of state entries written to the database.</returns>
    public override Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        // Automatically update overdue status before persisting changes
        UpdateOverDueTasks();
        return base.SaveChangesAsync(cancellationToken);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);


}
