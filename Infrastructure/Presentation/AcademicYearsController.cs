using Microsoft.AspNetCore.Mvc;
using ServiceAbstraction;
using Shared;
using Shared.Params;

namespace Presentation
{
    public class AcademicYearsController(IServiceManager serviceManager) : ApiBaseController
    {
        [HttpGet]
        public async Task<ActionResult<PaginatedResultDto<AcademicYearDto>>> Get([FromQuery] AcademicYearsParams parameters)
            => await serviceManager.AcademicYearService.GetAcademicYearsAsync(parameters);

        [HttpGet("current")]
        public async Task<ActionResult<AcademicYearDto>> GetCurrent()
            => await serviceManager.AcademicYearService.GetActiveAcademicYearAsync();

        [HttpGet("{id:int}")]
        public async Task<ActionResult<AcademicYearDto>> GetById(int id)
            => await serviceManager.AcademicYearService.GetAcademicYearByIdAsync(id);

        [HttpPost]
        public async Task<ActionResult<AcademicYearDto>> Create([FromBody] CreateAcademicYearDto dto)
            => await serviceManager.AcademicYearService.CreateAcademicYearAsync(dto);

        [HttpPut]
        public async Task<ActionResult<AcademicYearDto>> Update([FromBody] UpdateAcademicYearDto dto)
            => await serviceManager.AcademicYearService.UpdateAcademicYearAsync(dto);

        [HttpPost("{id:int}/activate")]
        public async Task<ActionResult<AcademicYearDto>> Activate(int id)
            => await serviceManager.AcademicYearService.ActivateAcademicYearAsync(id);

        [HttpPost("{id:int}/deactivate")]
        public async Task<ActionResult<AcademicYearDto>> Deactivate(int id)
            => await serviceManager.AcademicYearService.DeactivateAcademicYearAsync(id);

        [HttpDelete("{id:int}")]
        public async Task<ActionResult<string>> Delete(int id)
            => await serviceManager.AcademicYearService.DeleteAcademicYearAsync(id);
    }
}
