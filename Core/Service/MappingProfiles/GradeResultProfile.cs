using AutoMapper;
using Domain.Entities;
using Shared;

namespace Service.MappingProfiles
{
    public class GradeResultProfile : Profile
    {
        public GradeResultProfile()
        {
            CreateMap<GradeResult, GradeResultDto>()
                .ForMember(destination => destination.ExamName, options => options.MapFrom(source => source.Exam.Name))
                .ForMember(destination => destination.StudentName, options => options.MapFrom(source => source.Student.FullName))
                .ForMember(destination => destination.GradeName, options => options.MapFrom(source => source.Grade.Name))
                .ForMember(destination => destination.MaxScore, options => options.MapFrom(source => source.Exam.MaxScore))
                .ForMember(destination => destination.PassScore, options => options.MapFrom(source => source.Exam.PassScore))
                .ForMember(destination => destination.IsPassed, options => options.MapFrom(source => source.Score >= source.Exam.PassScore));
        }
    }
}
