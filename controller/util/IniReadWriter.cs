using System;
using System.Collections.Generic;
using System.Text;
using System.Runtime.InteropServices;

namespace controller.util
{
    class IniReadWriter
    {
        //ini配置读写
        [DllImport("kernel32")]
        private static extern long WritePrivateProfileString(string section, string key, string val, string filePath);

        [DllImport("kernel32")]
        private static extern int GetPrivateProfileString(string lpAppName, string lpKeyName, string lpDefault, StringBuilder lpReturnedString, int nSize, string lpFileName);

        [DllImport("kernel32.dll", CharSet = CharSet.Auto)]
        private static extern uint GetPrivateProfileSection(string lpAppName, IntPtr lpReturnedString, uint nSize, string lpFileName);

        private static string ReadString(string section, string key, string def, string filePath)
        {
            StringBuilder temp = new StringBuilder(1024);

            try
            {
                GetPrivateProfileString(section, key, def, temp, 1024, filePath);
            }
            catch
            { }
            return temp.ToString();
        }

        /// <summary>  
        /// 根据section，key取值  
        /// </summary>  
        /// <param name="section"></param>  
        /// <param name="keys"></param>  
        /// <param name="filePath">ini文件路径</param>  
        /// <returns></returns>  
        public static string ReadIniKeys(string section, string keys, string filePath)
        {
            return ReadString(section, keys, "", filePath);
        }

        /// <summary>  
        /// 保存ini  
        /// </summary>  
        /// <param name="section"></param>  
        /// <param name="key"></param>  
        /// <param name="value"></param>  
        /// <param name="filePath">ini文件路径</param>  
        public static void WriteIniKeys(string section, string key, string value, string filePath)
        {
            WritePrivateProfileString(section, key, value, filePath);
        }
    }
}
