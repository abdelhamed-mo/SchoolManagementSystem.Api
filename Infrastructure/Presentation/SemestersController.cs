using Microsoft.AspNetCore.Mvc;
using ServiceAbstraction;
using Shared;
using Shared.Params;

namespace Presentation
{
    public class SemestersController(IServiceManager serviceManager) : ApiBaseController
    {
        [HttpGet]
        public async Task<ActionResult<PaginatedResultDto<SemesterDto>>> Get([FromQuery] SemestersParams parameters)
            => await serviceManager.SemesterService.GetSemestersAsync(parameters);

        [HttpGet("current")]
        public async Task<ActionResult<SemesterDto>> GetCurrent()
            => await serviceManager.SemesterService.GetActiveSemesterAsync();

        [HttpGet("academic-year/{academicYearId:int}")]
        public async Task<ActionResult<PaginatedResultDto<SemesterDto>>> GetByAcademicYear(int academicYearId, [FromQuery] SemestersParams parameters)
            => await serviceManager.SemesterService.GetSemestersByAcademicYearAsync(academicYearId, parameters);

        [HttpGet("{id:int}")]
        public async Task<ActionResult<SemesterDto>> GetById(int id)
            => await serviceManager.SemesterService.GetSemesterByIdAsync(id);

        [HttpPost]
        public async Task<ActionResult<SemesterDto>> Create([FromBody] CreateSemesterDto dto)
            => await serviceManager.SemesterService.CreateSemesterAsync(dto);

        [HttpPut]
        public async Task<ActionResult<SemesterDto>> Update([FromBody] UpdateSemesterDto dto)
            => await serviceManager.SemesterService.UpdateSemesterAsync(dto);

        [HttpPost("{id:int}/activate")]
        public async Task<ActionResult<SemesterDto>> Activate(int id)
            => await serviceManager.SemesterService.ActivateSemesterAsync(id);

        [HttpPost("{id:int}/deactivate")]
        public async Task<ActionResult<SemesterDto>> Deactivate(int id)
            => await serviceManager.SemesterService.DeactivateSemesterAsync(id);

        [HttpDelete("{id:int}")]
        public async Task<ActionResult<string>> Delete(int id)
            => await serviceManager.SemesterService.DeleteSemesterAsync(id);
    }
}
