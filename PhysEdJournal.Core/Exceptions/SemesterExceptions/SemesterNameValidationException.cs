namespace PhysEdJournal.Core.Exceptions.SemesterExceptions;

public class SemesterNameValidationException : Exception
{
    public SemesterNameValidationException()
        : base("Semester name must be like: 2022-2023/весна") { }
}
