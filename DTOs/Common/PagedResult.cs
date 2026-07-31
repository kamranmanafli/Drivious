namespace Drivious.DTOs.Common
{
    /// <summary>
    /// One page of a list endpoint's result, together with the counters a client
    /// needs to draw paging controls without issuing a second request.
    /// </summary>
    public class PagedResult<T>
    {
        public List<T> Items { get; set; } = new();

        public int Page { get; set; }

        public int PageSize { get; set; }

        public int TotalCount { get; set; }

        public int TotalPages { get; set; }

        public bool HasPrevious => Page > 1;

        public bool HasNext => Page < TotalPages;

        public PagedResult() { }

        public PagedResult(List<T> items, int totalCount, int page, int pageSize)
        {
            Items = items;
            TotalCount = totalCount;
            Page = page;
            PageSize = pageSize;
            TotalPages = pageSize > 0 ? (int)Math.Ceiling(totalCount / (double)pageSize) : 0;
        }
    }
}
