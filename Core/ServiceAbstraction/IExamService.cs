using Shared;
using Shared.Params;
using System;
using System.Collections.Generic;
using System.Text;

namespace ServiceAbstraction
{
    public interface IExamService
    {
        Task<PaginatedResultDto<ExamDto>> GetExamsAsync(ExamsParams _params);
        Task<ExamDto> GetExamByIdAsync(int id);
        Task<ExamDto> CreateExamAsync(CreateExamDto dto);
        Task<ExamDto> UpdateExamAsync(UpdateExamDto dto);
        Task<string> DeleteExamAsync(int id);
    }
}
