using Domain.Exceptions;

namespace Domain.Exceptions.NotFoundExceptions
{
    public class ScheduledLessonNotFoundException(int id)
        : NotFoundException($"Scheduled lesson with Id [{id}] is not found.")
    {
    }
}
