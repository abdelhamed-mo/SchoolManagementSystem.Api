using AutoMapper;
using Domain.Entities;
using Shared;

namespace Service.MappingProfiles
{
    public class AcademicYearProfile : Profile
    {
        public AcademicYearProfile()
        {
            CreateMap<AcademicYear, AcademicYearDto>()
                .ForMember(destination => destination.SemesterCount, options => options.MapFrom(source => source.Semesters.Count));
        }
    }
}
