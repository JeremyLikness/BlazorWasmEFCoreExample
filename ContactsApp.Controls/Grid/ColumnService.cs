using ContactsApp.Model;
using System.Collections.Generic;

namespace ContactsApp.Controls.Grid
{
    /// <summary>
    /// Provides class attributes for columns
    /// </summary>
    public class ColumnService
    {
        /// <summary>
        /// Map columns
        /// </summary>
        private readonly Dictionary<ContactFilterColumns, string> _columnMappings =
            new Dictionary<ContactFilterColumns, string>
            {
                { ContactFilterColumns.City, "d-none d-sm-block col-lg-1 col-sm-3"},
                { ContactFilterColumns.Name, "col-8 col-lg-2 col-sm-3" },
                { ContactFilterColumns.Phone, "d-none d-sm-block col-lg-2 col-sm-2" },
                { ContactFilterColumns.State, "d-none d-sm-block col-sm-1" },
                { ContactFilterColumns.Street, "d-none d-lg-block col-lg-3" },
                { ContactFilterColumns.ZipCode, "d-none d-sm-block col-sm-2" }
            }; // 2 2 1 1 2 1

        /// <summary>
        /// Left edit column.
        /// </summary>
        public string EditColumn => "col-4 col-sm-1";

        /// <summary>
        /// Delete confirmation column.
        /// </summary>
        public string DeleteConfirmation => "col-lg-9 col-sm-8";

        /// <summary>
        /// Get attributes for column
        /// </summary>
        /// <param name="column">The <see cref="ContactFilterColumns"/> to reference.</param>
        /// <returns>A <see cref="string"/> representing the classes.</returns>
        public string GetClassForColumn(ContactFilterColumns column) => _columnMappings[column];
    }
}
