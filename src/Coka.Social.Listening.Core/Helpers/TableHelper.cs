using System.Reflection;
using Coka.Social.Listening.Core.Attributes;

namespace Coka.Social.Listening.Core.Helpers;

public static class TableHelper
{
    public static string GetTableName<T>()
    {
        var type = typeof(T);
        var attribute = type.GetCustomAttribute<TableAttribute>();
        if (attribute != null && !string.IsNullOrWhiteSpace(attribute.Name))
        {
            return attribute.Name;
        }

        return type.Name;
    }
}
