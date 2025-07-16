using Telegram.Bot.Types.ReplyMarkups;
using Telegram.Bot.Types;
using Telegram.Bot;

namespace ProTasker.Domain.Extension;

public static class UpdateHelper
{
    public static List<TTarget> UpdateByObj<TSource, TTarget>(
    this TSource updatedObj,
    List<TTarget> list,
    string filePath,
    string phoneNumber)
    where TSource : class
    where TTarget : class, new()
    {
        var index = list.FindIndex(x =>
            x.GetType().GetProperty("PhoneNumber")?.GetValue(x)?.ToString() == phoneNumber);

        if (index == -1)
            throw new Exception("Object not found by phone number!");

        var existingTarget = list[index];

        var sourceProps = typeof(TSource).GetProperties();
        var targetProps = typeof(TTarget).GetProperties();

        foreach (var sProp in sourceProps)
        {
            var tProp = targetProps.FirstOrDefault(p => p.Name == sProp.Name);
            if (tProp != null)
            {
                var value = sProp.GetValue(updatedObj);
                if (value != null)
                {
                    tProp.SetValue(existingTarget, value);
                }
            }
        }

        list[index] = existingTarget;
        return list;
    }
}


