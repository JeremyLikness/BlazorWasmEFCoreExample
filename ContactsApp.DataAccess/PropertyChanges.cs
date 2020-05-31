using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using System;
using System.Collections.Generic;

namespace ContactsApp.DataAccess
{
    public class PropertyChanges<TEntity> where TEntity: class
    {
        public List<PropertyChange> PropertiesChanged { get; set; }
            = new List<PropertyChange>();

        private bool HasChanges(IComparable oldValue, IComparable newValue)
        {
            if (oldValue == null && newValue == null)
            {
                return false;
            }
            if (oldValue == null || newValue == null)
            {
                return true;
            }
            return !oldValue.Equals(newValue);
        }

        public PropertyChanges(EntityEntry<TEntity> entityEntry)
        {
            foreach(var prop in entityEntry.OriginalValues.Properties)
            {
                if (!(prop is IComparable))
                {
                    continue;
                }
                var propertyChange = new PropertyChange
                {
                    PropertyName = prop.Name
                };
                var oldProp = PropertyToString(entityEntry.OriginalValues[prop.Name]);
                var newProp = PropertyToString(entityEntry.CurrentValues[prop.Name]);
                if (entityEntry.State == EntityState.Added)
                {
                    propertyChange.NewValue = newProp;
                    PropertiesChanged.Add(propertyChange);
                    continue;
                }
                if (entityEntry.State == EntityState.Deleted)
                {
                    propertyChange.OldValue = oldProp;
                    PropertiesChanged.Add(propertyChange);
                    continue;
                }
                IComparable oldValue = (IComparable)entityEntry.OriginalValues[prop.Name];
                IComparable newValue = (IComparable)entityEntry.CurrentValues[prop.Name];
                if (HasChanges(oldValue, newValue))
                {
                    PropertiesChanged.Add(new PropertyChange
                    {
                        PropertyName = prop.Name,
                        OldValue = oldProp,
                        NewValue = newProp
                    });
                }
            }
        }

        private string PropertyToString(object value)
        {
            return value == null ? "<null>" : value.ToString();
        }
    }
}
