using Domain.Exceptions;

namespace Domain.Exceptions.NotFoundExceptions
{
    public class AttendanceNotFoundException(int id)
        : NotFoundException($"Attendance with Id [[{id}]] is not found.")
    {
    }
}
