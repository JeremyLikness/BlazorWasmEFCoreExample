namespace ContactsApp.Model
{
    /// <summary>
    /// To keep these consistent.
    /// </summary>
    public interface IPageHelper
    {
        /// <summary>
        /// Current page, 0-based.
        /// </summary>
        int DbPage { get; }

        /// <summary>
        /// Current page, 1-based.
        /// </summary>
        int Page { get; set; }

        /// <summary>
        /// Total page count.
        /// </summary>
        int PageCount { get; }

        /// <summary>
        /// The next page.
        /// </summary>
        int NextPage { get; }

        /// <summary>
        /// <c>true</c> when next page exists.
        /// </summary>
        bool HasNext { get;  }

        /// <summary>
        /// The previous page
        /// </summary>
        int PrevPage { get; }

        /// <summary>
        /// <c>true</c> when previous page exists.
        /// </summary>
        bool HasPrev { get; }

        /// <summary>
        /// Items on current page.
        /// </summary>
        int PageItems { get; set; }

        /// <summary>
        /// Items per page.
        /// </summary>
        int PageSize { get; set; }

        /// <summary>
        /// How many items to skip.
        /// </summary>
        int Skip { get; }

        /// <summary>
        /// Total items based on filter.
        /// </summary>
        int TotalItemCount { get; set; }
    }
}
