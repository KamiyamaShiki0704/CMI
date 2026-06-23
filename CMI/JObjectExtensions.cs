using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;

public static class JObjectExtensions
{
    public static void InsertAt(this JObject obj, int index, JObject newPropertyObj)
    {
        List<JProperty> properties = obj.Properties().ToList();
        if (index < 0 || index > properties.Count)
            throw new ArgumentOutOfRangeException(nameof(index), "Index out of bounds for JObject properties.");
        if (newPropertyObj.Count != 1)
            throw new ArgumentException("newPropertyObj must contain exactly one property.", nameof(newPropertyObj));
        JProperty newProp = newPropertyObj.Properties().First();
        obj.RemoveAll();
        for (int i = 0; i <= properties.Count; i++)
        {
            if (i == index)
            {
                obj.Add(newProp.Name, newProp.Value);
            }
            if (i < properties.Count)
            {
                obj.Add(properties[i]);
            }
        }
    }

    public static int IndexOf(this JObject obj, string propertyName)
    {
        int index = 0;
        foreach (JProperty prop in obj.Properties())
        {
            if (prop.Name == propertyName)
                return index;
            index++;
        }
        return -1;
    }
}