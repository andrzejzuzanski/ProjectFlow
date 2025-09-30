using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ProjectFlow.Core.Entities;

namespace ProjectFlow.Core.Interfaces
{
    public interface IAttachmentRepository
    {
        Task<Attachment> CreateAsync(Attachment attachment);
        Task<Attachment?> GetByIdAsync(int id);
        Task<IEnumerable<Attachment>> GetByTaskIdAsync(int taskId);
        Task DeleteAsync(int id);
        Task<bool> ExistsAsync(int id);
    }
}
