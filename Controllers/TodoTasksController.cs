using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using TodoApi.Exceptions;
using TodoApi.Models;

namespace TodoApi.Controllers
{
    /// <summary>
    /// Provides API endpoints for managing todo tasks.
    /// </summary>
    /// <remarks>This controller includes methods for retrieving, creating, updating, and deleting todo tasks.
    /// It leverages caching to optimize performance for frequently accessed data and logs operations for
    /// monitoring.</remarks>
    [Route("api/[controller]")]
    [ApiController]
    public class TodoTasksController : ControllerBase
    {
        // Provides Database Context, Logger and Memory Cache for TodoTasksController
        private readonly TodoApiContext _context;
        private readonly ILogger<TodoTasksController> _logger;
        private readonly IMemoryCache _cache;
        private readonly string _cacheKey = "TodoTasks"; // Cache key for storing todo tasks

        /// <summary>
        /// Initializes a new instance of the <see cref="TodoTasksController"/> class with the required dependencies.
        /// </summary>
        /// <param name="context">The database context used to interact with the Todo API data store.</param>
        /// <param name="logger">The logger instance used for logging information, warnings, and errors.</param>
        /// <param name="memoryCache">The in-memory cache service used for caching frequently accessed data.</param>
        public TodoTasksController(TodoApiContext context, ILogger<TodoTasksController> logger, IMemoryCache memoryCache)
        {
            _context = context;
            _logger = logger;
            _cache = memoryCache;
        }
        /// <summary>
        /// Retrieve all todo tasks from the database.
        /// </summary>
        /// <returns>NoContent if the tasks are not found. Ok with list of todo tasks if tasks are found.</returns>
        [HttpGet]
        [ResponseCache(Duration = 60, Location = ResponseCacheLocation.Any, NoStore = false)]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<ActionResult<List<TodoTask>>> GetTask()
        {
            // Check if the tasks are already cached
            if (!_cache.TryGetValue(_cacheKey, out List<TodoTask>? cachedTasks))
            {
                _logger.LogInformation("Cache Miss. Fetching from database...");
                // If not cached, retrieve from the database
                cachedTasks = await _context.TodoTasks
                    .AsNoTracking()
                    .ToListAsync();
                if (cachedTasks.Count == 0)
                {
                    _logger.LogTrace("No todo tasks found in the database.");
                    return NoContent();
                }
                else
                {
                    // Cache the retrieved tasks for future requests
                    _logger.LogInformation($"Returning {cachedTasks.Count} todo tasks from the database.");
                    // Set cache options with sliding expiration of 60 seconds
                    var cacheEntryOptions = new MemoryCacheEntryOptions()
                        .SetSlidingExpiration(TimeSpan.FromSeconds(60));
                    // Store the tasks in the cache
                    _cache.Set(_cacheKey, cachedTasks, cacheEntryOptions);
                }
            }
            else
            {
                // If cached, return the cached tasks
                _logger.LogInformation("Cache Hit. Returning cached todo tasks.");
            }
            return Ok(cachedTasks);
        }
        /// <summary>
        /// Retrieve a todo task by its ID.
        /// </summary>
        /// <param name="id">ID of todo task.</param>
        /// <returns>The todo task with specific ID.</returns>
        /// <exception cref="ArgumentNullException">Thrown when the provided ID is null.</exception>
        /// <exception cref="ArgumentException">Thrown if either ID is less than zero, greater than max value or ID is not an integer.</exception>
        /// <exception cref="ResourceNotFoundException">Thrown when the todo task is with specified ID is not found.</exception>
        [HttpGet("{id}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<TodoTask>> GetTaskById(int? id)
        {
            if (id is null)
            {
                _logger.LogError($"Todo task with id {id} not found.");
                throw new ArgumentNullException("Id cannot be null");
            }
            else if (id <= 0)
            {
                _logger.LogError($"Todo task with id {id} not found.");
                throw new ArgumentException("Id must be greater than zero");
            } else if (id > int.MaxValue)
            {
                _logger.LogError($"Todo task with id {id} not found.");
                throw new ArgumentException("Id must be less than or equal to " + int.MaxValue);
            } else if (id is not int)
            {
                _logger.LogError($"Todo task with id {id} not found.");
                throw new ArgumentException("Id must be an integer");
            }
            // Retrive the todo task that matches the passed ID
                var todoTask = await _context.TodoTasks
                        .AsNoTracking()
                        .FirstOrDefaultAsync(t => t.Id == id);
            if (todoTask is null)
            {
                _logger.LogError($"Todo task with id {id} not found.");
                throw new ResourceNotFoundException($"Todo task with id {id} does not exist.");
            }
            _logger.LogInformation($"Returning todo task with id {id}.");
            return Ok(todoTask);
        }

        /// <summary>
        /// Create a new todo task.
        /// </summary>
        /// <param name="todoTask">The task object to create.</param>
        /// <returns>The newly created task.</returns>
        /// <exception cref="ArgumentNullException">Thrown when the task object is null.</exception>
        /// <exception cref="BusinessRuleViolationException">Thrown when the task object is not valid.</exception>
        [HttpPost]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status422UnprocessableEntity)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<TodoTask>> PostTask([FromBody] TodoTask? todoTask)
        {
            if (todoTask is null)
            {
                _logger.LogError("Todo task cannot be null.");
                throw new ArgumentNullException(nameof(todoTask), "Todo task cannot be null.");
            } else if (!ModelState.IsValid)
            {
                _logger.LogError("Invalid model state for the todo task.");
                throw new BusinessRuleViolationException("The todo task entered is not valid.");
            }

                try
                {
                    // Update CreatedAt UpdateAt and Status properties manually
                    todoTask.CreatedAt = DateOnly.FromDateTime(DateTime.Now);
                    todoTask.UpdateAt = DateOnly.FromDateTime(DateTime.Now);
                    todoTask.Status = false;
                    await _context.TodoTasks.AddAsync(todoTask);
                    await _context.SaveChangesAsync();
                    // Invalidate cache when adding new task
                    _cache.Remove(_cacheKey); 
                    _logger.LogInformation($"Todo task with id {todoTask.Id} created successfully.");
                // Return the created task with a 201 Created response, Location Header pointing to the new resource and URL to retrieve it
                return CreatedAtAction(nameof(GetTaskById), new { id = todoTask.Id }, todoTask);
                }
                catch (DbUpdateConcurrencyException ex)
                {
                    _logger.LogError(ex, "A concurrency error occurred while creating the todo task.");
                    throw;
                }
                catch (SqlException ex)
                {
                    _logger.LogError(ex, "A SQL error occurred while creating the todo task.");
                    throw;
                }
                catch (DbUpdateException ex)
                {
                    _logger.LogError(ex, "A database update error occurred while creating the todo task.");
                    throw;
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "An unexpected error occurred while creating the todo task.");
                    throw;
                }
        }
        /// <summary>
        /// Retrieve a todo task by its ID and update it. 
        /// </summary>
        /// <param name="id">ID of todo task to update.</param>
        /// <param name="todoTask">Todo task to update.</param>
        /// <returns>Updated todo task.</returns>
        /// <exception cref="ArgumentNullException">Thrown when either ID is null, todo task is null or ID and todo task's ID mismatch.</exception>
        /// <exception cref="BusinessRuleViolationException">Thrown if the format of todo task is invalid.</exception>
        [HttpPut("{id}")]
        public async Task<ActionResult<TodoTask>> PutTask(int? id, [FromBody] TodoTask? todoTask)
        {
            if (id is null || todoTask is null)
            {
                _logger.LogError("Todo task or ID is null.");
                throw new ArgumentNullException(nameof(todoTask), "Todo task or ID cannot be null.");
            }
            else if (id != todoTask.Id)
            {
                _logger.LogError($"Todo task ID mismatch: provided ID {id} does not match task ID {todoTask.Id}.");
                throw new ArgumentException(nameof(todoTask), "Todo task ID does not match the provided ID.");
            }
            else if (id <= 0)
            {
                _logger.LogError($"Todo task with id {id} not found.");
                throw new ArgumentException("Id must be greater than zero");
            } else if (id > int.MaxValue)
            {
                _logger.LogError($"Todo task with id {id} not found.");
                throw new ArgumentException("Id must be less than or equal to " + int.MaxValue);
            } else if (id is not int)
            {
                _logger.LogError($"Todo task with id {id} not found.");
                throw new ArgumentException("Id must be an integer");
            }
            else if (!ModelState.IsValid)
            {
                _logger.LogError("Invalid model state for the todo task.");
                throw new BusinessRuleViolationException("The todo task entered is not valid.");
            }
            try
            {
                // Retrieve the existing task from the database
                var existingTask = await _context.TodoTasks.FindAsync(id);
                if (existingTask is null)
                {
                    _logger.LogError($"Todo task with id {id} not found.");
                    throw new ResourceNotFoundException($"Todo task with id {id} does not exist.");
                }
                // Update the existing task with the new values except ID, CreatedAt
                existingTask.Name = todoTask.Name;
                existingTask.Description = todoTask.Description;
                existingTask.Status = todoTask.Status;
                existingTask.Priority = todoTask.Priority;
                existingTask.Category = todoTask.Category;
                existingTask.DueDate = todoTask.DueDate;
                existingTask.UpdateAt = DateOnly.FromDateTime(DateTime.Now);
                await _context.SaveChangesAsync();
                // Invalidate cache after updating the task
                _cache.Remove(_cacheKey); 
                _logger.LogInformation($"Todo task with id {todoTask.Id} updated successfully.");
                return Ok(existingTask);
            }
            catch (DbUpdateConcurrencyException ex)
            {
                _logger.LogError(ex, "A concurrency error occurred while updating the todo task.");
                throw;
            }
            catch (SqlException ex)
            {
                _logger.LogError(ex, "A SQL error occurred while updating the todo task.");
                throw;
            }
            catch (DbUpdateException ex)
            {
                _logger.LogError(ex, "A database update error occurred while updating the todo task.");
                throw;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An unexpected error occurred while updating the todo task.");
                throw;
            }
        }

        /// <summary>
        /// Delete a todo task by its ID.
        /// </summary>
        /// <param name="id">ID of todo task to delete.</param>
        /// <returns>Todo task removed.</returns>
        /// <exception cref="ArgumentNullException">Thrown if ID is null.</exception>
        /// <exception cref="ArgumentException">Thrown if either ID is less than zero, greater than max value or ID is not an integer.</exception>
        /// <exception cref="ResourceNotFoundException">Thrown if todo task is not found againt provided ID.</exception>
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteTask(int? id)
        {
            if (id is null)
            {
                _logger.LogError("Todo task ID cannot be null.");
                throw new ArgumentNullException(nameof(id), "Todo task ID cannot be null.");
            }
            else if (id <= 0)
            {
                _logger.LogError($"Todo task with id {id} not found.");
                throw new ArgumentException("Id must be greater than zero");
            } else if (id > int.MaxValue)
            {
                _logger.LogError($"Todo task with id {id} not found.");
                throw new ArgumentException("Id must be less than or equal to " + int.MaxValue);
            } else if (id is not int)
            {
                _logger.LogError($"Todo task with id {id} not found.");
                throw new ArgumentException("Id must be an integer");
            }
            var todoTask = await _context.TodoTasks.FindAsync(id);
            if (todoTask is null)
            {
                _logger.LogError($"Todo task with id {id} not found.");
                throw new ResourceNotFoundException($"Todo task with id {id} does not exist.");
            }
            try
            {
                _context.TodoTasks.Remove(todoTask);
                await _context.SaveChangesAsync();
                // Invalidate cache
                _cache.Remove(_cacheKey);
                _logger.LogInformation($"Todo task with id {id} deleted successfully.");
                return Ok(todoTask);
            }
            catch (DbUpdateConcurrencyException ex)
            {
                _logger.LogError(ex, "A concurrency error occurred while deleting the todo task.");
                throw;
            }
            catch (SqlException ex)
            {
                _logger.LogError(ex, "A SQL error occurred while deleting the todo task.");
                throw;
            }
            catch (DbUpdateException ex)
            {
                _logger.LogError(ex, "A database update error occurred while deleting the todo task.");
                throw;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An unexpected error occurred while updating the todo task.");
                throw;
            }
        }

    }
}
