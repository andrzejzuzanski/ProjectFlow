using ProjectFlow.Core.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ProjectFlow.Core.Interfaces
{
    public interface ITimeEntryRepository
    {
        Task<TimeEntry> CreateAsync(TimeEntry timeEntry);
        Task<TimeEntry?> GetByIdAsync(int id);
        Task<IEnumerable<TimeEntry>> GetByTaskIdAsync(int taskId);
        Task<IEnumerable<TimeEntry>> GetByUserIdAsync(int userId);
        Task<TimeEntry?> GetActiveTimerByUserIdAsync(int userId);
        Task<TimeEntry> UpdateAsync(TimeEntry timeEntry);
        Task<bool> DeleteAsync(int id);
        Task<int> GetTotalTimeMinutesByTaskIdAsync(int taskId);
        Task<TimeEntry?> GetActiveTimerByTaskIdAsync(int taskId);
    }
}
