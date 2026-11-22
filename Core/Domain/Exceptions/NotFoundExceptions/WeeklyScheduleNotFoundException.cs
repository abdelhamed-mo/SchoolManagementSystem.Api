using Domain.Exceptions;

namespace Domain.Exceptions.NotFoundExceptions
{
    public class WeeklyScheduleNotFoundException(int id)
        : NotFoundException($"Weekly schedule with Id [{id}] is not found.")
    {
    }
}
