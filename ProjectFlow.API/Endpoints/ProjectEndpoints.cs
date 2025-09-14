using AutoMapper;
using FluentValidation;
using ProjectFlow.Core.DTOs;
using ProjectFlow.Core.Entities;
using ProjectFlow.Core.Interfaces;

namespace ProjectFlow.API.Endpoints
{
    public static class ProjectEndpoints
    {
        public static void MapProjectEndpoints(this IEndpointRouteBuilder app)
        {
            var group = app.MapGroup("api/projects").WithTags("Projects");

            group.MapGet("/", GetProjects);
            group.MapGet("/{id}", GetProject);
            group.MapGet("/{id}/with-tasks", GetProjectWithTasks);
            group.MapPost("/", CreateProject);
            group.MapPut("/{id}", UpdateProject);
            group.MapDelete("/{id}", DeleteProject);
        }

        private static async Task<IResult> GetProjects(IProjectRepository repository, IMapper mapper)
        {
            var projects = await repository.GetAllActiveAsync();
            var projectDtos = mapper.Map<IEnumerable<ProjectDto>>(projects);
            return Results.Ok(projectDtos);
        }

        private static async Task<IResult> GetProject(int id, IProjectRepository repository, IMapper mapper)
        {
            var project = await repository.GetByIdAsync(id);

            if (project == null)
                return Results.NotFound();

            var projectDto = mapper.Map<ProjectDto>(project);
            return Results.Ok(projectDto);
        }
        private static async Task<IResult> GetProjectWithTasks(int id, IProjectRepository repository, IMapper mapper)
        {
            var project = await repository.GetByIdWithTasksAsync(id);

            if (project == null)
                return Results.NotFound();

            var projectDto = mapper.Map<ProjectWithTasksDto>(project);
            return Results.Ok(projectDto);
        }
        private static async Task<IResult> CreateProject(
            CreateProjectDto createDto,
            IProjectRepository repository,
            IUserRepository userRepository,
            IValidator<CreateProjectDto> validator,
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

            var creator = await userRepository.GetByIdAsync(createDto.CreatedById);
            if (creator == null)
                return Results.BadRequest("Creator user not found");

            var project = mapper.Map<Project>(createDto);

            var createdProject = await repository.CreateAsync(project);
            var projectDto = mapper.Map<ProjectDto>(createdProject);

            return Results.Created($"/api/projects/{createdProject.Id}", projectDto);
        }
        private static async Task<IResult> UpdateProject(
            int id,
            UpdateProjectDto updateDto,
            IProjectRepository repository,
            IMapper mapper)
        {
            var existingProject = await repository.GetByIdAsync(id);
            if (existingProject == null)
                return Results.NotFound();

            mapper.Map(updateDto, existingProject);

            var updatedProject = await repository.UpdateAsync(existingProject);
            var projectDto = mapper.Map<ProjectDto>(updatedProject);

            return Results.Ok(projectDto);
        }
        private static async Task<IResult> DeleteProject(int id, IProjectRepository repository)
        {
            var success = await repository.DeleteAsync(id);

            if (!success)
                return Results.NotFound();

            return Results.NoContent();
        }
    }
}
