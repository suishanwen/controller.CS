using System;
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

        public static void swichVm(string vm1, string vm2, Form1 _mainForm, string customPath, string taskName, string pathShare)
        {
            if (StringUtil.isEmpty(_mainForm.VM3))
            {
                if (!StringUtil.isEmpty(vm1) && !StringUtil.isEmpty(vm2))
                {
                    for (int i = int.Parse(vm1); i <= int.Parse(vm2); i++)
                    {
                        bool pass = false;
                        do
                        {
                            if (taskName.Equals("待命"))
                            {
                                TaskInfos.Clear(i);
                            }
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
                                if (CacheMemory.Equals("") && CustomPath.Equals(customPath) && TaskName.Equals(taskName) && TaskChange.Equals("1"))
                                {
                                    pass = true;
                                }
                            }
                            catch (Exception) { }
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
                IniReadWriter.WriteIniKeys("Command", "CacheMemory" + _mainForm.VM3, "", pathShare + "/TaskPlus.ini");
                IniReadWriter.WriteIniKeys("Command", "CustomPath" + _mainForm.VM3, customPath, pathShare + "/TaskPlus.ini");
                IniReadWriter.WriteIniKeys("Command", "TaskName" + _mainForm.VM3, taskName, pathShare + "/Task.ini");
                IniReadWriter.WriteIniKeys("Command", "TaskChange" + _mainForm.VM3, "1", pathShare + "/Task.ini");
                Form1.SetVM3("");
            }
        }
    }
}
