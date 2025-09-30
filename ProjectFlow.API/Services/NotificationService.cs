using Microsoft.AspNetCore.SignalR;
using ProjectFlow.API.Hubs;
using ProjectFlow.Core.Entities;
using ProjectFlow.Core.Interfaces;
using Serilog;

namespace ProjectFlow.API.Services
{
    public class NotificationService : INotificationService
    {
        private readonly IServiceProvider _serviceProvider;

        public NotificationService(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;
        }

        public async Task NotifyTaskCreated(ProjectTask task)
        {
            var hubContext = _serviceProvider.GetRequiredService<IHubContext<TaskUpdatesHub>>();
            var groupName = $"Project_{task.ProjectId}";

            await hubContext.Clients.Group(groupName).SendAsync("TaskCreated", new
            {
                TaskId = task.Id,
                Title = task.Title,
                Status = task.Status,
                Priority = task.Priority,
                ProjectId = task.ProjectId,
                AssignedToId = task.AssignedToId,
                CreatedAt = task.CreatedAt
            });

            Log.Information("Sent TaskCreated notification for task {TaskId} to project group {ProjectId}",
                task.Id, task.ProjectId);
        }

        public async Task NotifyTaskUpdated(ProjectTask task)
        {
            var hubContext = _serviceProvider.GetRequiredService<IHubContext<TaskUpdatesHub>>();
            var groupName = $"Project_{task.ProjectId}";

            await hubContext.Clients.Group(groupName).SendAsync("TaskUpdated", new
            {
                TaskId = task.Id,
                Title = task.Title,
                Status = task.Status,
                Priority = task.Priority,
                ProjectId = task.ProjectId,
                AssignedToId = task.AssignedToId,
                UpdatedAt = task.UpdatedAt
            });

            Log.Information("Sent TaskUpdated notification for task {TaskId} to project group {ProjectId}",
                task.Id, task.ProjectId);
        }

        public async Task NotifyTaskDeleted(int taskId, int projectId)
        {
            var hubContext = _serviceProvider.GetRequiredService<IHubContext<TaskUpdatesHub>>();
            var groupName = $"Project_{projectId}";

            await hubContext.Clients.Group(groupName).SendAsync("TaskDeleted", new
            {
                TaskId = taskId,
                ProjectId = projectId,
                DeletedAt = DateTime.UtcNow
            });

            Log.Information("Sent TaskDeleted notification for task {TaskId} to project group {ProjectId}",
                taskId, projectId);
        }

        public async Task NotifyProjectUpdated(Project project)
        {
            var hubContext = _serviceProvider.GetRequiredService<IHubContext<TaskUpdatesHub>>();
            var groupName = $"Project_{project.Id}";

            await hubContext.Clients.Group(groupName).SendAsync("ProjectUpdated", new
            {
                ProjectId = project.Id,
                Name = project.Name,
                Status = project.Status,
                UpdatedAt = DateTime.UtcNow
            });

            Log.Information("Sent ProjectUpdated notification for project {ProjectId}", project.Id);
        }
        public async Task NotifyTimerStarted(TimeEntry timeEntry)
        {
            var hubContext = _serviceProvider.GetRequiredService<IHubContext<TaskUpdatesHub>>();
            var groupName = $"Project_{timeEntry.Task.ProjectId}";

            await hubContext.Clients.Group(groupName).SendAsync("TimerStarted", new
            {
                TaskId = timeEntry.TaskId,
                UserId = timeEntry.UserId,
                UserName = $"{timeEntry.User.FirstName} {timeEntry.User.LastName}",
                StartTime = timeEntry.StartTime,
                TaskTitle = timeEntry.Task.Title
            });

            Log.Information("Sent TimerStarted notification for task {TaskId} by user {UserId}",
                timeEntry.TaskId, timeEntry.UserId);
        }

        public async Task NotifyTimerStopped(TimeEntry timeEntry)
        {
            var hubContext = _serviceProvider.GetRequiredService<IHubContext<TaskUpdatesHub>>();
            var groupName = $"Project_{timeEntry.Task.ProjectId}";

            await hubContext.Clients.Group(groupName).SendAsync("TimerStopped", new
            {
                TaskId = timeEntry.TaskId,
                UserId = timeEntry.UserId,
                UserName = $"{timeEntry.User.FirstName} {timeEntry.User.LastName}",
                DurationMinutes = timeEntry.DurationMinutes,
                TaskTitle = timeEntry.Task.Title
            });

            Log.Information("Sent TimerStopped notification for task {TaskId} by user {UserId}",
                timeEntry.TaskId, timeEntry.UserId);
        }

        public async Task NotifyAttachmentAdded(Attachment attachment)
        {
            var hubContext = _serviceProvider.GetRequiredService<IHubContext<TaskUpdatesHub>>();
            var groupName = $"Project_{attachment.Task.ProjectId}";

            await hubContext.Clients.Group(groupName).SendAsync("AttachmentAdded", new
            {
                AttachmentId = attachment.Id,
                FileName = attachment.FileName,
                TaskId = attachment.TaskId,
                UploadedById = attachment.UploadedById,
                CreatedAt = attachment.CreatedAt
            });

            Log.Information("Sent AttachmentAdded notification for attachment {AttachmentId} to project group",
                attachment.Id);
        }

        public async Task NotifyAttachmentDeleted(int attachmentId, int taskId)
        {
            var hubContext = _serviceProvider.GetRequiredService<IHubContext<TaskUpdatesHub>>();

            // Get task to find project
            var taskRepository = _serviceProvider.GetRequiredService<ITaskRepository>();
            var task = await taskRepository.GetByIdAsync(taskId);

            if (task != null)
            {
                var groupName = $"Project_{task.ProjectId}";

                await hubContext.Clients.Group(groupName).SendAsync("AttachmentDeleted", new
                {
                    AttachmentId = attachmentId,
                    TaskId = taskId
                });

                Log.Information("Sent AttachmentDeleted notification for attachment {AttachmentId}", attachmentId);
            }
        }
    }
}