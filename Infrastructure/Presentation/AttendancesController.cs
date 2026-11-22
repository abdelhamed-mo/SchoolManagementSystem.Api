using Microsoft.AspNetCore.Mvc;
using ServiceAbstraction;
using Shared;
using Shared.Params;

namespace Presentation
{
    public class AttendancesController(IServiceManager serviceManager) : ApiBaseController
    {
        [HttpGet]
        public async Task<ActionResult<PaginatedResultDto<AttendanceDto>>> Get([FromQuery] AttendancesParams parameters)
            => await serviceManager.AttendanceService.GetAttendancesAsync(parameters);

        [HttpGet("student/{studentId:int}")]
        public async Task<ActionResult<IEnumerable<AttendanceDto>>> GetByStudent(int studentId)
            => new ActionResult<IEnumerable<AttendanceDto>>(
                (await serviceManager.AttendanceService.GetAttendanceByStudentAsync(studentId)).ToList());

        [HttpGet("{id:int}")]
        public async Task<ActionResult<AttendanceDto>> GetById(int id)
            => await serviceManager.AttendanceService.GetAttendanceByIdAsync(id);

        [HttpPost]
        public async Task<ActionResult<AttendanceDto>> Create([FromBody] CreateAttendanceDto dto)
            => await serviceManager.AttendanceService.CreateAttendanceAsync(dto);

        [HttpPut]
        public async Task<ActionResult<AttendanceDto>> Update([FromBody] UpdateAttendanceDto dto)
            => await serviceManager.AttendanceService.UpdateAttendanceAsync(dto);

        [HttpDelete("{id:int}")]
        public async Task<ActionResult<string>> Delete(int id)
            => await serviceManager.AttendanceService.DeleteAttendanceAsync(id);
    }
}