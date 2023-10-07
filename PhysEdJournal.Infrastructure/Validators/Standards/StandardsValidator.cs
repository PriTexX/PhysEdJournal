using PhysEdJournal.Core.Exceptions.StudentExceptions;
using static PhysEdJournal.Core.Constants.PointsConstants;
namespace PhysEdJournal.Infrastructure.Validators.Standards;

public class StandardsValidator
{
    public void ValidateStudentPointsForStandardsAndThrow(int pointsToAdd, int pointsForStandards, string studentGuid)
    {
        var validationResult = ValidateStudentPointsForStandards(pointsToAdd, pointsForStandards, studentGuid);

        validationResult.Match(_ => true, exception => throw exception);
    }
    
    public LanguageExt.Common.Result<bool> ValidateStudentPointsForStandards(int pointsToAdd, int pointsForStandards, string studentGuid)
    {
        if (pointsForStandards == MAX_POINTS_FOR_STANDARDS)
        {
            return new LanguageExt.Common.Result<bool>(new OverAbundanceOfPointsForStudentException(studentGuid));
        }

        return true;
    }
}