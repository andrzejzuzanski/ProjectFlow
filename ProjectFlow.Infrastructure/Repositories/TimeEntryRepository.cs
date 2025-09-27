using Microsoft.EntityFrameworkCore;
using ProjectFlow.Core.Entities;
using ProjectFlow.Core.Interfaces;
using ProjectFlow.Infrastructure.Data;

namespace ProjectFlow.Infrastructure.Repositories
{
    public class TimeEntryRepository : ITimeEntryRepository
    {
        private readonly ProjectFlowDbContext _context;

        public TimeEntryRepository(ProjectFlowDbContext context)
        {
            _context = context;
        }

        public async Task<TimeEntry> CreateAsync(TimeEntry timeEntry)
        {
            timeEntry.CreatedAt = DateTime.UtcNow;
            timeEntry.StartTime = DateTime.UtcNow;

            _context.TimeEntries.Add(timeEntry);
            await _context.SaveChangesAsync();

            return await GetByIdAsync(timeEntry.Id) ?? timeEntry;
        }

        public async Task<TimeEntry?> GetByIdAsync(int id)
        {
            return await _context.TimeEntries
                .Include(te => te.Task)
                .Include(te => te.User)
                .FirstOrDefaultAsync(te => te.Id == id);
        }

        public async Task<IEnumerable<TimeEntry>> GetByTaskIdAsync(int taskId)
        {
            return await _context.TimeEntries
                .Include(te => te.User)
                .Where(te => te.TaskId == taskId)
                .OrderByDescending(te => te.StartTime)
                .ToListAsync();
        }

        public async Task<IEnumerable<TimeEntry>> GetByUserIdAsync(int userId)
        {
            return await _context.TimeEntries
                .Include(te => te.Task)
                .Where(te => te.UserId == userId)
                .OrderByDescending(te => te.StartTime)
                .ToListAsync();
        }

        public async Task<TimeEntry?> GetActiveTimerByUserIdAsync(int userId)
        {
            return await _context.TimeEntries
                .Include(te => te.Task)
                .FirstOrDefaultAsync(te => te.UserId == userId && te.EndTime == null);
        }

        public async Task<TimeEntry> UpdateAsync(TimeEntry timeEntry)
        {
            _context.TimeEntries.Update(timeEntry);
            await _context.SaveChangesAsync();
            return timeEntry;
        }

        public async Task<bool> DeleteAsync(int id)
        {
            var timeEntry = await _context.TimeEntries.FindAsync(id);
            if (timeEntry == null) return false;

            _context.TimeEntries.Remove(timeEntry);
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<int> GetTotalTimeMinutesByTaskIdAsync(int taskId)
        {
            return await _context.TimeEntries
                .Where(te => te.TaskId == taskId && te.EndTime != null)
                .SumAsync(te => te.DurationMinutes);
        }
        public async Task<TimeEntry?> GetActiveTimerByTaskIdAsync(int taskId)
        {
            return await _context.TimeEntries
                .FirstOrDefaultAsync(te => te.TaskId == taskId && te.EndTime == null);
        }
    }
}