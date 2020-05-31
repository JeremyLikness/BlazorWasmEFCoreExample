using System;

namespace ContactsApp.BaseRepository
{
    /// <summary>
    /// Simplify concurrency issues.
    /// </summary>
    /// <typeparam name="TEntity">The entity type the exception is for.</typeparam>
    public class RepoConcurrencyException<TEntity> : Exception
    {
        /// <summary>
        /// The <see cref="TEntity"/> to update.
        /// </summary>
        public TEntity Entity { get; private set; }

        /// <summary>
        /// The changed <see cref="TEntity"/> in the database.
        /// </summary>
        public TEntity DbEntity { get; set; }

        /// <summary>
        /// The database row version.
        /// </summary>
        public byte[] RowVersion { get; set; } = null;

        /// <summary>
        /// Initializes a new instance of the <see cref="RepoConcurrencyException"/>
        /// class.
        /// </summary>
        /// <param name="entity">The <see cref="TEntity"/> instance with a conflict.</param>
        /// <param name="ex">The exception that was thrown.</param>
        public RepoConcurrencyException(TEntity entity, Exception ex) :
            base("A concurrency issue was detected", ex)
        {
            Entity = entity;
        }
    }
}
