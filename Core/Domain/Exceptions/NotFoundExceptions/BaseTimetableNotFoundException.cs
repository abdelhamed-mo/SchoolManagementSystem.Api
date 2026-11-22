using Domain.Exceptions;

namespace Domain.Exceptions.NotFoundExceptions
{
    public class BaseTimetableNotFoundException(int id)
        : NotFoundException($"Base timetable with Id [{id}] is not found.")
    {
    }
}
