using Microsoft.EntityFrameworkCore;
using TaskManagement.DbContexts;
using TaskManagement.Dtos.Tasks;
using TaskManagement.Models;

namespace TaskManagement.Services
{
    public interface ITaskService
    {
        List<TaskDto> GetByCriteria(long userId, SearchTaskDto searchDto);
        TaskDto GetById(long userId, long id);
        long Add(long userId, SaveTaskDto dto);
        void Update(long userId, SaveTaskDto dto);
        void Delete(long userId, long id);
    }

    public class TaskService : ITaskService
    {
        private readonly TaskManagementContext _dbContext;

        public TaskService(TaskManagementContext dbContext)
        {
            _dbContext = dbContext;
        }

        public List<TaskDto> GetByCriteria(long userId, SearchTaskDto searchDto)
        {
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

            return query.ToList();
        }

        public TaskDto GetById(long userId, long id)
        {
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
                throw new KeyNotFoundException("Task not found.");

            return data;
        }

        public long Add(long userId, SaveTaskDto dto)
        {
            if (dto.FromDate > dto.ToDate)
            {
                throw new ArgumentException("FromDate must be less than or equal to ToDate.");
            }

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

            return task.Id;
        }

        public void Update(long userId, SaveTaskDto dto)
        {
            if (dto.Id == null)
            {
                throw new ArgumentException("Task Id is required.");
            }

            if (dto.FromDate > dto.ToDate)
            {
                throw new ArgumentException("FromDate must be less than or equal to ToDate.");
            }

            var task = _dbContext.Tasks
                .FirstOrDefault(t => t.Id == dto.Id && t.UserId == userId);

            if (task == null)
                throw new UnauthorizedAccessException("Task not found or access denied.");

            task.Title = dto.Title;
            task.Description = dto.Description;
            task.FromDate = dto.FromDate;
            task.ToDate = dto.ToDate;
            task.StatusId = dto.StatusId;

            _dbContext.SaveChanges();
        }

        public void Delete(long userId, long id)
        {
            var task = _dbContext.Tasks
                .FirstOrDefault(t => t.Id == id && t.UserId == userId);

            if (task == null)
                throw new UnauthorizedAccessException("Task not found or access denied.");

            _dbContext.Tasks.Remove(task);
            _dbContext.SaveChanges();
        }
    }
}
