using AutoMapper;
using Domain.Contracts;
using Domain.Entities;
using ServiceAbstraction;
using Shared;
using Shared.Params;
using Domain.Exceptions.NotFoundExceptions;

namespace Service.GradeService
{
    public class GradeService : IGradeService
    {
        private readonly IUnitOfWork unitOfWork;
        private readonly IMapper mapper;

        public GradeService(IUnitOfWork unitOfWork, IMapper mapper)
        {
            this.unitOfWork = unitOfWork;
            this.mapper = mapper;
        }

        public async Task<PaginatedResultDto<GradeDto>> GetGradesAsync(GradesParams _params)
        {
            // build specifications: one for count (no pagination) and one for page
            var specForCount = new Service.ConcreteSpecifications.GradeSpecifications(_params, applyPagination: false);
            var specForPage = new Service.ConcreteSpecifications.GradeSpecifications(_params, applyPagination: true);

            var total = await unitOfWork.GetRepository<Grade, int>().CountAsync(specForCount);

            var grades = await unitOfWork.GetRepository<Grade, int>().GetAllAsync(specForPage);

            var dto = mapper.Map<IEnumerable<GradeDto>>(grades);

            return new PaginatedResultDto<GradeDto>(dto.Count(), _params.PageIndex, total, dto);
        }

        public async Task<GradeDto> GetGradeByIdAsync(int id)
        {
            var grade = await unitOfWork.GetRepository<Grade, int>().GetAsync(id);
            if (grade is null) throw new GradeNotFoundException(id);
            return mapper.Map<GradeDto>(grade);
        }
    }
}
