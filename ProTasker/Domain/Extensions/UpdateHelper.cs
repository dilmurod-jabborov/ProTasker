namespace ProTasker.Domain.Extension;

public static class UpdateHelper
{
    public static List<string> UpdateByObj<T>(this T updatedObj, List<T> list, string filePath, int id) where T : new()
    {
        var exists = list.FindIndex(x => x.GetType().GetProperty("Id")?.GetValue(x).ToString() == id.ToString());
        if (exists == -1)
            throw new Exception("This object is not found!!!");

        list[exists] = updatedObj;

        return list.ToTextWriteLines<T>();
    }
}


