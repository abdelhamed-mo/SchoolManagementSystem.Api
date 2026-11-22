using Microsoft.AspNetCore.Mvc;
using ServiceAbstraction;
using Shared;

namespace Presentation
{
    public class BaseTimetablesController(IServiceManager serviceManager) : ApiBaseController
    {
        [HttpPost]
        public async Task<ActionResult<BaseTimetableDto>> Create([FromBody] CreateBaseTimetableDto dto)
            => await serviceManager.BaseTimetableService.CreateAsync(dto);

        [HttpGet("class/{classId:int}")]
        public async Task<ActionResult<BaseTimetableDto>> GetByClass(int classId)
            => await serviceManager.BaseTimetableService.GetByClassAsync(classId);

        [HttpPut]
        public async Task<ActionResult<BaseTimetableDto>> Update([FromBody] UpdateBaseTimetableDto dto)
            => await serviceManager.BaseTimetableService.UpdateAsync(dto);
    }
}
