using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

public class SimpleMapper
{
    public void Copy<TSource, TDestination>(TSource source, TDestination destination)
    {
        if (source == null || destination == null)
        {
            throw new ArgumentNullException("Source and destination objects must not be null.");
        }

        MapProperties(source, destination);
    }

    private void MapProperties(object source, object destination)
    {
        var sourceType = source.GetType();
        var destinationType = destination.GetType();

        var sourceProperties = sourceType.GetProperties();
        var destinationProperties = destinationType.GetProperties();

        foreach (var sourceProperty in sourceProperties)
        {
            var matchingDestinationProperty = destinationProperties.FirstOrDefault(
                dp => dp.Name == sourceProperty.Name && dp.PropertyType == sourceProperty.PropertyType);

            if (matchingDestinationProperty != null)
            {
                var sourceValue = sourceProperty.GetValue(source);
                if (sourceValue != null)
                {
                    if (IsListType(sourceProperty.PropertyType))
                    {
                        MapListProperty(sourceProperty, matchingDestinationProperty, source, destination);
                    }
                    else if (IsArrayType(sourceProperty.PropertyType))
                    {
                        MapArrayProperty(sourceProperty, matchingDestinationProperty, source, destination);
                    }
                    else if (!IsPrimitive(sourceProperty.PropertyType))
                    {
                        // Recursively map nested objects
                        var destinationNestedObject = matchingDestinationProperty.GetValue(destination);
                        if (destinationNestedObject == null)
                        {
                            destinationNestedObject = Activator.CreateInstance(matchingDestinationProperty.PropertyType);
                            matchingDestinationProperty.SetValue(destination, destinationNestedObject);
                        }
                        MapProperties(sourceValue, destinationNestedObject);
                    }
                    else
                    {
                        matchingDestinationProperty.SetValue(destination, sourceValue);
                    }
                }
            }
        }
    }

    private void MapListProperty(PropertyInfo sourceProperty, PropertyInfo destinationProperty, object source, object destination)
    {
        var sourceList = sourceProperty.GetValue(source) as IEnumerable;
        if (sourceList != null)
        {
            Type listType = sourceProperty.PropertyType;
            Type elementType = listType.GetGenericArguments()[0];

            if (elementType == typeof(string))
            {
                // For string lists, just copy the list as is.
                destinationProperty.SetValue(destination, sourceList);
            }
            else
            {
                var destinationList = (IList)Activator.CreateInstance(listType);

                foreach (var sourceItem in sourceList)
                {
                    if (sourceItem != null)
                    {
                        var destinationItem = Activator.CreateInstance(elementType);
                        MapProperties(sourceItem, destinationItem);
                        destinationList.Add(destinationItem);
                    }
                    else
                    {
                        // Handle the case where an element within the source list is null.
                        destinationList.Add(null);
                    }
                }

                destinationProperty.SetValue(destination, destinationList);
            }
        }
        else
        {
            // Handle the case where the source property is null.
            destinationProperty.SetValue(destination, null);
        }
    }

    private void MapArrayProperty(PropertyInfo sourceProperty, PropertyInfo destinationProperty, object source, object destination)
{
    var sourceArray = sourceProperty.GetValue(source) as Array;
    if (sourceArray != null)
    {
        Type arrayType = sourceProperty.PropertyType;
        Type elementType = arrayType.GetElementType();

        if (elementType == typeof(string))
        {
            // Handle string arrays
            var destinationArray = Array.CreateInstance(typeof(string), sourceArray.Length);
            for (int i = 0; i < sourceArray.Length; i++)
            {
                destinationArray.SetValue(sourceArray.GetValue(i), i);
            }
            destinationProperty.SetValue(destination, destinationArray);
        }
        else
        {
            // Handle other types of arrays
            var destinationArray = Array.CreateInstance(elementType, sourceArray.Length);
            for (int i = 0; i < sourceArray.Length; i++)
            {
                var sourceItem = sourceArray.GetValue(i);
                if (sourceItem != null)
                {
                    var destinationItem = Activator.CreateInstance(elementType);
                    MapProperties(sourceItem, destinationItem);
                    destinationArray.SetValue(destinationItem, i);
                }
                else
                {
                    destinationArray.SetValue(null, i);
                }
            }
            destinationProperty.SetValue(destination, destinationArray);
        }
    }
    else
    {
        // Handle the case where the source property is null.
        destinationProperty.SetValue(destination, null);
    }
    }

    private bool IsListType(Type type)
    {
        return type.IsGenericType && type.GetGenericTypeDefinition() == typeof(List<>);
    }

    private bool IsArrayType(Type type)
    {
        return type.IsArray;
    }

    private bool IsPrimitive(Type type)
    {
        return type.IsPrimitive || type == typeof(string) || type == typeof(DateTime) || type == typeof(decimal);
    }
}
