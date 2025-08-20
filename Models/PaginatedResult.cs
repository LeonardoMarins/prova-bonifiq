namespace ProvaPub.Models;

public class PaginatedResult<T>
{
    public List<T> Items { get; set; } = new List<T>();
    public int TotalCount { get; set; }
    public bool HasNext { get; set; }
}