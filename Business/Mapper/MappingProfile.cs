using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AutoMapper;
using ColleagueRequestManager.Models;
using DataAccess;
using Models;

namespace Business.Mapper
{
    internal class MappingProfile : Profile
    {
        public MappingProfile()
        {
            CreateMap<ApplicationUser, ApplicationUserModel>().ReverseMap();
            CreateMap<ToDoItem, ToDoItemDto>().ReverseMap();
            CreateMap<ToDoAttachment, ToDoAttachmentDto>().ReverseMap();
            CreateMap<ToDoResponse, ToDoResponseDto>().ReverseMap();
            CreateMap<ToDoHistory, ToDoHistoryDto>().ReverseMap();
        }
    }
}
