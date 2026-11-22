namespace Domain.Exceptions.NotFoundExceptions
{
    public class AcademicYearNotFoundException(int id) : NotFoundException($"Academic year with Id [{id}] is not found.")
    {
    }
}
