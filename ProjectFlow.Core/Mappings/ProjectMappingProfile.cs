using AutoMapper;
using ProjectFlow.Core.DTOs;
using ProjectFlow.Core.Entities;
using ProjectFlow.Core.Enums;

namespace ProjectFlow.Core.Mappings
{
    public class ProjectMappingProfile : Profile
    {
        public ProjectMappingProfile()
        {
            // Entity to DTO
            CreateMap<Project, ProjectDto>()
                .ForMember(dest => dest.CreatedByName,
                    opt => opt.MapFrom(src => $"{src.CreatedBy.FirstName} {src.CreatedBy.LastName}"))
                .ForMember(dest => dest.TaskCount,
                    opt => opt.MapFrom(src => src.Tasks.Count))
                .ForMember(dest => dest.CompletedTaskCount,
                    opt => opt.MapFrom(src => src.Tasks.Count(t => t.Status == ProjectTaskStatus.Done)));

            CreateMap<Project, ProjectWithTasksDto>()
                .ForMember(dest => dest.CreatedByName,
                    opt => opt.MapFrom(src => $"{src.CreatedBy.FirstName} {src.CreatedBy.LastName}"))
                .ForMember(dest => dest.TaskCount,
                    opt => opt.MapFrom(src => src.Tasks.Count))
                .ForMember(dest => dest.CompletedTaskCount,
                    opt => opt.MapFrom(src => src.Tasks.Count(t => t.Status == ProjectTaskStatus.Done)));

            // DTO to Entity
            CreateMap<CreateProjectDto, Project>()
                .ForMember(dest => dest.Id, opt => opt.Ignore())
                .ForMember(dest => dest.Status, opt => opt.MapFrom(src => ProjectStatus.Planning))
                .ForMember(dest => dest.CreatedAt, opt => opt.MapFrom(src => DateTime.UtcNow))
                .ForMember(dest => dest.IsActive, opt => opt.MapFrom(src => true))
                .ForMember(dest => dest.CreatedBy, opt => opt.Ignore())
                .ForMember(dest => dest.Tasks, opt => opt.Ignore());

            CreateMap<UpdateProjectDto, Project>()
                .ForMember(dest => dest.Id, opt => opt.Ignore())
                .ForMember(dest => dest.CreatedById, opt => opt.Ignore())
                .ForMember(dest => dest.CreatedAt, opt => opt.Ignore())
                .ForMember(dest => dest.IsActive, opt => opt.Ignore())
                .ForMember(dest => dest.CreatedBy, opt => opt.Ignore())
                .ForMember(dest => dest.Tasks, opt => opt.Ignore());

            CreateMap<ProjectTask, TaskDto>()
                .ForMember(dest => dest.AssignedToName,
                    opt => opt.MapFrom(src => src.AssignedTo != null
                        ? $"{src.AssignedTo.FirstName} {src.AssignedTo.LastName}"
                        : null));

            CreateMap<ProjectTask, TaskDto>()
                .ForMember(dest => dest.ProjectName, opt => opt.MapFrom(src => src.Project.Name))
                .ForMember(dest => dest.AssignedToName,
                    opt => opt.MapFrom(src => src.AssignedTo != null
                        ? $"{src.AssignedTo.FirstName} {src.AssignedTo.LastName}"
                        : null));

            CreateMap<ProjectTask, TaskWithDetailsDto>()
                .ForMember(dest => dest.ProjectName, opt => opt.MapFrom(src => src.Project.Name))
                .ForMember(dest => dest.ProjectDescription, opt => opt.MapFrom(src => src.Project.Description))
                .ForMember(dest => dest.ProjectCreatedByName,
                    opt => opt.MapFrom(src => $"{src.Project.CreatedBy.FirstName} {src.Project.CreatedBy.LastName}"))
                .ForMember(dest => dest.AssignedToName,
                    opt => opt.MapFrom(src => src.AssignedTo != null
                        ? $"{src.AssignedTo.FirstName} {src.AssignedTo.LastName}"
                        : null));

            CreateMap<CreateTaskDto, ProjectTask>()
                .ForMember(dest => dest.Id, opt => opt.Ignore())
                .ForMember(dest => dest.Status, opt => opt.MapFrom(src => ProjectTaskStatus.ToDo))
                .ForMember(dest => dest.CreatedAt, opt => opt.Ignore())
                .ForMember(dest => dest.UpdatedAt, opt => opt.Ignore())
                .ForMember(dest => dest.Project, opt => opt.Ignore())
                .ForMember(dest => dest.AssignedTo, opt => opt.Ignore());

            CreateMap<UpdateTaskDto, ProjectTask>()
                .ForMember(dest => dest.Id, opt => opt.Ignore())
                .ForMember(dest => dest.ProjectId, opt => opt.Ignore())
                .ForMember(dest => dest.CreatedAt, opt => opt.Ignore())
                .ForMember(dest => dest.UpdatedAt, opt => opt.Ignore())
                .ForMember(dest => dest.Project, opt => opt.Ignore())
                .ForMember(dest => dest.AssignedTo, opt => opt.Ignore());
        }
    }
}
