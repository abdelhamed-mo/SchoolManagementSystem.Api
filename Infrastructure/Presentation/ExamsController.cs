using Microsoft.AspNetCore.Mvc;
using ServiceAbstraction;
using Shared;
using Shared.Params;

namespace Presentation
{
    public class ExamsController(IServiceManager serviceManager) : ApiBaseController
    {
        [HttpGet]
        public async Task<ActionResult<PaginatedResultDto<ExamDto>>> Get([FromQuery] ExamsParams _params)
            => await serviceManager.ExamService.GetExamsAsync(_params);

        [HttpGet("{id:int}")]
        public async Task<ActionResult<ExamDto>> GetById(int id)
            => await serviceManager.ExamService.GetExamByIdAsync(id);

        [HttpPost]
        public async Task<ActionResult<ExamDto>> Create([FromBody] CreateExamDto dto)
            => await serviceManager.ExamService.CreateExamAsync(dto);
        
        [HttpPut]
        public async Task<ActionResult<ExamDto>> Update([FromBody] UpdateExamDto dto)
            => await serviceManager.ExamService.UpdateExamAsync(dto);

        [HttpDelete("{id:int}")]
        public async Task<ActionResult<string>> Delete(int id)
            => await serviceManager.ExamService.DeleteExamAsync(id);
    }
}
