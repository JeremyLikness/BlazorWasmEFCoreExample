using System.ComponentModel;
using ContactsApp.Model;

namespace ContactsApp.Controls.Grid
{
    /// <summary>
    /// State of grid filters.
    /// </summary>
    public class GridControls : IContactFilters
    {
        private bool _showFirstNameFirst;
        private ContactFilterColumns _sortColumn = ContactFilterColumns.Name;
        private ContactFilterColumns _filterColumn = ContactFilterColumns.Name;
        private string _filterText;
        private bool _sortAscending = true;
        private bool _loading;

        /// <summary>
        /// Keep state of paging.
        /// </summary>
        public IPageHelper PageHelper { get; set; } = new PageHelper();

        /// <summary>
        /// Handler for grid updates.
        /// </summary>
        /// <param name="sender">The instance that changed.</param>
        /// <param name="e">The property that changed.</param>
        public delegate void GridUpdateHandler(object sender, PropertyChangedEventArgs e);

        /// <summary>
        /// The <see cref="GridUpdateHandler"/> event.
        /// </summary>
        public event GridUpdateHandler OnGridControlsChanged;

        /// <summary>
        /// Avoid multiple concurrent requests.
        /// </summary>
        public bool Loading
        {
            get => _loading;
            set
            {
                if (_loading != value)
                {
                    _loading = value;
                    GridControlsChanged(nameof(Loading));
                }
            }
        }

        /// <summary>
        /// Firstname Lastname, or Lastname, Firstname.
        /// </summary>
        public bool ShowFirstNameFirst
        {
            get => _showFirstNameFirst;
            set
            {
                if (_showFirstNameFirst != value)
                {
                    _showFirstNameFirst = value;
                    GridControlsChanged(nameof(ShowFirstNameFirst));
                }
            }
        }

        /// <summary>
        /// Column to sort by.
        /// </summary>
        public ContactFilterColumns SortColumn
        {
            get => _sortColumn;
            set
            {
                if (_sortColumn != value)
                {
                    _sortColumn = value;
                    _sortAscending = true;
                    GridControlsChanged(nameof(SortColumn));
                    GridControlsChanged(nameof(SortAscending));
                }
            }
        }

        /// <summary>
        /// True when sorting ascending, otherwise sort descending.
        /// </summary>
        public bool SortAscending
        {
            get => _sortAscending;
            set
            {
                if (_sortAscending != value)
                {
                    _sortAscending = value;
                    GridControlsChanged(nameof(SortAscending));
                }
            }
        }

        /// <summary>
        /// Column filtered text is against.
        /// </summary>
        public ContactFilterColumns FilterColumn
        {
            get => _filterColumn;
            set
            {
                if (_filterColumn != value)
                {
                    _filterColumn = value;
                    _filterText = string.Empty;
                    GridControlsChanged(nameof(FilterColumn));
                }
            }
        }

        /// <summary>
        /// Text to filter on.
        /// </summary>
        public string FilterText
        {
            get => _filterText;
            set
            {
                if (_filterText != value)
                {
                    _filterText = value;
                    GridControlsChanged(nameof(FilterText));
                }
            }
        }

        /// <summary>
        /// Raise <see cref="OnGridControlsChanged"/> that something changed.
        /// </summary>
        /// <param name="property">Name of the property that changed.</param>
        private void GridControlsChanged(string property)
        {
            // avoid firing multiple refreshes if in the middle of a sync
            if (!Loading)
            {
                OnGridControlsChanged?.Invoke(this, new PropertyChangedEventArgs(property));
            }
        }
    }
}
