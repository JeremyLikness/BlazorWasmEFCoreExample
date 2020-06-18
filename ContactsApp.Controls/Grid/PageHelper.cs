using ContactsApp.Model;

namespace ContactsApp.Controls.Grid
{
    /// <summary>
    /// Because math is hard. Holds the state for paging.
    /// </summary>
    public class PageHelper : IPageHelper
    {
        /// <summary>
        /// Items on a page.
        /// </summary>
        public int PageSize { get; set; } = 20;

        /// <summary>
        /// Current page, 1-based.
        /// </summary>
        public int Page { get; set; }

        /// <summary>
        /// Total items across all pages.
        /// </summary>
        public int TotalItemCount { get; set; }

        /// <summary>
        /// Items on the current page (should be less than or equal to
        /// <see cref="PageSize"/>).
        /// </summary>
        public int PageItems { get; set; }

        /// <summary>
        /// Current page, 0-based.
        /// </summary>
        public int DbPage => Page - 1;

        /// <summary>
        /// How many records to skip to start current page.
        /// </summary>
        public int Skip => PageSize * DbPage;

        /// <summary>
        /// Total number of pages.
        /// </summary>
        public int PageCount => (TotalItemCount + PageSize - 1) / PageSize;

        /// <summary>
        /// Next page
        /// </summary>
        public int NextPage => Page < PageCount ? Page + 1 : Page;

        /// <summary>
        /// <c>true</c> when paging forward makes sense.
        /// </summary>
        public bool HasNext => Page < PageCount;

        /// <summary>
        /// The previous page.
        /// </summary>
        public int PrevPage => Page > 1 ? Page - 1 : Page;

        /// <summary>
        /// <c>true</c> when a previous page exists.
        /// </summary>
        public bool HasPrev => Page > 1;
    }
}
