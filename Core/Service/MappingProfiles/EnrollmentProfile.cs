using AutoMapper;
using Domain.Entities;
using Shared;

namespace Service.MappingProfiles
{
    public class EnrollmentProfile : Profile
    {
        public EnrollmentProfile()
        {
            CreateMap<Enrollment, EnrollmentDto>()
                .ForMember(destination => destination.StudentName, options => options.MapFrom(source => source.Student.FullName))
                .ForMember(destination => destination.ClassName, options => options.MapFrom(source => source.Class.Name))
                .ForMember(destination => destination.AcademicYearName, options => options.MapFrom(source => source.AcademicYear == null ? null : source.AcademicYear.Name));
        }
    }
}
