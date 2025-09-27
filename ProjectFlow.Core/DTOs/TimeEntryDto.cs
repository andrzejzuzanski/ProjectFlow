using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ProjectFlow.Core.DTOs
{
    public class TimeEntryDto
    {
        public int Id { get; set; }
        public int TaskId { get; set; }
        public int UserId { get; set; }
        public string UserName { get; set; } = string.Empty;
        public DateTime StartTime { get; set; }
        public DateTime? EndTime { get; set; }
        public int DurationMinutes { get; set; }
        public string Description { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }
    }

    public class CreateTimeEntryDto
    {
        public int TaskId { get; set; }
        public string Description { get; set; } = string.Empty;
    }

    public class UpdateTimeEntryDto
    {
        public string Description { get; set; } = string.Empty;
    }
}
