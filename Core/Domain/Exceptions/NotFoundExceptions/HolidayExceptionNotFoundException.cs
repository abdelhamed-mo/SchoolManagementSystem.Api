using Domain.Exceptions;

namespace Domain.Exceptions.NotFoundExceptions
{
    public class HolidayExceptionNotFoundException(int id)
        : NotFoundException($"Holiday exception with Id [{id}] is not found.")
    {
    }
}
