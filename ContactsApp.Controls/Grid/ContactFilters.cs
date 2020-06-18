using System.ComponentModel;
using ContactsApp.Model;

namespace ContactsApp.Controls.Grid
{
    /// <summary>
    /// State of grid filters.
    /// </summary>
    public class ContactFilters : IContactFilters
    {
        /// <summary>
        /// Keep state of paging.
        /// </summary>
        public IPageHelper PageHelper { get; set; }

        /// <summary>
        /// Default: take scoped instance of page helper
        /// </summary>
        /// <param name="pageHelper">The <see cref="IPageHelper"/> instance.</param>
        public ContactFilters(IPageHelper pageHelper)
        {
            PageHelper = pageHelper;
        }

        /// <summary>
        /// Avoid multiple concurrent requests.
        /// </summary>
        public bool Loading { get; set; }

        /// <summary>
        /// Firstname Lastname, or Lastname, Firstname.
        /// </summary>
        public bool ShowFirstNameFirst { get; set; }

        /// <summary>
        /// Column to sort by.
        /// </summary>
        public ContactFilterColumns SortColumn { get; set; } = ContactFilterColumns.Name;
        
        /// <summary>
        /// True when sorting ascending, otherwise sort descending.
        /// </summary>
        public bool SortAscending { get; set; }

        /// <summary>
        /// Column filtered text is against.
        /// </summary>
        public ContactFilterColumns FilterColumn { get; set; } = ContactFilterColumns.Name;

        /// <summary>
        /// Text to filter on.
        /// </summary>
        public string FilterText { get; set; }
    }
}
