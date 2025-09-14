using ProjectFlow.Core.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ProjectFlow.Core.Entities
{
    public class ProjectTask
    {
        public int Id { get; set; }
        public string Title { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public ProjectTaskStatus Status { get; set; } = ProjectTaskStatus.ToDo;
        public TaskPriority Priority { get; set; } = TaskPriority.Medium;
        public int ProjectId { get; set; }
        public int? AssignedToId { get; set; }
        public DateTime? DueDate { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }

        // Navigation properties
        public Project Project { get; set; } = null!;
        public User? AssignedTo { get; set; }
    }
}
