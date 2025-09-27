using ProjectFlow.Core.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ProjectFlow.Core.DTOs
{
    public class TaskDto
    {
        public int Id { get; set; }
        public string Title { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public ProjectTaskStatus Status { get; set; }
        public TaskPriority Priority { get; set; }
        public int ProjectId { get; set; }
        public string ProjectName { get; set; } = string.Empty;
        public int? AssignedToId { get; set; }
        public string? AssignedToName { get; set; }
        public DateTime? DueDate { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
        public int TotalTimeMinutes { get; set; }
        public bool HasActiveTimer { get; set; }
        public DateTime? ActiveTimerStart { get; set; }
    }
    public class CreateTaskDto
    {
        public string Title { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public TaskPriority Priority { get; set; } = TaskPriority.Medium;
        public int ProjectId { get; set; }
        public int? AssignedToId { get; set; }
        public DateTime? DueDate { get; set; }
    }

    public class UpdateTaskDto
    {
        public string Title { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public ProjectTaskStatus Status { get; set; }
        public TaskPriority Priority { get; set; }
        public int? AssignedToId { get; set; }
        public DateTime? DueDate { get; set; }
    }

    public class TaskWithDetailsDto : TaskDto
    {
        public string ProjectDescription { get; set; } = string.Empty;
        public string ProjectCreatedByName { get; set; } = string.Empty;
        public IEnumerable<TimeEntryDto> TimeEntries { get; set; } = new List<TimeEntryDto>();
    }
}
