using Shared;
using Shared.Params;

namespace ServiceAbstraction
{
    public interface IEnrollmentService
    {
        Task<PaginatedResultDto<EnrollmentDto>> GetEnrollmentsAsync(EnrollmentsParams parameters);
        Task<EnrollmentDto> GetEnrollmentByIdAsync(int id);
        Task<IEnumerable<EnrollmentDto>> GetStudentEnrollmentHistoryAsync(int studentId);
        Task<EnrollmentDto> GetCurrentStudentEnrollmentAsync(int studentId);
        Task<EnrollmentDto> CreateEnrollmentAsync(CreateEnrollmentDto dto);
        Task<EnrollmentDto> TransferStudentAsync(TransferEnrollmentDto dto);
        Task<EnrollmentDto> UpdateEnrollmentAsync(UpdateEnrollmentDto dto);
        Task<string> DeleteEnrollmentAsync(int id);
    }
}
