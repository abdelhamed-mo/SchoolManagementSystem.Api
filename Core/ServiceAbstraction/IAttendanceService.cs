using Shared;
using Shared.Params;

namespace ServiceAbstraction
{
    public interface IAttendanceService
    {
        Task<PaginatedResultDto<AttendanceDto>> GetAttendancesAsync(AttendancesParams parameters);
        Task<IEnumerable<AttendanceDto>> GetAttendanceByStudentAsync(int studentId);
        Task<AttendanceDto> GetAttendanceByIdAsync(int id);
        Task<AttendanceDto> CreateAttendanceAsync(CreateAttendanceDto dto);
        Task<AttendanceDto> UpdateAttendanceAsync(UpdateAttendanceDto dto);
        Task<string> DeleteAttendanceAsync(int id);
    }
}
