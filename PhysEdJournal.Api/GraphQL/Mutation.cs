using HotChocolate.AspNetCore.Authorization;

namespace PhysEdJournal.Api.GraphQL;

[Authorize(Roles = new[] { "staff", "admin" })]
public class Mutation
{
    public string Check()
    {
        return "Success";
    }
}