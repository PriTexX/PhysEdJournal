namespace PhysEdJournal.Core.Exceptions.GroupExceptions;

public class GroupNotFoundException : Exception
{
    public GroupNotFoundException(string groupGuid)
        : base($"No group with guid: {groupGuid}") { }
}
