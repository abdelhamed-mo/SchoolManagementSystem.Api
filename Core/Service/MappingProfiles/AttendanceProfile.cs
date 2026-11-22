using AutoMapper;
using Domain.Entities;
using Shared;

namespace Service.MappingProfiles
{
    public class AttendanceProfile : Profile
    {
        public AttendanceProfile()
        {
            CreateMap<Attendance, AttendanceDto>()
                .ForMember(destination => destination.StudentName,
                    options => options.MapFrom(source => source.Student.FullName))
                .ForMember(destination => destination.ClassName,
                    options => options.MapFrom(source => source.Class.Name));
        }
    }
}
