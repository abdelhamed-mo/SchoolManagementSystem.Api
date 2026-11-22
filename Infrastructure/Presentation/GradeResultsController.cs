using Microsoft.AspNetCore.Mvc;
using ServiceAbstraction;
using Shared;
using Shared.Params;

namespace Presentation
{
    public class GradeResultsController(IServiceManager serviceManager) : ApiBaseController
    {
        [HttpGet]
        public async Task<ActionResult<PaginatedResultDto<GradeResultDto>>> Get([FromQuery] GradeResultsParams parameters)
            => await serviceManager.GradeResultService.GetGradeResultsAsync(parameters);

        [HttpGet("{id:int}")]
        public async Task<ActionResult<GradeResultDto>> GetById(int id)
            => await serviceManager.GradeResultService.GetGradeResultByIdAsync(id);

        [HttpPost]
        public async Task<ActionResult<GradeResultDto>> Create([FromBody] CreateGradeResultDto dto)
            => await serviceManager.GradeResultService.CreateGradeResultAsync(dto);

        [HttpPut]
        public async Task<ActionResult<GradeResultDto>> Update([FromBody] UpdateGradeResultDto dto)
            => await serviceManager.GradeResultService.UpdateGradeResultAsync(dto);

        [HttpDelete("{id:int}")]
        public async Task<ActionResult<string>> Delete(int id)
            => await serviceManager.GradeResultService.DeleteGradeResultAsync(id);
    }
}