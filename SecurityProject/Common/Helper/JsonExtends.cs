using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Common.Helper
{
    /// <summary>
    /// Json扩展方法
    /// </summary>
    public static class JsonExtends
    {
        public static T ToEntity<T>(this string val)
        {
            return JsonConvert.DeserializeObject<T>(val);
        }

        public static string ToJson<T>(this T entity, Formatting formatting = Formatting.None)
        {
            return JsonConvert.SerializeObject(entity, formatting);
        }
    }
}
