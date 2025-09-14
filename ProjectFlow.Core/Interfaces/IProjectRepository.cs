using ProjectFlow.Core.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ProjectFlow.Core.Interfaces
{
    public interface IProjectRepository
    {
        Task<IEnumerable<Project>> GetAllActiveAsync();
        Task<Project?> GetByIdAsync(int id);
        Task<Project?> GetByIdWithTasksAsync(int id);
        Task<IEnumerable<Project>> GetByCreatedByIdAsync(int createdById);
        Task<Project> CreateAsync(Project project);
        Task<Project> UpdateAsync(Project project);
        Task<bool> DeleteAsync(int id);
    }
}
