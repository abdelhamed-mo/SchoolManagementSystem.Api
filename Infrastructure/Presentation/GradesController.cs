using Microsoft.AspNetCore.Mvc;
using ServiceAbstraction;
using Shared;
using Shared.Params;

namespace Presentation
{
    public class GradesController(IServiceManager serviceManager) : ApiBaseController
    {
        [HttpGet]
        public async Task<ActionResult<PaginatedResultDto<GradeDto>>> Get([FromQuery] GradesParams _params)
            => await serviceManager.GradeService.GetGradesAsync(_params);

        [HttpGet("{id:int}")]
        public async Task<ActionResult<GradeDto>> GetById(int id)
            => await serviceManager.GradeService.GetGradeByIdAsync(id);
    }
}
