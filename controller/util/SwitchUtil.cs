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

        public static void swichVm(string customPath, string taskName, string pathShare)
        {
            string vm1 = Form1.VM1;
            string vm2 = Form1.VM2;
            string vm3 = Form1.VM3;
            if (StringUtil.isEmpty(vm3))
            {
                if (!StringUtil.isEmpty(vm1) && !StringUtil.isEmpty(vm2))
                {
                    for (int i = int.Parse(vm1); i <= int.Parse(vm2); i++)
                    {
                        SetVmState(i, customPath, taskName, pathShare);
                    }
                }
                else
                {
                    MessageBox.Show("虚拟机不能为空！");
                }

            }
            else
            {
                SetVmState(int.Parse(vm3), customPath, taskName, pathShare);
                Form1.SetVM3("");
            }
        }


        public static void SetVmState(int vm, string customPath, string taskName, string pathShare)
        {
            if (taskName.Equals(SocketAction.TASK_SYS_WAIT_ORDER))
            {
                TaskInfos.Clear(vm);
            }
            IniReadWriter.WriteIniKeys("Command", "CacheMemory" + vm, "", pathShare + "/TaskPlus.ini");
            IniReadWriter.WriteIniKeys("Command", "CustomPath" + vm, customPath, pathShare + "/TaskPlus.ini");
            IniReadWriter.WriteIniKeys("Command", "TaskName" + vm, taskName, pathShare + "/Task.ini");
            IniReadWriter.WriteIniKeys("Command", "TaskChange" + vm, "1", pathShare + "/Task.ini");
        }
    }
}
