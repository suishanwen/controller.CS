using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using controller.util;
using System.Windows.Forms;
using System.Threading;

namespace controller.util
{
    class SwitchUtil
    {

        public static void clearAutoVote(string pathShare)
        {
            IniReadWriter.WriteIniKeys("Command", "ProjectName", "", pathShare + "/AutoVote.ini");
            IniReadWriter.WriteIniKeys("Command", "Price", "", pathShare + "/AutoVote.ini");
            IniReadWriter.WriteIniKeys("Command", "TotalRequire", "", pathShare + "/AutoVote.ini");
            IniReadWriter.WriteIniKeys("Command", "FinishQuantity", "", pathShare + "/AutoVote.ini");
            IniReadWriter.WriteIniKeys("Command", "Remains", "", pathShare + "/AutoVote.ini");
            IniReadWriter.WriteIniKeys("Command", "BackgroundNo", "", pathShare + "/AutoVote.ini");
            IniReadWriter.WriteIniKeys("Command", "BackgroundAddress", "", pathShare + "/AutoVote.ini");
            IniReadWriter.WriteIniKeys("Command", "DownloadAddress", "", pathShare + "/AutoVote.ini");
            IniReadWriter.WriteIniKeys("Command", "IsRestrict", "", pathShare + "/AutoVote.ini");
            IniReadWriter.WriteIniKeys("Command", "IdType", "", pathShare + "/AutoVote.ini");
            IniReadWriter.WriteIniKeys("Command", "RefreshDate", "", pathShare + "/AutoVote.ini");
            IniReadWriter.WriteIniKeys("Command", "dropVote", "0", pathShare + "/AutoVote.ini");
        }

        public static void swichVm(string vm1, string vm2, TextBox textBox, string customPath, string taskName, string pathShare)
        {
            if (StringUtil.isEmpty(textBox.Text))
            {
                if (!StringUtil.isEmpty(vm1) && !StringUtil.isEmpty(vm2))
                {
                    for (int i = int.Parse(vm1); i <= int.Parse(vm2); i++)
                    {
                        bool pass = false;
                        do
                        {
                            IniReadWriter.WriteIniKeys("Command", "CacheMemory" + i, "", pathShare + "/TaskPlus.ini");
                            IniReadWriter.WriteIniKeys("Command", "CustomPath" + i, customPath, pathShare + "/TaskPlus.ini");
                            IniReadWriter.WriteIniKeys("Command", "TaskName" + i, taskName, pathShare + "/Task.ini");
                            IniReadWriter.WriteIniKeys("Command", "TaskChange" + i, "1", pathShare + "/Task.ini");
                            Thread.Sleep(50);
                            String CacheMemory = IniReadWriter.ReadIniKeys("Command", "CacheMemory" + i, pathShare + "/TaskPlus.ini");
                            String CustomPath = IniReadWriter.ReadIniKeys("Command", "CustomPath" + i, pathShare + "/TaskPlus.ini");
                            String TaskName = IniReadWriter.ReadIniKeys("Command", "TaskName" + i, pathShare + "/Task.ini");
                            String TaskChange = IniReadWriter.ReadIniKeys("Command", "TaskChange" + i, pathShare + "/Task.ini");
                            try
                            {
                                if (CacheMemory.Equals("")&& CustomPath.Equals(customPath)&& TaskName.Equals(taskName)&& TaskChange.Equals("1"))
                                {
                                    pass = true;
                                }
                            }
                            catch(Exception){}
                        } while (!pass);
                    }
                }
                else
                {
                    MessageBox.Show("虚拟机不能为空！");
                }

            }
            else
            {
                IniReadWriter.WriteIniKeys("Command", "CacheMemory" + textBox.Text, "", pathShare + "/TaskPlus.ini");
                IniReadWriter.WriteIniKeys("Command", "CustomPath" + textBox.Text, customPath, pathShare + "/TaskPlus.ini");
                IniReadWriter.WriteIniKeys("Command", "TaskName" + textBox.Text, taskName, pathShare + "/Task.ini");
                IniReadWriter.WriteIniKeys("Command", "TaskChange" + textBox.Text, "1", pathShare + "/Task.ini");
                textBox.Text = "";
            }
        }
    }
}
