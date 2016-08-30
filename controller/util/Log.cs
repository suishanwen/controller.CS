using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace controller.util
{
    class Log
    {
        //写txt
        public static void writeLogs(string pathName, string content)
        {
            if (content.Equals(""))
            {
                StreamWriter sw = new StreamWriter(pathName);
                sw.Write("");
                sw.Close();
            }
            else
            {
                StreamWriter sw = File.AppendText(pathName);
                sw.WriteLine(content);
                sw.Close();
            }
        }
    }
}
