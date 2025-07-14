namespace ProTasker.Domain.Extension;

public static class ToObjectHelper
{
    public static T ToNewObjDest<T>(this object model) where T : new()
    {
        T result = new T();

        var modelProps = model.GetType().GetProperties();
        var resultProps = typeof(T).GetProperties();

        foreach (var prop in resultProps)
        {
            var modelProp = modelProps.FirstOrDefault(p => p.Name == prop.Name);
            if (modelProp != null)
            {
                var value = modelProp.GetValue(model);
                prop.SetValue(result, value);
            }
        }

        return result;
    }
}