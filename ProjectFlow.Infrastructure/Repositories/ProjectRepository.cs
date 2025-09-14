using Microsoft.EntityFrameworkCore;
using ProjectFlow.Core.Entities;
using ProjectFlow.Core.Interfaces;
using ProjectFlow.Infrastructure.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ProjectFlow.Infrastructure.Repositories
{
    public class ProjectRepository : IProjectRepository
    {
        private readonly ProjectFlowDbContext _context;

        public ProjectRepository(ProjectFlowDbContext context)
        {
            _context = context;
        }

        public async Task<Project> CreateAsync(Project project)
        {
            _context.Projects.Add(project);
            await _context.SaveChangesAsync();

            await _context.Entry(project)
                .Reference(p => p.CreatedBy)
                .LoadAsync();

            return project;
        }

        public async Task<bool> DeleteAsync(int id)
        {
            var project = await _context.Projects.FindAsync(id);

            if(project == null) 
                return false;

            project.IsActive = false;
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<IEnumerable<Project>> GetAllActiveAsync()
        {
            return await _context.Projects
                .Where(p => p.IsActive)
                .Include(p => p.CreatedBy)
                .OrderByDescending(p => p.CreatedAt)
                .ToListAsync();
        }

        public async Task<IEnumerable<Project>> GetByCreatedByIdAsync(int createdById)
        {
            return await _context.Projects
                .Where(p => p.CreatedById == createdById && p.IsActive)
                .Include(p => p.CreatedBy)
                .OrderByDescending(p => p.CreatedAt)
                .ToListAsync();
        }

        public Task<Project?> GetByIdAsync(int id)
        {
            return _context.Projects
                .Include(p => p.CreatedBy)
                .FirstOrDefaultAsync(p => p.Id == id && p.IsActive);
        }

        public async Task<Project?> GetByIdWithTasksAsync(int id)
        {
            return await _context.Projects
                .Include(p => p. CreatedBy)
                .Include(p => p.Tasks.Where(t => t.Status != Core.Enums.ProjectTaskStatus.Done))
                .FirstOrDefaultAsync(p => p.Id == id && p.IsActive);
        }

        public async Task<Project> UpdateAsync(Project project)
        {
            _context.Projects.Update(project);
            await _context.SaveChangesAsync();
            return project;
        }
    }
}
