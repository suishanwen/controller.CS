using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Text;

namespace controller.util
{
    class IniSections
    {
        [DllImport("kernel32")]
        private static extern uint GetPrivateProfileString(

        string lpAppName, // points to section name

                    string lpKeyName, // points to key name

                    string lpDefault, // points to default string

                    byte[] lpReturnedString, // points to destination buffer

                    uint nSize, // size of destination buffer

                    string lpFileName  // points to initialization filename

                );



        /// <summary>

        /// 读取section

        /// </summary>

        /// <param name="Strings"></param>

        /// <returns></returns>

        public static List<string> ReadSections(string iniFilename)

        {

            List<string> result = new List<string>();

            byte[] buf = new byte[65536];

            uint len = GetPrivateProfileString(null, null, null, buf, (uint)buf.Length, iniFilename);

            int j = 0;

            for (int i = 0; i < len; i++)

                if (buf[i] == 0)

                {

                    result.Add(Encoding.Default.GetString(buf, j, i - j));

                    j = i + 1;

                }

            return result;

        }

        /// <summary>

        /// 读取指定区域Keys列表。

        /// </summary>

        /// <param name="Section"></param>

        /// <param name="Strings"></param>

        /// <returns></returns>

        public static List<string> ReadSingleSection(string Section, string iniFilename)

        {

            List<string> result = new List<string>();

            byte[] buf = new byte[65536];

            uint lenf = GetPrivateProfileString(Section, null, null, buf, (uint)buf.Length, iniFilename);

            int j = 0;

            for (int i = 0; i < lenf; i++)

                if (buf[i] == 0)

                {

                    result.Add(Encoding.Default.GetString(buf, j, i - j));

                    j = i + 1;

                }

            return result;

        }

    }
}
