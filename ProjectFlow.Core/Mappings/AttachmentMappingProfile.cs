using AutoMapper;
using ProjectFlow.Core.DTOs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ProjectFlow.Core.Entities;

namespace ProjectFlow.Core.Mappings
{
    public class AttachmentMappingProfile : Profile
    {
        public AttachmentMappingProfile()
        {
            // Entity to DTO
            CreateMap<Attachment, AttachmentDto>()
                .ForMember(dest => dest.UploadedByName,
                    opt => opt.MapFrom(src => $"{src.UploadedBy.FirstName} {src.UploadedBy.LastName}"));

            // DTO to Entity
            CreateMap<CreateAttachmentDto, Attachment>()
                .ForMember(dest => dest.Id, opt => opt.Ignore())
                .ForMember(dest => dest.FilePath, opt => opt.Ignore())
                .ForMember(dest => dest.UploadedById, opt => opt.Ignore())
                .ForMember(dest => dest.CreatedAt, opt => opt.Ignore())
                .ForMember(dest => dest.Task, opt => opt.Ignore())
                .ForMember(dest => dest.UploadedBy, opt => opt.Ignore());
        }
    }
}
