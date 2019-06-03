using controller.util;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Windows.Forms;

namespace controller
{
    class SocketAction
    {
        public static string TASK_SYS_WAIT_ORDER = "待命";
        public static string TASK_SYS_SHUTDOWN = "关机";
        public static string TASK_SYS_RESTART = "重启";
        public static string TASK_SYS_NET_TEST = "网络测试";
        public static string TASK_SYS_UPDATE = "Update";
        public static string TASK_SYS_CLEAN = "CLEAN";

        public static string TASK_VOTE_PROJECT = "投票项目";

        public static string FORM1_VM1 = "FORM1_VM1";
        public static string FORM1_VM2 = "FORM1_VM2";
        public static string FORM1_VM3 = "FORM1_VM3";

        public static string FORM1_WORKER_SET = "FORM1_WORKER_SET";
        public static string FORM1_WORKER_INPUT = "FORM1_WORKER_INPUT";
        public static string FORM1_WORKER_TAIL = "FORM1_WORKER_TAIL";

        public static string FORM1_TIMEOUT = "FORM1_TIMEOUT";

        public static string TASK_EXEC_REPLUG = "TASK_EXEC_REPLUG";


        public static string AUTO_VOTE_SET1 = "AUTO_VOTE";
        public static string AUTO_VOTE_SET2 = "OVER_AUTO";

        public static string AUTO_VOTE_INDEX_SELECT = "AUTO_VOTE_INDEX_SELECT";
        public static string AUTO_VOTE_INDEX_NAME_START = "AUTO_VOTE_INDEX_NAME_START";

        public static string REPORT_STATE = "REPORT_STATE";
        public static string REPORT_STATE_LESS = "REPORT_STATE_LESS";
        public static string REPORT_STATE_DB = "REPORT_STATE_DB";



        private static string PathShare = Form1._Form1.PathShare;


        public static bool TASK_SYS(string taskName, bool socket = false)
        {
            if (socket || MessageBox.Show($"确定要‘{taskName}’吗?", taskName, MessageBoxButtons.OKCancel) == DialogResult.OK)
            {
                SwitchUtil.swichVm("", taskName, PathShare);
                return true;
            }
            return false;
        }

        public static void SYS(string taskName, bool socket = false)
        {
            if (socket)
            {
                BindingFlags flag = BindingFlags.Static | BindingFlags.Public;
                FieldInfo f_key = typeof(SocketAction).GetField(taskName, flag);
                object o = f_key.GetValue(new SocketAction());
                taskName = o.ToString();
            }
            bool result = TASK_SYS(taskName, socket);
            if (result && taskName == TASK_SYS_WAIT_ORDER)
            {
                IniReadWriter.WriteIniKeys("Command", "Copy", "0", PathShare + "/CF.ini");
                IniReadWriter.WriteIniKeys("Command", "Delete", "0", PathShare + "/CF.ini");
                IniReadWriter.WriteIniKeys("Command", "Cookie", "0", PathShare + "/CF.ini");
            }
        }

        public static void VM(int type, string val)
        {
            switch (type)
            {
                case 1:
                    Form1.VM1 = val;
                    break;
                case 2:
                    Form1.VM2 = val;
                    break;
                case 3:
                    Form1.VM3 = val;
                    break;
            }
        }

        public static void WORKER_SET(int type, string val = "")
        {
            switch (type)
            {
                case 1:
                    Form1.WorkerId = val;
                    break;
                case 2:
                    Form1.InputWorkerId = !Form1.InputWorkerId;
                    break;
                case 3:
                    Form1.Tail =  !Form1.Tail;
                    break;
            }
        }

        public static void TIMEOUT_SET(string val)
        {
            Form1.Timeout = val;
        }

        public static void EXEC_REPLUG(string val = "")
        {
            string hexData = IniReadWriter.ReadIniKeys("Command", $"{val}", Form1.GetPathShare() + "/Com.ini");
            if (!StringUtil.isEmpty(hexData))
            {
                if (ComUtil.Send(Form1.GetCom(), hexData))
                {
                    Log.writeLogs("./log.txt", $"{val}号{hexData}发送成功！");
                }
                else
                {
                    Log.writeLogs("./log.txt", $"{val}号{hexData}发送失败！");
                }
            }
            else
            {
                Log.writeLogs("./log.txt", $"{val}号,hexData NOT FOUND！");
            }
        }

        public static void AUTO_VOTE_SET(int type, string val = "")
        {
            Form3.DlAction(type, val);
        }

        public static void AUTO_VOTE_SELECT_INDEX(int index)
        {
            Form3.SetSelectedDataGrid(index);
        }

        public static void AUTO_VOTE_START_NAME_INDEX(string name = "")
        {
            if (!StringUtil.isEmpty(name))
            {
                int index = Form3.VoteProjectMonitorList.FindIndex(VoteProject =>
                {
                    return VoteProject.ProjectName.Equals(name);
                });
                if (index != -1)
                {
                    AUTO_VOTE_SELECT_INDEX(index);
                }
                else
                {
                    return;
                }
            }
            new Thread(Form3.StartSelectedVoteProject).Start();
        }

        public static void REPORT(int type)
        {
            Dictionary<string, string> state = new Dictionary<string, string>();
            Dictionary<string, string> param = new Dictionary<string, string>();
            string prefix = "";
            state.Add("identity", Form1.identity);
            HttpManager httpUtil = HttpManager.getInstance();
            switch (type)
            {
                case 1:
                    param.Add("startNum", Form1.VM1);
                    param.Add("endNum", Form1.VM2);
                    param.Add("workerId", Form1.WorkerId);
                    param.Add("workerInput", Form1.InputWorkerId ? "1" : "0");
                    param.Add("tail", Form1.Tail ? "1" : "0");
                    param.Add("timeout", Form1.Timeout);
                    param.Add("autoVote", Form3.IsAutoVote ? "1" : "0");
                    param.Add("overAuto", Form3.IsOverAuto ? "1" : "0");
                    state.Add("code", JsonUtil.Dict2Json(param));
                    prefix = "/api/mq/send/sync2";
                    break;
                case 2:
                    string arrDrop = IniReadWriter.ReadIniKeys("Command", "ArrDrop", Form1.GetPathShare() + "/CF.ini");
                    state.Add("code", $"arrDrop={arrDrop}&arrActive={Form3.ActiveVm}&taskInfos={ TaskInfos.Active()}");
                    prefix = "/api/mq/send/sync";
                    break;
                case 3:
                    state.Add("startNum", Form1.VM1);
                    state.Add("endNum", Form1.VM2);
                    state.Add("workerId", Form1.WorkerId);
                    state.Add("workerInput", Form1.InputWorkerId ? "1" : "0");
                    state.Add("tail", Form1.Tail ? "1" : "0");
                    state.Add("timeout", Form1.Timeout);
                    state.Add("autoVote", Form3.IsAutoVote ? "1" : "0");
                    state.Add("overAuto", Form3.IsOverAuto ? "1" : "0");
                    prefix = "/api/vote/report";
                    break;
            }
            try
            {
                string result = httpUtil.requestHttpPost("https://bitcoinrobot.cn", prefix, state);
                Log.writeLogs("./socket.txt", "report state:" + result);
            }
            catch (Exception e)
            {
                Log.writeLogs("./socket.txt", "report state ERROR:" + e.Message);
            }

        }

    }
}
