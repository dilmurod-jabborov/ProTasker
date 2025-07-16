using ProTasker.Domain.Models;

namespace ProTasker.Domain.Extension;

public static class ListToTextExtension
{
    public static List<string> ConvertToString<T>(this List<T> lists)
    {
        var convertedlist = new List<string>();

        foreach (var list in lists)
        {
            convertedlist.Add(list.ToString());
        }

        return convertedlist;
    }
}
