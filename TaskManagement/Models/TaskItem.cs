using System.ComponentModel.DataAnnotations.Schema;

namespace TaskManagement.Models
{
    public class TaskItem
    {
        public long Id { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }
        public DateTime FromDate { get; set; }
        public DateTime ToDate { get; set; }
        public long StatusId { get; set; }
        public long UserId { get; set; }

        [ForeignKey("StatusId")]
        public Lookup Status { get; set; }

        [ForeignKey("UserId")]
        public User User { get; set; }
    }
}
