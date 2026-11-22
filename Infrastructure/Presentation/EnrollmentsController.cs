using Microsoft.AspNetCore.Mvc;
using ServiceAbstraction;
using Shared;
using Shared.Params;

namespace Presentation
{
    public class EnrollmentsController(IServiceManager serviceManager) : ApiBaseController
    {
        [HttpGet]
        public async Task<ActionResult<PaginatedResultDto<EnrollmentDto>>> Get([FromQuery] EnrollmentsParams parameters)
            => await serviceManager.EnrollmentService.GetEnrollmentsAsync(parameters);

        [HttpGet("{id:int}")]
        public async Task<ActionResult<EnrollmentDto>> GetById(int id)
            => await serviceManager.EnrollmentService.GetEnrollmentByIdAsync(id);

        [HttpGet("student/{studentId:int}/history")]
        public async Task<ActionResult<IEnumerable<EnrollmentDto>>> GetStudentHistory(int studentId)
            => (await serviceManager.EnrollmentService.GetStudentEnrollmentHistoryAsync(studentId)).ToList();

        [HttpGet("student/{studentId:int}/current")]
        public async Task<ActionResult<EnrollmentDto>> GetCurrentStudentEnrollment(int studentId)
            => await serviceManager.EnrollmentService.GetCurrentStudentEnrollmentAsync(studentId);

        [HttpPost]
        public async Task<ActionResult<EnrollmentDto>> Create([FromBody] CreateEnrollmentDto dto)
            => await serviceManager.EnrollmentService.CreateEnrollmentAsync(dto);

        [HttpPost("transfer")]
        public async Task<ActionResult<EnrollmentDto>> Transfer([FromBody] TransferEnrollmentDto dto)
            => await serviceManager.EnrollmentService.TransferStudentAsync(dto);

        [HttpPut]
        public async Task<ActionResult<EnrollmentDto>> Update([FromBody] UpdateEnrollmentDto dto)
            => await serviceManager.EnrollmentService.UpdateEnrollmentAsync(dto);

        [HttpDelete("{id:int}")]
        public async Task<ActionResult<string>> Delete(int id)
            => await serviceManager.EnrollmentService.DeleteEnrollmentAsync(id);
    }
}
