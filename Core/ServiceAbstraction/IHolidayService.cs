using Shared;

namespace ServiceAbstraction
{
    public interface IHolidayService
    {
        Task<HolidayExceptionDto> CreateAsync(CreateHolidayExceptionDto dto);
        Task<IEnumerable<HolidayExceptionDto>> GetAllAsync();
        Task<string> RemoveAsync(int id);
    }
}
