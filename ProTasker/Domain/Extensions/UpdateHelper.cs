namespace ProTasker.Domain.Extension;

public static class UpdateHelper
{
    public static List<TTarget> UpdateByObj<TSource, TTarget>(
        this TSource updatedObj,
        List<TTarget> list,
        string filePath,
        int id)
        where TSource : class
        where TTarget : class, new()
    {
        var index = list.FindIndex(x =>
            x.GetType().GetProperty("Id")?.GetValue(x)?.ToString() == id.ToString());

        if (index == -1)
            throw new Exception("Obyekt not found!");

        var updatedTarget = new TTarget();

        var sourceProps = typeof(TSource).GetProperties();
        var targetProps = typeof(TTarget).GetProperties();

        foreach (var tProp in targetProps)
        {
            var sProp = sourceProps.FirstOrDefault(p => p.Name == tProp.Name);

            if (sProp != null)
            {
                var value = sProp.GetValue(updatedObj);
                tProp.SetValue(updatedTarget, value);
            }
        }

        var idProp = targetProps.FirstOrDefault(p => p.Name == "Id");
        if (idProp != null)
        {
            idProp.SetValue(updatedTarget, id);
        }

        list[index] = updatedTarget;

        return list;
    }
}


