using System.Linq.Dynamic.Core;
using System.Reflection;
using System.Text;

namespace Api.Rest.Common;

public sealed class OrderByQueryStructureError() : Exception("Wrong format") { }

public static class OrderByAddons
{
    public static IQueryable<T> ApplySort<T>(IQueryable<T> entities, string orderByQueryString)
    {
        var orderParams = orderByQueryString
            .Trim()
            .Split(',', StringSplitOptions.RemoveEmptyEntries);
        var propertyInfos = typeof(T).GetProperties(BindingFlags.Public | BindingFlags.Instance);
        var orderQueryBuilder = new StringBuilder();

        foreach (var param in orderParams)
        {
            if (string.IsNullOrWhiteSpace(param))
            {
                continue;
            }

            var kvp = param.Split(" ", StringSplitOptions.RemoveEmptyEntries);
            var propertyFromQueryName = kvp[0];
            var objectProperty = propertyInfos.FirstOrDefault(pi =>
                pi.Name.Equals(propertyFromQueryName, StringComparison.InvariantCultureIgnoreCase)
            );

            if (objectProperty is null)
            {
                continue;
            }

            var sortingOrder = "ascending";
            switch (kvp.Length)
            {
                case > 2:
                    throw new OrderByQueryStructureError();
                case > 1 when kvp[1] == "asc":
                    break;
                case > 1 when kvp[1] == "desc":
                    sortingOrder = "descending";
                    break;
            }

            orderQueryBuilder.Append($"{objectProperty.Name} {sortingOrder}, ");
        }

        var orderQuery = orderQueryBuilder.ToString().TrimEnd(',', ' ');
        return entities.OrderBy(orderQuery);
    }
}
