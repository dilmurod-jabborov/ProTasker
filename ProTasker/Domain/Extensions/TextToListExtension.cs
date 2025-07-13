using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ProTasker.Domain.Extension;

public static class TextToListExtension
{
    public static List<T> TextToObjectList<T>(this string text) where T : new()
    {
        List<T> list = new();

        string[] lines = text.Split('\n', StringSplitOptions.RemoveEmptyEntries);

        foreach (string line in lines)
        {
            string[] parts = line.Trim().Split(',');

            T obj = new T();
            var properties = typeof(T).GetProperties();

            for (int i = 0; i < properties.Length && i < parts.Length; i++)
            {
                var prop = properties[i];
                var valueStr = parts[i].Trim();

                try
                {
                    object? value = Convert.ChangeType(valueStr, prop.PropertyType);
                    prop.SetValue(obj, value);
                }
                catch
                {
                    // Ignor qilamiz, notog'ri type bo'lsa o'tkazib yuboramiz
                }
            }

            list.Add(obj);
        }

        return list;
    }
}


