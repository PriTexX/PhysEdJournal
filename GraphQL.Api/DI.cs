using DB;
using GraphQL.Api.MutationExtensions;
using GraphQL.Api.QueryExtensions;
using GraphQL.Api.ScalarTypes;
using HotChocolate.Data.Filters;
using HotChocolate.Data.Filters.Expressions;
using HotChocolate.Types.Pagination;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;

namespace GraphQL.Api;

public static class DI
{
    public static void AddGraphQLApi(this IServiceCollection services)
    {
        services.AddScoped<GraphQLPermissionValidator>();

        services
            .AddGraphQLServer()
            .InitializeOnStartup()
            .AddDiagnosticEventListener<ErrorLoggingDiagnosticsEventListener>()
            .AddAuthorization()
            .AddMutationConventions(applyToAllMutations: true)
            .RegisterDbContext<ApplicationContext>()
            .AddQueryType<Query>()
            .AddTypeExtension<TeacherQueryExtensions>()
            .AddTypeExtension<StudentQueryExtensions>()
            .AddType<SuccessType>()
            .AddType<DateOnlyType>()
            .BindRuntimeType<Success, SuccessType>()
            .BindRuntimeType<DateOnly, DateOnlyType>()
            .AddMutationType<Mutation>()
            .AddTypeExtension<StudentMutationExtensions>()
            .AddProjections()
            .AddFiltering()
            .AddConvention<IFilterConvention>(
                new FilterConventionExtension(x =>
                    x.AddProviderExtension(
                        new QueryableFilterProviderExtension(y =>
                            y.AddFieldHandler<QueryableStringInvariantContainsHandler>()
                        )
                    )
                )
            )
            .AddSorting()
            .SetPagingOptions(
                new PagingOptions
                {
                    MaxPageSize = 200,
                    DefaultPageSize = 30,
                    IncludeTotalCount = true,
                }
            );
    }

    public static void UseGraphQLApi(this IEndpointRouteBuilder router)
    {
        router.MapGraphQL();
    }
}
