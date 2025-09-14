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

            group.MapGet("/project/{projectId}", GetTasksByProject);
            group.MapGet("/user/{userId}", GetTasksByUser);
            group.MapGet("/status/{status}", GetTasksByStatus);
            group.MapGet("/priority/{priority}", GetTasksByPriority);
            group.MapGet("/{id}", GetTask);
            group.MapGet("/{id}/details", GetTaskWithDetails);
            group.MapPost("/", CreateTask);
            group.MapPut("/{id}", UpdateTask);
            group.MapDelete("/{id}", DeleteTask);
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
            IMapper mapper)
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
            var taskDto = mapper.Map<TaskDto>(createdTask);

            return Results.Created($"/api/tasks/{createdTask.Id}", taskDto);
        }

        private static async Task<IResult> UpdateTask(
            int id,
            UpdateTaskDto updateDto,
            ITaskRepository taskRepository,
            IUserRepository userRepository,
            IMapper mapper)
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
            var taskDto = mapper.Map<TaskDto>(updatedTask);

            return Results.Ok(taskDto);
        }

        private static async Task<IResult> DeleteTask(int id, ITaskRepository repository)
        {
            var success = await repository.DeleteAsync(id);

            if (!success)
                return Results.NotFound();

            return Results.NoContent();
        }
    }
}
