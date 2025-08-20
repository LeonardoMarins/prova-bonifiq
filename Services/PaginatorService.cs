using Microsoft.EntityFrameworkCore;
using ProvaPub.Models;

namespace ProvaPub.Services;

public class PaginatorService
{
    public async Task<PaginatedResult<T>> PaginateAsync<T>(
        IQueryable<T> query, int page, int pageSize = 10) where T : class
    {
        if (page < 1) page = 1;

        var totalCount = await query.CountAsync();
        var items = await query
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        return new PaginatedResult<T>
        {
            Items = items,
            TotalCount = totalCount,
            HasNext = page * pageSize < totalCount
        };
    }
}