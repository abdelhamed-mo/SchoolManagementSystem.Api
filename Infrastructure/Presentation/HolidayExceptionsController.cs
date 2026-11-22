using Microsoft.AspNetCore.Mvc;
using ServiceAbstraction;
using Shared;

namespace Presentation
{
    public class HolidayExceptionsController(IServiceManager serviceManager) : ApiBaseController
    {
        [HttpGet]
        public async Task<ActionResult<IEnumerable<HolidayExceptionDto>>> Get()
            => new ActionResult<IEnumerable<HolidayExceptionDto>>(
                (await serviceManager.HolidayService.GetAllAsync()).ToList());

        [HttpPost]
        public async Task<ActionResult<HolidayExceptionDto>> Create([FromBody] CreateHolidayExceptionDto dto)
            => await serviceManager.HolidayService.CreateAsync(dto);

        [HttpDelete("{id:int}")]
        public async Task<ActionResult<string>> Remove(int id)
            => await serviceManager.HolidayService.RemoveAsync(id);
    }
}
