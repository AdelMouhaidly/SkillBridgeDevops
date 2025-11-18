using System.Linq.Expressions;

namespace SkillBridge.API.Extensions;

public static class QueryableExtensions
{
    public static IQueryable<T> ApplyOrdering<T>(this IQueryable<T> query, string? orderBy, string sortDirection)
    {
        if (string.IsNullOrWhiteSpace(orderBy))
        {
            return query;
        }

        var property = typeof(T).GetProperties()
            .FirstOrDefault(p => p.Name.Equals(orderBy, StringComparison.OrdinalIgnoreCase));

        if (property is null)
        {
            return query;
        }

        var parameter = Expression.Parameter(typeof(T), "x");
        var propertyAccess = Expression.MakeMemberAccess(parameter, property);
        var orderByExpression = Expression.Lambda(propertyAccess, parameter);
        var methodName = string.Equals(sortDirection, "desc", StringComparison.OrdinalIgnoreCase)
            ? nameof(Queryable.OrderByDescending)
            : nameof(Queryable.OrderBy);

        var resultExpression = Expression.Call(
            typeof(Queryable),
            methodName,
            new[] { typeof(T), property.PropertyType },
            query.Expression,
            Expression.Quote(orderByExpression));

        return query.Provider.CreateQuery<T>(resultExpression);
    }

    public static IQueryable<T> ApplyPagination<T>(this IQueryable<T> query, int pageNumber, int pageSize)
    {
        var skip = (pageNumber - 1) * pageSize;
        return query.Skip(skip).Take(pageSize);
    }
}
