using AutoMapper;
using Domain.Entities;
using Shared;

namespace Service.MappingProfiles
{
    public class ExamProfile : Profile
    {
        public ExamProfile()
        {
            CreateMap<Exam, ExamDto>()
                .ForMember(d => d.Subject, opt => opt.MapFrom(s => s.Subject.Name))
                .ForMember(d => d.Class, opt => opt.MapFrom(s => s.Class.Name))
                .ForMember(d => d.Grade, opt => opt.MapFrom(s => s.Grade.Name))
                .ForMember(d => d.Semester, opt => opt.MapFrom(s => s.Semester == null ? null : s.Semester.Name))
                .ForMember(d => d.AcademicYear, opt => opt.MapFrom(s => s.Semester == null ? null : s.Semester.AcademicYear.Name));
        }
    }
}
