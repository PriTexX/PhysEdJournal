using System.Linq.Expressions;
using System.Reflection;
using HotChocolate.Data.Filters;
using HotChocolate.Data.Filters.Expressions;
using HotChocolate.Language;

namespace PhysEdJournal.Api.FilterExtensions;

public class QueryableStringInvariantContainsHandler : QueryableStringOperationHandler
{
    // For creating a expression tree we need the `MethodInfo` of the `ToLower` method of string
    private static readonly MethodInfo ToLower = typeof(string)
        .GetMethods()
        .Single(
            x => x.Name == nameof(string.ToLower) &&
                 x.GetParameters().Length == 0);

    // This is used to match the handler to all `contains` fields
    protected override int Operation => DefaultFilterOperations.Contains;

    public override Expression HandleOperation(
        QueryableFilterContext context,
        IFilterOperationField field,
        IValueNode value,
        object? parsedValue)
    {
        // We get the instance of the context. This is the expression path to the property
        // e.g. ~> y.Street
        Expression property = context.GetInstance();
        
        // the parsed value is what was specified in the query
        // e.g. ~> eq: "221B Baker Street"
        if (parsedValue is string str)
        {
            
            // Creates and returns the operation
            // e.g. ~> y.Street.ToLower() == "221b baker street"
            return FilterExpressionBuilder.Contains(Expression.Call(property, ToLower), str.ToLower());
        }

        // Something went wrong 😱
        throw new InvalidOperationException();
    }

    public QueryableStringInvariantContainsHandler(InputParser inputParser) : base(inputParser)
    {
    }
}