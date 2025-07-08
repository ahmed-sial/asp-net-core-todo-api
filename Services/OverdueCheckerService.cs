using Microsoft.EntityFrameworkCore;
using TodoApi.Models;

namespace TodoApi.Services
{
    /// <summary>
    /// A background service that periodically checks for overdue tasks in the database and updates their status.
    /// </summary>
    /// <remarks>This service runs continuously while the application is active, checking for tasks that are
    /// overdue and marking them as such. It performs the check at regular intervals, ensuring that overdue tasks are
    /// updated promptly. The service uses dependency injection to access the application's database context and
    /// operates within a scoped lifetime to ensure proper resource management.</remarks>

    public class OverdueCheckerService : BackgroundService
    {
        // Used to resolve scoped services inside the background service.
        private readonly IServiceProvider _serviceProvider;

        /// <summary>
        /// Constructor to initialize the background service with dependency injection.
        /// </summary>
        /// <param name="serviceProvider">Provides scoped services like DbContext.</param>

        public OverdueCheckerService(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;
        }

        /// <summary>
        /// The main execution loop for the background service.
        /// Runs continuously and marks tasks as overdue if their due date has passed.
        /// </summary>
        /// <param name="stoppingToken">Token used to gracefully stop the background task.</param>

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            // Run the loop until the host signals cancellation (e.g., during shutdown).
            while (!stoppingToken.IsCancellationRequested)
            {
                // Create a new scope for scoped services like DbContext.
                using var scope = _serviceProvider.CreateScope();
                var context = scope.ServiceProvider.GetRequiredService<TodoApiContext>();
                
                // Query all tasks that:
                // - Are not completed (Status = false)
                // - Have a due date before today
                // - Are not already marked as overdue
                var overdueTasks = await context.TodoTasks
                    .Where(t => !t.Status && t.DueDate < DateOnly.FromDateTime(DateTime.Now) && !t.IsOverdue)
                    .ToListAsync(stoppingToken);

                // Update each matching task to mark it as overdue
                foreach (var task in overdueTasks)
                {
                    task.IsOverdue = true;
                }

                // Persist the changes to the database
                await context.SaveChangesAsync(stoppingToken);

                // Wait before checking again (e.g., every 2 minutes)
                await Task.Delay(TimeSpan.FromMinutes(2), stoppingToken);
            }
        }
    }
}
