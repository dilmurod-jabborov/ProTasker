namespace ProTasker.Domain.Extension;

public static class ToObjectHelper
{
    public static TDestination ToNewObjDest<TDestination>(this object source) where TDestination : new()
    {
        var dest = new TDestination();
        var destProps = typeof(TDestination).GetProperties();
        var srcProps = source.GetType().GetProperties();

        foreach (var destProp in destProps)
        {
            var srcProp = srcProps.FirstOrDefault(p => p.Name == destProp.Name &&
                                                  p.PropertyType == destProp.PropertyType);
            if (srcProp != null)
            {
                var value = srcProp.GetValue(source);
                destProp.SetValue(dest, value);
            }
        }

        return dest;
    }
}


