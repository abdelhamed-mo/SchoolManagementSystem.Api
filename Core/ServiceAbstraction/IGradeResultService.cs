using Shared;
using Shared.Params;

namespace ServiceAbstraction
{
    public interface IGradeResultService
    {
        Task<PaginatedResultDto<GradeResultDto>> GetGradeResultsAsync(GradeResultsParams parameters);
        Task<GradeResultDto> GetGradeResultByIdAsync(int id);
        Task<GradeResultDto> CreateGradeResultAsync(CreateGradeResultDto dto);
        Task<GradeResultDto> UpdateGradeResultAsync(UpdateGradeResultDto dto);
        Task<string> DeleteGradeResultAsync(int id);
    }
}
