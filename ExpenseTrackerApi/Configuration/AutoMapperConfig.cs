using AutoMapper;
using ExpenseTrackerApi.Entities;
using ExpenseTrackerApi.Models.DTO;

namespace ExpenseTrackerApi.Configuration
{
    public class AutoMapperConfig : Profile
    {
        public AutoMapperConfig()
        {
            CreateMap<RegisterDTO, Users>();
            CreateMap<RoleDTO, Role>().ReverseMap();
            CreateMap<ExpenseDTO, Expenses>().ReverseMap();
        }
    }
}
