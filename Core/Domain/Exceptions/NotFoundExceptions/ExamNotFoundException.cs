namespace Domain.Exceptions.NotFoundExceptions
{
    public class ExamNotFoundException(int id) : NotFoundException($"Exam with Id [[{id}]] is not found.")
    {
    }
}
