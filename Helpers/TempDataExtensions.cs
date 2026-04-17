using Microsoft.AspNetCore.Mvc.ViewFeatures;
using System.Text.Json;

namespace Catalogo_sis.Helpers
{
    public static class TempDataExtensions
    {
        public static void Set<T>(this ITempDataDictionary tempData, string key, T value)
        {
            tempData[key] = JsonSerializer.Serialize(value);
        }

        public static T? Get<T>(this ITempDataDictionary tempData, string key)
        {
            if (tempData[key] == null) return default;
            return JsonSerializer.Deserialize<T>(tempData[key]!.ToString()!);
        }
    }
}
