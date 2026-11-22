namespace Domain.Exceptions.NotFoundExceptions
{
    public class SemesterNotFoundException(int id) : NotFoundException($"Semester with Id [{id}] is not found.")
    {
    }
}
