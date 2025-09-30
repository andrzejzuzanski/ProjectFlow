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
    public class AttachmentRepository : IAttachmentRepository
    {
        private readonly ProjectFlowDbContext _context;

        public AttachmentRepository(ProjectFlowDbContext context)
        {
            _context = context;
        }

        public async Task<Attachment> CreateAsync(Attachment attachment)
        {
            attachment.CreatedAt = DateTime.UtcNow;
            _context.Attachments.Add(attachment);
            await _context.SaveChangesAsync();
            return attachment;
        }

        public async Task DeleteAsync(int id)
        {
            var attachment = await _context.Attachments.FindAsync(id);
            if (attachment != null)
            {
                _context.Attachments.Remove(attachment);
                await _context.SaveChangesAsync();
            }
        }

        public async Task<bool> ExistsAsync(int id)
        {
            return await _context.Attachments.AnyAsync(a => a.Id == id);

        }

        public async Task<Attachment?> GetByIdAsync(int id)
        {
            return await _context.Attachments
                .Include(a => a.Task)
                .Include(a => a.UploadedBy)
                .FirstOrDefaultAsync(a => a.Id == id);
        }

        public async Task<IEnumerable<Attachment>> GetByTaskIdAsync(int taskId)
        {
            return await _context.Attachments
                .Include(a => a.UploadedBy)
                .Where(a => a.TaskId == taskId)
                .OrderByDescending(a => a.CreatedAt)
                .ToListAsync();
        }
    }
}
