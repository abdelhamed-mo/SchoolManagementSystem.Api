using Shared;
using Shared.Params;

namespace ServiceAbstraction
{
    public interface IAcademicYearService
    {
        Task<PaginatedResultDto<AcademicYearDto>> GetAcademicYearsAsync(AcademicYearsParams parameters);
        Task<AcademicYearDto> GetAcademicYearByIdAsync(int id);
        Task<AcademicYearDto> GetActiveAcademicYearAsync();
        Task<AcademicYearDto> CreateAcademicYearAsync(CreateAcademicYearDto dto);
        Task<AcademicYearDto> UpdateAcademicYearAsync(UpdateAcademicYearDto dto);
        Task<AcademicYearDto> ActivateAcademicYearAsync(int id);
        Task<AcademicYearDto> DeactivateAcademicYearAsync(int id);
        Task<string> DeleteAcademicYearAsync(int id);
    }
}
