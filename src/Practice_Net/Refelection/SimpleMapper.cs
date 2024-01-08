using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Reflection;

namespace Refelection
{

    public class SimpleMapper
    {
        public void Copy<T>(T source, T destination)
        {
            if (source == null || destination == null)
            {
                throw new ArgumentNullException("Source and destination objects must not be null.");
            }

            Type sourceType = source.GetType();
            Type destinationType = destination.GetType();

            if (sourceType != destinationType)
            {
                throw new ArgumentException("Source and destination objects must be of the same type.");
            }

            PropertyInfo[] properties = sourceType.GetProperties();

            foreach (PropertyInfo property in properties)
            {
                if (property.CanRead && property.CanWrite)
                {
                    object sourceValue = property.GetValue(source);
                    object destinationValue = property.GetValue(destination);

                    if (IsSimpleType(property.PropertyType))
                    {
                        if (sourceValue != null)
                        {
                            property.SetValue(destination, sourceValue);
                        }
                    }
                    else if (IsListType(property.PropertyType))
                    {
                        // Handle lists of objects
                        if (sourceValue != null && destinationValue != null)
                        {
                            IList<object> sourceList = (IList<object>)sourceValue;
                            IList<object> destinationList = (IList<object>)destinationValue;

                            destinationList.Clear();

                            foreach (object item in sourceList)
                            {
                                object newItem = Activator.CreateInstance(item.GetType());
                                Copy(item, newItem);
                                destinationList.Add(newItem);
                            }
                        }
                    }
                    else
                    {
                        // Handle nested objects
                        if (sourceValue != null && destinationValue != null)
                        {
                            Copy(sourceValue, destinationValue);
                        }
                    }
                }
            }
        }

        private bool IsSimpleType(Type type)
        {
            return type.IsValueType || type == typeof(string) || type == typeof(decimal);
        }

        private bool IsListType(Type type)
        {
            return type.IsGenericType && type.GetGenericTypeDefinition() == typeof(List<>);
        }
    }

}
