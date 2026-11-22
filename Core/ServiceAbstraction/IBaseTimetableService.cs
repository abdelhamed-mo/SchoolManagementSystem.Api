using Shared;

namespace ServiceAbstraction
{
    public interface IBaseTimetableService
    {
        Task<BaseTimetableDto> CreateAsync(CreateBaseTimetableDto dto);
        Task<BaseTimetableDto> UpdateAsync(UpdateBaseTimetableDto dto);
        Task<BaseTimetableDto> GetByClassAsync(int classId);
    }
}
