namespace ProTasker.Domain.Extension;

public static class ListToTextExtension
{
    private static string ToTextWriteLine<T>(this T obj)
    {
        var values = typeof(T).GetProperties()
                              .Select(prop => prop.GetValue(obj)?.ToString() ?? "")
                              .ToArray();

        return string.Join(",", values);
    }

    public static List<string> ToTextWriteLines<T>(this List<T> list)
    {
        return list.Select(item => item.ToTextWriteLine()).ToList();
    }
}
