using Microsoft.Win32;
using System;
using System.Diagnostics;
using System.IO;

namespace controller.util
{
    class Winrar
    {
        /// <summary>
        /// 验证WinRar是否安装。
        /// </summary>
        /// <returns>true：已安装，false：未安装</returns>
        private static bool ExistsRar(out String winRarPath)
        {
            winRarPath = String.Empty;
            //通过Regedit（注册表）找到WinRar文件
            var registryKey = Registry.LocalMachine.OpenSubKey(@"SOFTWARE\Microsoft\Windows\CurrentVersion\App Paths\WinRAR.exe");
            if (registryKey == null) return false;//未安装
                                                  //registryKey = theReg;可以直接返回Registry对象供会面操作
            winRarPath = registryKey.GetValue("").ToString();
            //这里为节约资源，直接返回路径，反正下面也没用到
            registryKey.Close();//关闭注册表
            return !String.IsNullOrEmpty(winRarPath);
        }

        /// <summary>
        /// 解压
        /// </summary>
        /// <param name="unRarPath">文件夹路径</param>
        /// <param name="rarPath">压缩文件的路径</param>
        /// <param name="rarName">压缩文件的文件名</param>
        /// <returns></returns>
        public static String UnCompressRar(String unRarPath, String rarPath, String rarName)
        {
            try
            {
                String winRarPath = null;
                if (!ExistsRar(out winRarPath)) return "";
                //验证WinRar是否安装。
                if (Directory.Exists(unRarPath) == false)
                {
                    Directory.CreateDirectory(unRarPath);
                }
                var pathInfo = "x " + rarName + " " + unRarPath + " -y";
                var process = new Process
                {
                    StartInfo = new ProcessStartInfo
                    {
                        FileName = winRarPath,//执行的文件名
                        Arguments = pathInfo,//需要执行的命令
                        UseShellExecute = false,//使用Shell执行
                        WindowStyle = ProcessWindowStyle.Hidden,//隐藏窗体
                        WorkingDirectory = rarPath,//rar 存放位置
                        CreateNoWindow = false,//不显示窗体
                    },
                };
                process.Start();//开始执行
                process.WaitForExit();//等待完成并退出
                process.Close();//关闭调用 cmd 的什么什么
            }
            catch (Exception ex)
            {
                throw ex;
            }
            return unRarPath;
        }
    }
}
