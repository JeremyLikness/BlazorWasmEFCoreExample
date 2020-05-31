using ContactsApp.Model;
using System;
using System.ComponentModel.DataAnnotations.Schema;

namespace ContactsApp.DataAccess
{
    /// <summary>
    /// Audit for <see cref="Contact"/>.
    /// </summary>
    public class ContactAudit
    {
        public int Id { get; set; }
        public DateTimeOffset EventTime { get; set; }
            = DateTimeOffset.UtcNow;
        public int ContactId { get; set; }
        public string User { get; set; }
        public string Action { get; set; }
        public string Changes { get; set; }

        [NotMapped]
        public Contact ContactRef { get; set; }
    }
}
