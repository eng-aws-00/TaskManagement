using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using TaskManagement.DbContexts;
using TaskManagement.Dtos.Tasks;
using TaskManagement.Models;

namespace TaskManagement.Controllers
{
    [Authorize]
    [Route("api/[controller]")]
    [ApiController]
    public class TasksController : ControllerBase
    {
        private readonly TaskManagementContext _dbContext;

        public TasksController(TaskManagementContext dbContext)
        {
            _dbContext = dbContext;
        }

        [HttpGet("GetByCriteria")]
        public IActionResult GetByCriteria([FromQuery] SearchTaskDto searchDto)
        {
            try
            {
                // Extract UserId from JWT
                var userId = long.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value);

                // Query tasks for current user only
                var query = from task in _dbContext.Tasks
                            from lookup in _dbContext.Lookups.Where(l => l.Id == task.StatusId).DefaultIfEmpty()
                            where task.UserId == userId
                               && (searchDto.Title == null || task.Title.ToLower().Contains(searchDto.Title.ToLower()))
                               && (searchDto.StatusId == null || task.StatusId == searchDto.StatusId)
                            orderby task.Id descending
                            select new TaskDto
                            {
                                Id = task.Id,
                                Title = task.Title,
                                Description = task.Description,
                                FromDate = task.FromDate,
                                ToDate = task.ToDate,
                                StatusId = task.StatusId,
                                StatusName = lookup.Name,
                                UserId = task.UserId
                            };

                var data = query.ToList();
                return Ok(data);
            }
            catch (Exception ex)
            {
                return StatusCode(500, ex.Message);
            }
        }

        [HttpGet("GetById/{id}")]
        public IActionResult GetById(long id)
        {
            try
            {
                // Extract UserId from JWT
                var userId = long.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value);

                // Get task only if it belongs to current user
                var data = (from task in _dbContext.Tasks
                            from lookup in _dbContext.Lookups.Where(l => l.Id == task.StatusId).DefaultIfEmpty()
                            where task.Id == id && task.UserId == userId
                            select new TaskDto
                            {
                                Id = task.Id,
                                Title = task.Title,
                                Description = task.Description,
                                FromDate = task.FromDate,
                                ToDate = task.ToDate,
                                StatusId = task.StatusId,
                                StatusName = lookup.Name,
                                UserId = task.UserId
                            }).FirstOrDefault();

                if (data == null)
                    return NotFound("Task not found.");

                return Ok(data);
            }
            catch (Exception ex)
            {
                return StatusCode(500, ex.Message);
            }
        }

        [HttpPost("Add")]
        public IActionResult Add([FromBody] SaveTaskDto dto)
        {
            try
            {
                // Validate date range
                if (dto.FromDate > dto.ToDate)
                {
                    return BadRequest("FromDate must be less than or equal to ToDate.");
                }

                // Extract UserId from JWT
                var userId = long.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value);

                // Create task
                var task = new TaskItem
                {
                    Title = dto.Title,
                    Description = dto.Description,
                    FromDate = dto.FromDate,
                    ToDate = dto.ToDate,
                    StatusId = dto.StatusId,
                    UserId = userId
                };

                _dbContext.Tasks.Add(task);
                _dbContext.SaveChanges();

                return Ok(task.Id);
            }
            catch (Exception ex)
            {
                return StatusCode(500, ex.Message);
            }
        }

        [HttpPut("Update")]
        public IActionResult Update([FromBody] SaveTaskDto dto)
        {
            try
            {
                if (dto.Id == null)
                {
                    return BadRequest("Task Id is required.");
                }

                // Validate date range
                if (dto.FromDate > dto.ToDate)
                {
                    return BadRequest("FromDate must be less than or equal to ToDate.");
                }

                // Extract UserId from JWT
                var userId = long.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value);

                // Find task only if it belongs to current user
                var task = _dbContext.Tasks
                    .FirstOrDefault(t => t.Id == dto.Id && t.UserId == userId);

                if (task == null)
                    return BadRequest("Task not found or access denied.");

                // Update task
                task.Title = dto.Title;
                task.Description = dto.Description;
                task.FromDate = dto.FromDate;
                task.ToDate = dto.ToDate;
                task.StatusId = dto.StatusId;

                _dbContext.SaveChanges();

                return Ok("Task updated successfully.");
            }
            catch (Exception ex)
            {
                return StatusCode(500, ex.Message);
            }
        }

        [HttpDelete("Delete/{id}")]
        public IActionResult Delete(long id)
        {
            try
            {
                // Extract UserId from JWT
                var userId = long.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value);

                // Find task only if it belongs to current user
                var task = _dbContext.Tasks
                    .FirstOrDefault(t => t.Id == id && t.UserId == userId);

                if (task == null)
                    return BadRequest("Task not found or access denied.");

                _dbContext.Tasks.Remove(task);
                _dbContext.SaveChanges();

                return Ok("Task deleted successfully.");
            }
            catch (Exception ex)
            {
                return StatusCode(500, ex.Message);
            }
        }
    }
}
