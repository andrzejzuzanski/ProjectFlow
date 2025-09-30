using ProjectFlow.Core.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ProjectFlow.Core.Interfaces
{
    public interface INotificationService
    {
        Task NotifyTaskCreated(ProjectTask task);
        Task NotifyTaskUpdated(ProjectTask task);
        Task NotifyTaskDeleted(int taskId, int projectId);
        Task NotifyProjectUpdated(Project project);
        Task NotifyTimerStarted(TimeEntry timeEntry);
        Task NotifyTimerStopped(TimeEntry timeEntry);
        Task NotifyAttachmentAdded(Attachment attachment);
        Task NotifyAttachmentDeleted(int attachmentId, int taskId);
    }
}
