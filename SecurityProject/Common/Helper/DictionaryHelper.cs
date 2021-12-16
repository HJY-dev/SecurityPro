using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Common.Helper
{
    public class DictionaryHelper
    {
        /// <summary>
        /// 排序枚举
        /// </summary>
        public enum SortOrder { Normal, Ascending, Descending }

        /// <summary>
        /// 字典排序
        /// </summary>
        /// <param name="form"></param>
        /// <param name="sortOrder"></param>
        /// <param name="formatKeyFun"></param>
        /// <param name="formatDataFun"></param>
        /// <param name="spliter"></param>
        /// <returns></returns>
        public static string DictionaryToFormString(IDictionary<string, object> form, SortOrder sortOrder = SortOrder.Normal, Func<string, string> formatKeyFun = null, Func<object, string> formatDataFun = null, string spliter = "&")
        {
            if (form == null || form.Count == 0)
                return string.Empty;

            var keyArray = form.Keys.ToArray();
            Array.Sort(keyArray, string.CompareOrdinal);
            if (sortOrder == SortOrder.Descending)
                Array.Reverse(keyArray);

            StringBuilder strs = new StringBuilder();
            foreach (var k in keyArray)
            {
                strs.AppendFormat("{0}={1}{2}", formatKeyFun == null ? k : formatKeyFun(k), formatDataFun == null ? form[k] : formatDataFun(form[k]), spliter);
            }
            strs.Remove(strs.Length - 1, 1);
            return strs.ToString();
        }

    }
}
