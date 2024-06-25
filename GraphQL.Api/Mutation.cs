using HotChocolate.AspNetCore.Authorization;

namespace GraphQL.Api;

[Authorize(Roles = new[] { "staff", "admin" })]
public class Mutation
{
    public string Check()
    {
        return "Success";
    }
}
