using System;
using System.Collections.Generic;
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
    }
}
