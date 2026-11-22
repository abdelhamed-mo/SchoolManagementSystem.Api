using Shared;
using Shared.Params;

namespace ServiceAbstraction
{
    public interface ISemesterService
    {
        Task<PaginatedResultDto<SemesterDto>> GetSemestersAsync(SemestersParams parameters);
        Task<PaginatedResultDto<SemesterDto>> GetSemestersByAcademicYearAsync(int academicYearId, SemestersParams parameters);
        Task<SemesterDto> GetSemesterByIdAsync(int id);
        Task<SemesterDto> GetActiveSemesterAsync();
        Task<SemesterDto> CreateSemesterAsync(CreateSemesterDto dto);
        Task<SemesterDto> UpdateSemesterAsync(UpdateSemesterDto dto);
        Task<SemesterDto> ActivateSemesterAsync(int id);
        Task<SemesterDto> DeactivateSemesterAsync(int id);
        Task<string> DeleteSemesterAsync(int id);
    }
}
