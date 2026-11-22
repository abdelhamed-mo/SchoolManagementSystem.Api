using AutoMapper;
using Domain.Entities;
using Shared;

namespace Service.MappingProfiles
{
    public class SemesterProfile : Profile
    {
        public SemesterProfile()
        {
            CreateMap<Semester, SemesterDto>()
                .ForMember(destination => destination.AcademicYearName, options => options.MapFrom(source => source.AcademicYear.Name));
        }
    }
}
