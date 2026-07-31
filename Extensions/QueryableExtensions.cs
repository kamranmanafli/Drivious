using AutoMapper.QueryableExtensions;
using Drivious.DTOs.Common;
using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;

namespace Drivious.Extensions
{
    public static class QueryableExtensions
    {
        /// <summary>
        /// Orders by one of the fields the endpoint declares as sortable. Sorting by
        /// an arbitrary caller-supplied name would mean building expressions from
        /// user input, so unknown names quietly fall back to the default instead.
        /// </summary>
        public static IQueryable<T> ApplySort<T>(
            this IQueryable<T> query,
            QueryParameters parameters,
            IReadOnlyDictionary<string, Expression<Func<T, object?>>> sortable,
            string defaultField)
        {
            var field = !string.IsNullOrWhiteSpace(parameters.SortBy)
                        && sortable.ContainsKey(parameters.SortBy)
                ? parameters.SortBy
                : defaultField;

            var selector = sortable[field];

            return parameters.Descending
                ? query.OrderByDescending(selector)
                : query.OrderBy(selector);
        }

        /// <summary>
        /// Counts the whole filtered set, then materialises only the requested page.
        /// Projection happens in the database, so a list of expenses does not drag
        /// its vehicles' rows across the wire just to show a plate number.
        /// </summary>
        public static async Task<PagedResult<TDto>> ToPagedResultAsync<TEntity, TDto>(
            this IQueryable<TEntity> query,
            QueryParameters parameters,
            AutoMapper.IConfigurationProvider configuration)
        {
            var totalCount = await query.CountAsync();

            var items = await query
                .Skip((parameters.Page - 1) * parameters.PageSize)
                .Take(parameters.PageSize)
                .ProjectTo<TDto>(configuration)
                .ToListAsync();

            return new PagedResult<TDto>(items, totalCount, parameters.Page, parameters.PageSize);
        }

        /// <summary>Applies a filter only when the caller actually supplied a value.</summary>
        public static IQueryable<T> WhereIf<T>(
            this IQueryable<T> query,
            bool condition,
            Expression<Func<T, bool>> predicate)
        {
            return condition ? query.Where(predicate) : query;
        }
    }
}
