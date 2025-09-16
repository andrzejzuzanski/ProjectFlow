using AutoMapper;
using FluentValidation;
using ProjectFlow.Core.DTOs;
using ProjectFlow.Core.Entities;
using ProjectFlow.Core.Enums;
using ProjectFlow.Core.Interfaces;

namespace ProjectFlow.API.Endpoints
{
    public static class TaskEndpoints
    {
        public static void MapTaskEndpoints(this IEndpointRouteBuilder app)
        {
            var group = app.MapGroup("/api/tasks").WithTags("Tasks");

            group.MapGet("/project/{projectId}", GetTasksByProject).RequireAuthorization();
            group.MapGet("/user/{userId}", GetTasksByUser).RequireAuthorization();
            group.MapGet("/status/{status}", GetTasksByStatus).RequireAuthorization();
            group.MapGet("/priority/{priority}", GetTasksByPriority).RequireAuthorization();
            group.MapGet("/{id}", GetTask).RequireAuthorization();
            group.MapGet("/{id}/details", GetTaskWithDetails).RequireAuthorization();
            group.MapPost("/", CreateTask).RequireAuthorization("Developer");
            group.MapPut("/{id}", UpdateTask).RequireAuthorization("Developer");
            group.MapDelete("/{id}", DeleteTask).RequireAuthorization("ProjectManager");
        }
        private static async Task<IResult> GetTasksByProject(int projectId, ITaskRepository repository, IMapper mapper)
        {
            var tasks = await repository.GetAllByProjectIdAsync(projectId);
            var taskDtos = mapper.Map<IEnumerable<TaskDto>>(tasks);
            return Results.Ok(taskDtos);
        }

        private static async Task<IResult> GetTasksByUser(int userId, ITaskRepository repository, IMapper mapper)
        {
            var tasks = await repository.GetByAssignedUserIdAsync(userId);
            var taskDtos = mapper.Map<IEnumerable<TaskDto>>(tasks);
            return Results.Ok(taskDtos);
        }

        private static async Task<IResult> GetTasksByStatus(int status, ITaskRepository repository, IMapper mapper)
        {
            if (!Enum.IsDefined(typeof(ProjectTaskStatus), status))
                return Results.BadRequest("Invalid status value");

            var taskStatus = (ProjectTaskStatus)status;
            var tasks = await repository.GetByStatusAsync(taskStatus);
            var taskDtos = mapper.Map<IEnumerable<TaskDto>>(tasks);
            return Results.Ok(taskDtos);
        }

        private static async Task<IResult> GetTasksByPriority(int priority, ITaskRepository repository, IMapper mapper)
        {
            if (!Enum.IsDefined(typeof(TaskPriority), priority))
                return Results.BadRequest("Invalid priority value");

            var taskPriority = (TaskPriority)priority;
            var tasks = await repository.GetTasksByPriorityAsync(taskPriority);
            var taskDtos = mapper.Map<IEnumerable<TaskDto>>(tasks);
            return Results.Ok(taskDtos);
        }

        private static async Task<IResult> GetTask(int id, ITaskRepository repository, IMapper mapper)
        {
            var task = await repository.GetByIdAsync(id);

            if (task == null)
                return Results.NotFound();

            var taskDto = mapper.Map<TaskDto>(task);
            return Results.Ok(taskDto);
        }

        private static async Task<IResult> GetTaskWithDetails(int id, ITaskRepository repository, IMapper mapper)
        {
            var task = await repository.GetByIdWithDetailsAsync(id);

            if (task == null)
                return Results.NotFound();

            var taskDto = mapper.Map<TaskWithDetailsDto>(task);
            return Results.Ok(taskDto);
        }

        private static async Task<IResult> CreateTask(
            CreateTaskDto createDto,
            ITaskRepository taskRepository,
            IProjectRepository projectRepository,
            IUserRepository userRepository,
            IValidator<CreateTaskDto> validator,
            IMapper mapper,
            INotificationService notificationService)
        {
            var validationResult = await validator.ValidateAsync(createDto);
            if (!validationResult.IsValid)
            {
                return Results.BadRequest(validationResult.Errors.Select(e => new
                {
                    Property = e.PropertyName,
                    Error = e.ErrorMessage
                }));
            }

            var project = await projectRepository.GetByIdAsync(createDto.ProjectId);
            if (project == null)
                return Results.BadRequest("Project not found");

            if (createDto.AssignedToId.HasValue)
            {
                var assignee = await userRepository.GetByIdAsync(createDto.AssignedToId.Value);
                if (assignee == null)
                    return Results.BadRequest("Assignee user not found");
            }

            var task = mapper.Map<ProjectTask>(createDto);
            var createdTask = await taskRepository.CreateAsync(task);

            await notificationService.NotifyTaskCreated(createdTask);

            var taskDto = mapper.Map<TaskDto>(createdTask);

            return Results.Created($"/api/tasks/{createdTask.Id}", taskDto);
        }

        private static async Task<IResult> UpdateTask(
            int id,
            UpdateTaskDto updateDto,
            ITaskRepository taskRepository,
            IUserRepository userRepository,
            IMapper mapper,
            INotificationService notificationService)
        {
            var existingTask = await taskRepository.GetByIdAsync(id);
            if (existingTask == null)
                return Results.NotFound();

            if (updateDto.AssignedToId.HasValue)
            {
                var assignee = await userRepository.GetByIdAsync(updateDto.AssignedToId.Value);
                if (assignee == null)
                    return Results.BadRequest("Assignee user not found");
            }

            mapper.Map(updateDto, existingTask);

            var updatedTask = await taskRepository.UpdateAsync(existingTask);

            await notificationService.NotifyTaskUpdated(updatedTask);

            var taskDto = mapper.Map<TaskDto>(updatedTask);

            return Results.Ok(taskDto);
        }

        private static async Task<IResult> DeleteTask(int id, ITaskRepository repository, INotificationService notificationService)
        {
            var taskToDelete = await repository.GetByIdAsync(id);
            if (taskToDelete == null)
                return Results.NotFound();

            var success = await repository.DeleteAsync(id);

            if (!success)
                return Results.NotFound();

            await notificationService.NotifyTaskDeleted(id, taskToDelete.ProjectId);

            return Results.NoContent();
        }
    }
}
