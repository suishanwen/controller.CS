using System.IO;

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
