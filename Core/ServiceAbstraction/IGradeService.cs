using Shared;
using Shared.Params;
using System;
using System.Collections.Generic;
using System.Text;

namespace ServiceAbstraction
{
    public interface IGradeService
    {
        Task<PaginatedResultDto<GradeDto>> GetGradesAsync(GradesParams _params);
        Task<GradeDto> GetGradeByIdAsync(int id);
    }
}
