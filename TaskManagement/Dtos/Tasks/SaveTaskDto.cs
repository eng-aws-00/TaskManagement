using System.ComponentModel.DataAnnotations;
using TaskManagement.Helpers;

namespace TaskManagement.Dtos.Tasks
{
    public class SaveTaskDto
    {
        public long? Id { get; set; }

        [Required]
        public string Title { get; set; }

        public string Description { get; set; }

        [DateRange]
        public DateTime FromDate { get; set; }

        public DateTime ToDate { get; set; }

        public long StatusId { get; set; }
    }
}
