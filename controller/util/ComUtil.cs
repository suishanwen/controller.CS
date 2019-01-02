using System;
using System.IO.Ports;

namespace controller.util
{
    class ComUtil
    {
        public static bool Send(string portName,string sendData)
        {
            SerialPort port = new SerialPort
            {
                PortName = portName,
                BaudRate = 4800,
                DataBits = 8,
                StopBits = StopBits.One
            };
            try
            {
                port.Open();
                byte[] bytes = strToHexByte(sendData);
                port.Write(bytes, 0, bytes.Length);
                port.Close();
                return true;
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
                Log.writeLogs("./log.txt",$"串口发送异常:e.Message");
                return false;
            }
        }

        private static byte[] strToHexByte(string hexString)
        {
            hexString = hexString.Replace(" ", "");
            if ((hexString.Length % 2) != 0)
                hexString += " ";
            byte[] returnBytes = new byte[hexString.Length / 2];
            for (int i = 0; i < returnBytes.Length; i++)
                returnBytes[i] = Convert.ToByte(hexString.Substring(i * 2, 2), 16);
            return returnBytes;
        }
    }
}
