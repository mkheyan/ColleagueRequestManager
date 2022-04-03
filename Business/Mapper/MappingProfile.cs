using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AutoMapper;
using ColleagueRequestManager.Models;
using DataAccess;

namespace Business.Mapper
{
    internal class MappingProfile : Profile
    {
        public MappingProfile()
        {
            CreateMap<ApplicationUser, ApplicationUserModel>().ReverseMap();
        }
    }
}
