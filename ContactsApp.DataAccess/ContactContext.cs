using ContactsApp.Model;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.Serialization.Json;
using System.Security.Claims;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;

namespace ContactsApp.DataAccess
{
    /// <summary>
    /// Context for the contacts database.
    /// </summary>
    public class ContactContext : DbContext
    {
        /// <summary>
        /// Tracking lifetime of contexts.
        /// </summary>
        private readonly Guid _id;

        /// <summary>
        /// The logged in <see cref="ClaimsPrincipal"/>.
        /// </summary>
        public ClaimsPrincipal User { get; set; }

        /// <summary>
        /// Magic string.
        /// </summary>
        public static readonly string RowVersion = nameof(RowVersion);

        /// <summary>
        /// Who created it?
        /// </summary>
        public static readonly string CreatedBy = nameof(CreatedBy);

        /// <summary>
        /// When was it created?
        /// </summary>
        public static readonly string CreatedOn = nameof(CreatedOn);

        /// <summary>
        /// Who last modified it?
        /// </summary>
        public static readonly string ModifiedBy = nameof(ModifiedBy);

        /// <summary>
        /// When was it last modified?
        /// </summary>
        public static readonly string ModifiedOn = nameof(ModifiedOn);

        /// <summary>
        /// Magic strings.
        /// </summary>
        public static readonly string BlazorContactsDb =
            nameof(BlazorContactsDb).ToLower();

        /// <summary>
        /// Inject options.
        /// </summary>
        /// <param name="options">The <see cref="DbContextOptions{ContactContext}"/>
        /// for the context
        /// </param>
        public ContactContext(DbContextOptions<ContactContext> options)
            : base(options)
        {
            _id = Guid.NewGuid();
            Debug.WriteLine($"{_id} context created.");
        }

        public override async Task<int> SaveChangesAsync(CancellationToken token
            = default)
        {
            var user = "Unknown";

            if (User != null)
            {
                var name = User.Claims.FirstOrDefault(
                    c => c.Type == ClaimTypes.NameIdentifier);
            
                if (name != null)
                {
                    user = name.Value;
                }
            }

            var audits = new List<ContactAudit>();

            // audit contacts
            foreach (var item in ChangeTracker.Entries<Contact>())
            {
                if (item.State == EntityState.Modified ||
                    item.State == EntityState.Added ||
                    item.State == EntityState.Deleted)
                {
                    if (item.State == EntityState.Added)
                    {
                        item.Property<string>(CreatedBy).CurrentValue =
                            user;
                        item.Property<DateTimeOffset>(CreatedOn).CurrentValue =
                            DateTimeOffset.UtcNow;
                    }

                    if (item.State == EntityState.Modified)
                    {
                        item.Property<string>(ModifiedBy).CurrentValue =
                            user;
                        item.Property<DateTimeOffset>(ModifiedOn).CurrentValue =
                          DateTimeOffset.UtcNow;
                    }
                    var changes = new PropertyChanges<Contact>(item);
                    var audit = new ContactAudit
                    {
                        ContactId = item.Entity.Id,
                        Action = item.State.ToString(),
                        User = user,
                        Changes = JsonSerializer.Serialize(changes),
                        ContactRef = item.Entity
                    };

                    audits.Add(audit);
                }
            }
            
            if (audits.Count > 0)
            {
                ContactAudits.AddRange(audits);
            }
            
            var result = await base.SaveChangesAsync(token);

            var secondSave = false;
            
            // attach ids for add operations
            foreach(var audit in audits.Where(a => a.ContactId == 0).ToList())
            {
                secondSave = true;
                audit.ContactId = audit.ContactRef.Id;
                Entry(audit).State = EntityState.Modified;
            }

            if (secondSave)
            {
                await base.SaveChangesAsync(token);
            }

            return result;
        }

        /// <summary>
        /// List of <see cref="Contact"/>.
        /// </summary>
        public DbSet<Contact> Contacts { get; set; }
        public DbSet<ContactAudit> ContactAudits { get; set; }

        /// <summary>
        /// Define the model.
        /// </summary>
        /// <param name="modelBuilder">The <see cref="ModelBuilder"/>.</param>
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            var contact = modelBuilder.Entity<Contact>();

            // this property isn't on the C# class
            // so we set it up as a "shadow" property and use it for concurrency
            contact.Property<byte[]>(RowVersion).IsRowVersion();

            // audit fields
            contact.Property<string>(ModifiedBy);
            contact.Property<DateTimeOffset>(ModifiedOn);
            contact.Property<string>(CreatedBy);
            contact.Property<DateTimeOffset>(CreatedOn);

            base.OnModelCreating(modelBuilder);
        }

        /// <summary>
        /// Dispose pattern.
        /// </summary>
        public override void Dispose()
        {
            Debug.WriteLine($"{_id} context disposed.");
            base.Dispose();
        }

        /// <summary>
        /// Dispose pattern.
        /// </summary>
        /// <returns>A <see cref="ValueTask"/></returns>
        public override ValueTask DisposeAsync()
        {
            Debug.WriteLine($"{_id} context disposed async.");
            return base.DisposeAsync();
        }
    }
}
