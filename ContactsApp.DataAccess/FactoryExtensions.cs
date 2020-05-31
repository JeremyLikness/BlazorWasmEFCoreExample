using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace ContactsApp.DataAccess
{
    /// <summary>
    /// Make it easier to register the factory
    /// </summary>
    public static class FactoryExtensions
    {
        /// <summary>
        /// Registers the <seealso cref="DbContextFactory{TContext}"/> with DI.
        /// </summary>
        /// <typeparam name="TContext">The instance of <see cref="DbContext"/> to register.</typeparam>
        /// <param name="collection">The instance of the <see cref="IServiceCollection"/>.</param>
        /// <param name="optionsAction">Optional access to the <see cref="DbContextOptions{TContext}"/>.</param>
        /// <returns>The registered <see cref="IServiceCollection"/>.</returns>
        public static IServiceCollection AddDbContextFactory<TContext>(
            this IServiceCollection collection,
            Action<DbContextOptionsBuilder> optionsAction = null)
            where TContext : DbContext
        {
            var optionsBuilder = new DbContextOptionsBuilder<TContext>();
            optionsAction?.Invoke(optionsBuilder);
            collection.AddSingleton(optionsBuilder.Options);
            collection.AddSingleton<DbContextFactory<TContext>>();
            return collection;
        }
    }
}
