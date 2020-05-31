using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using System;
using System.Collections.Generic;

namespace ContactsApp.DataAccess
{
    public class PropertyChanges<TEntity> where TEntity: class
    {
        public string Type => typeof(TEntity).AssemblyQualifiedName;
        public TEntity Original { get; set; }
        public TEntity New { get; set; }
        
        public PropertyChanges(EntityEntry<TEntity> entityEntry)
        {
            if (entityEntry.State == EntityState.Added ||
                entityEntry.State == EntityState.Modified)
            {
                New = entityEntry.Entity;
            }
            if (entityEntry.State == EntityState.Deleted ||
                entityEntry.State == EntityState.Modified)
            {
                Original = entityEntry.OriginalValues.ToObject() as TEntity;
            }
        }
    }
}
