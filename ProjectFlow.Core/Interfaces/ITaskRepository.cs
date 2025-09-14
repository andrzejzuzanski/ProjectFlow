using ProjectFlow.Core.Entities;
using ProjectFlow.Core.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ProjectFlow.Core.Interfaces
{
    public interface ITaskRepository
    {
        Task<IEnumerable<ProjectTask>> GetAllByProjectIdAsync(int projectId);
        Task<IEnumerable<ProjectTask>> GetByAssignedUserIdAsync(int userId);
        Task<IEnumerable<ProjectTask>> GetByStatusAsync(ProjectTaskStatus status);
        Task<ProjectTask?> GetByIdAsync(int id);
        Task<ProjectTask?> GetByIdWithDetailsAsync(int id);
        Task<ProjectTask> CreateAsync(ProjectTask task);
        Task<ProjectTask> UpdateAsync(ProjectTask task);
        Task<bool> DeleteAsync(int id);
        Task<IEnumerable<ProjectTask>> GetTasksByPriorityAsync(TaskPriority priority);
    }
}
