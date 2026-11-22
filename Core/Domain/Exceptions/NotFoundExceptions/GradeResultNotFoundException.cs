namespace Domain.Exceptions.NotFoundExceptions
{
    public class GradeResultNotFoundException(int id) : NotFoundException($"GradeResult with Id [[{id}]] is not found.")
    {
    }
}
