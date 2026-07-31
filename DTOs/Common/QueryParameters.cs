namespace Drivious.DTOs.Common
{
    /// <summary>
    /// Paging, search and sort options shared by every list endpoint. The bounds are
    /// enforced in the setters rather than by a validator, so a query string that
    /// asks for page 0 or ten thousand rows is corrected instead of rejected.
    /// </summary>
    public class QueryParameters
    {
        public const int MaxPageSize = 100;

        private int _page = 1;
        private int _pageSize = 20;

        /// <summary>1-based page number.</summary>
        public int Page
        {
            get => _page;
            set => _page = value < 1 ? 1 : value;
        }

        /// <summary>Capped so no single request can pull the whole table.</summary>
        public int PageSize
        {
            get => _pageSize;
            set => _pageSize = value switch
            {
                < 1 => 1,
                > MaxPageSize => MaxPageSize,
                _ => value
            };
        }

        /// <summary>
        /// Free text match. Each endpoint decides which of its columns this covers.
        /// </summary>
        public string? Search { get; set; }

        /// <summary>
        /// Field to order by. Names are matched case insensitively against the
        /// endpoint's own list; anything unknown falls back to its default order.
        /// </summary>
        public string? SortBy { get; set; }

        public bool Descending { get; set; }
    }
}
