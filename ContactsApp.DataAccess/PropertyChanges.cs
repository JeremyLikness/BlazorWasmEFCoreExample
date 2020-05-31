using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;

namespace ContactsApp.DataAccess
{
    /// <summary>
    /// Class facilitates serialization of changes for audits.
    /// </summary>
    /// <typeparam name="TEntity">The <see cref="TEntity"/> type to audit.</typeparam>
    public class PropertyChanges<TEntity> where TEntity : class
    {
        /// <summary>
        /// The type for serialization.
        /// </summary>
        public string Type => typeof(TEntity).FullName;

        /// <summary>
        /// Original entity.
        /// </summary>
        public TEntity Original { get; set; }

        /// <summary>
        /// Changed entity.
        /// </summary>
        public TEntity New { get; set; }

        /// <summary>
        /// Parses the tracker to build the snapshots.
        /// </summary>
        /// <param name="entityEntry">The tracked entity.</param>
        /// <param name="dbVal">The database snapshot.</param>
        public PropertyChanges(EntityEntry<TEntity> entityEntry,
            TEntity dbVal = default)
        {
            if (entityEntry.State == EntityState.Added ||
                entityEntry.State == EntityState.Modified)
            {
                New = entityEntry.Entity;
            }
            if (entityEntry.State == EntityState.Deleted ||
                entityEntry.State == EntityState.Modified)
            {
                Original = dbVal ??
                    entityEntry.OriginalValues.ToObject() as TEntity;
            }            
        }
    }
}
