using ContactsApp.Controls.Grid;
using ContactsApp.Model;

namespace ContactsApp.Server.Controllers
{
    /// <summary>
    /// Simple implementation of <see cref="IContactFilters"/> for
    /// serialization across REST endpoints.
    /// </summary>
    public class ContactFilter : IContactFilters
    {
        /// <summary>
        /// Initializes an instance of the <see cref="ContactFilter"/> class.
        /// </summary>
        public ContactFilter()
        {
            PageHelper = new PageHelper();
        }

        /// <summary>
        /// The <see cref="ContactFilterColumns"/> being filtered on.
        /// </summary>
        public ContactFilterColumns FilterColumn { get; set; }

        /// <summary>
        /// The text of the filter.
        /// </summary>
        public string FilterText { get; set; }

        /// <summary>
        /// Loading indicator.
        /// </summary>
        public bool Loading { get; set; }

        /// <summary>
        /// Paging state.
        /// </summary>
        public PageHelper PageHelper { get; set; }
      
        /// <summary>
        /// Gets or sets a value indicating if the name is rendered first name first.
        /// </summary>
        public bool ShowFirstNameFirst { get; set; }

        /// <summary>
        /// Gets or sets a value indicating if the sort is ascending or descending.
        /// </summary>
        public bool SortAscending { get; set; }

        /// <summary>
        /// The <see cref="ContactFilterColumns"/> being sorted.
        /// </summary>
        public ContactFilterColumns SortColumn { get; set; }

        /// <summary>
        /// To satisfy the contract.
        /// </summary>
        IPageHelper IContactFilters.PageHelper { get => PageHelper; set => throw new System.NotImplementedException(); }
    }
}
