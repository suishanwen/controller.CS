using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace controller.util
{
    class JsonUtil
    {

        public static string Dict2Json(Dictionary<String, String> paras)
        {
            StringBuilder buffer = new StringBuilder("{");
            int i = 0;
            foreach (string key in paras.Keys)
            {
                if (i == 0)
                {
                    buffer.AppendFormat("\"{0}\":\"{1}\"", key, paras[key]);
                }
                else
                {
                    buffer.AppendFormat(",\"{0}\":\"{1}\"", key, paras[key]);
                }
                i++;
            }
            buffer.Append("}");
            return buffer.ToString();
        }

        /// <summary>
            /// 解析JSON数组生成对象实体集合
            /// </summary>
            /// <typeparam name="T">对象类型</typeparam>
            /// <param name="json">json数组字符串</param>
            /// <returns>对象实体集合</returns>
        public static List<T> DeserializeJsonToList<T>(string json) where T : class
        {
            JsonSerializer serializer = new JsonSerializer();
            StringReader sr = new StringReader(json);
            object o = serializer.Deserialize(new JsonTextReader(sr), typeof(List<T>));
            List<T> list = o as List<T>;
            return list;
        }
    }
}
