using controller.util;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
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

        public static string TASK_PC_RAR = "TASK_PC_RAR";
        public static string TASK_PC_EPT = "TASK_PC_EPT";
        public static string TASK_PC_UPGRADE = "TASK_PC_UPGRADE";

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

        public static string DROP_PROJECT = "DROP_PROJECT";
        public static string TOP_PROJECT = "TOP_PROJECT";


        public static string REPORT_STATE = "REPORT_STATE";
        public static string REPORT_STATE_LESS = "REPORT_STATE_LESS";
        public static string REPORT_STATE_VOTE = "REPORT_STATE_VOTE";
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
                    Form1.Tail = !Form1.Tail;
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
            string prefix = "";
            state.Add("identity", Form1.identity);
            HttpManager httpUtil = HttpManager.getInstance();
            switch (type)
            {
                case 1:
                    string workerInput = Form1.InputWorkerId ? "1" : "0";
                    string tail = Form1.Tail ? "1" : "0";
                    string autoVote = Form3.IsAutoVote ? "1" : "0";
                    string overAuto = Form3.IsOverAuto ? "1" : "0";
                    string voteProjectNameDroped = IniReadWriter.ReadIniKeys("Command", "voteProjectNameDroped",
                           Form1.GetPathShare() + "/AutoVote.ini");
                    string voteProjectNameDropedTemp = IniReadWriter.ReadIniKeys("Command", "voteProjectNameDropedTemp",
                           Form1.GetPathShare() + "/AutoVote.ini");
                    string voteProjectNameToped = IniReadWriter.ReadIniKeys("Command", "voteProjectNameToped",
                           Form1.GetPathShare() + "/AutoVote.ini");
                    state.Add("code", $"startNum={Form1.VM1}&endNum={Form1.VM2}&workerId={Form1.WorkerId}&workerInput={workerInput}&tail={tail}&timeout={Form1.Timeout}&autoVote={autoVote}&overAuto={overAuto}&dropped={voteProjectNameDroped}&droppedTemp={voteProjectNameDropedTemp}&topped={voteProjectNameToped}");
                    prefix = "/api/mq/send/sync";
                    break;
                case 2:
                    string arrDrop = IniReadWriter.ReadIniKeys("Command", "ArrDrop", Form1.GetPathShare() + "/CF.ini");
                    state.Add("code", $"arrDrop={arrDrop}&arrActive={Form3.ActiveVm}&taskInfos={ TaskInfos.Active()}");
                    prefix = "/api/mq/send/sync";
                    break;
                case 3:
                    string votes = getVotes();
                    state.Add("code", $"votes={votes}");
                    prefix = "/api/mq/send/sync";
                    break;
                case 4:
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


        //通过路径启动进程
        private static void StartProcess(string pathName)
        {
            ProcessStartInfo info = new ProcessStartInfo();
            info.FileName = pathName;
            info.Arguments = "";
            info.WorkingDirectory = pathName.Substring(0, pathName.LastIndexOf("\\"));
            info.WindowStyle = ProcessWindowStyle.Normal;
            Process pro = Process.Start(info);
            Thread.Sleep(500);
        }


        public static void PC_UPGRADE()
        {
            Log.writeLogs("./log.txt", "开始下载:更新");
            string pathName = "./controller-new.exe";
            string url = "http://bitcoinrobot.cn/file/controller.exe";
            bool isDownloading = true;
            HttpManager httpManager = HttpManager.getInstance();
            do
            {
                try
                {
                    httpManager.HttpDownloadFile(url, pathName);
                    isDownloading = false;
                }
                catch (Exception)
                {
                    Log.writeLogs("./log.txt", "更新下载异常，重新下载");
                    File.Delete(pathName);
                    Thread.Sleep(1000);
                }
            } while (isDownloading);
            if (!File.Exists("./update.bat"))
            {
                string line1 = "Taskkill /F /IM controller.exe";
                string line2 = "ping -n 3 127.0.0.1>nul";
                string line3 = "del /s /Q " + Environment.CurrentDirectory + "\\controller.exe";
                string line4 = "ping -n 3 127.0.0.1>nul";
                string line5 = "ren " + Environment.CurrentDirectory + "\\controller-new.exe controller.exe";
                string line6 = "ping -n 3 127.0.0.1>nul";
                string line7 = "start " + Environment.CurrentDirectory + "\\controller.exe";
                string[] lines = { "@echo off", line1, line2, line3, line4, line5, line6, line7 };
                File.WriteAllLines(@"./update.bat", lines, Encoding.GetEncoding("GBK"));
            }

            StartProcess(Environment.CurrentDirectory + "\\update.bat");
            Application.Exit();//退出整个应用程序
        }

        public static void PC_RAR()
        {
            DirectoryInfo theFolder = new DirectoryInfo(Form1.Downloads);
            FileInfo[] fileInfo = theFolder.GetFiles();
            foreach (FileInfo NextFile in fileInfo)//遍历文件
            {
                Console.WriteLine(NextFile.Name);
                int index = NextFile.Name.IndexOf(".rar") != -1 ? NextFile.Name.IndexOf(".rar") : NextFile.Name.IndexOf(".zip");
                if (index == -1)
                {
                    index = NextFile.Name.IndexOf(".RAR") != -1 ? NextFile.Name.IndexOf(".RAR") : NextFile.Name.IndexOf(".ZIP");
                }
                if (index != -1)
                {
                    Winrar.UnCompressRar(PathShare + "/投票项目/" + NextFile.Name.Substring(0, index), NextFile.DirectoryName, NextFile.Name);
                    try
                    {
                        File.Delete(NextFile.FullName);

                    }
                    catch (System.IO.IOException)
                    {
                        Console.WriteLine(NextFile.FullName + "-->文件占用中，无法删除!");
                    }
                }
            }

        }

        public static void PC_EPT()
        {
            DirectoryInfo theFolder = new DirectoryInfo(PathShare + "/投票项目");
            DirectoryInfo[] allDir = theFolder.GetDirectories();
            foreach (DirectoryInfo d in allDir)
            {
                try
                {
                    Directory.Delete(d.FullName, true);
                }
                catch (Exception)
                {
                    Console.WriteLine(d.FullName + "-->文件占用中，无法删除!");
                }
            }
            string downLoads = IniReadWriter.ReadIniKeys("Command", "Downloads", PathShare + "/CF.ini");
            if (!StringUtil.isEmpty(downLoads))
            {
                DirectoryInfo di = new DirectoryInfo(downLoads);
                FileInfo[] files = di.GetFiles();
                foreach (FileInfo f in files)
                {
                    try
                    {
                        File.Delete(f.FullName);
                    }
                    catch (Exception)
                    {
                        Console.WriteLine(f.FullName + "-->文件占用中，无法删除!");
                    }
                }
            }
        }

        public static void Drop_Project(string projectName, bool val)
        {
            string voteProjectNameDroped = IniReadWriter.ReadIniKeys("Command", "voteProjectNameDroped",
                           Form1.GetPathShare() + "/AutoVote.ini");
            if (!val)
            {
                voteProjectNameDroped +=
                    StringUtil.isEmpty(voteProjectNameDroped) ? projectName : "|" + projectName;
            }
            else
            {
                voteProjectNameDroped = voteProjectNameDroped.Replace("|" + projectName, "")
                    .Replace(projectName, "");
                string voteProjectNameDropedTemp = IniReadWriter.ReadIniKeys("Command", "voteProjectNameDropedTemp", Form1.GetPathShare() + "/AutoVote.ini");
                if (voteProjectNameDropedTemp.IndexOf(projectName) != -1)
                {
                    voteProjectNameDropedTemp = voteProjectNameDropedTemp.Replace("|" + projectName, "")
                        .Replace(projectName, "");
                    IniReadWriter.WriteIniKeys("Command", "voteProjectNameDropedTemp",
                        voteProjectNameDropedTemp, Form1.GetPathShare() + "/AutoVote.ini");
                }

            }
            IniReadWriter.WriteIniKeys("Command", "voteProjectNameDroped", voteProjectNameDroped,
                Form1.GetPathShare() + "/AutoVote.ini");
        }

        public static void Top_Project(string projectName, bool val) {
            string allProjectName = projectName.Split('_')[0];
            string voteProjectNameToped = IniReadWriter.ReadIniKeys("Command", "voteProjectNameToped",
                Form1.GetPathShare() + "/AutoVote.ini");
            if (!val)
            {
                if (voteProjectNameToped.IndexOf(allProjectName) == -1)
                {
                    voteProjectNameToped += StringUtil.isEmpty(voteProjectNameToped)
                        ? allProjectName
                        : "|" + allProjectName;
                }

            }
            else
            {
                voteProjectNameToped = voteProjectNameToped.Replace("|" + allProjectName, "")
                    .Replace(allProjectName, "");
            }

            IniReadWriter.WriteIniKeys("Command", "voteProjectNameToped", voteProjectNameToped,
                Form1.GetPathShare() + "/AutoVote.ini");
        }

        public static string getVotes()
        {

            DirectoryInfo theFolder = new DirectoryInfo(PathShare + "/投票项目");
            DirectoryInfo[] allDir = theFolder.GetDirectories();
            string votes = "";
            for (int i = 0; i < allDir.Length; i++)
            {
                if (i == 0)
                {
                    votes += allDir[i];
                }
                else
                {
                    votes += $",{allDir[i]}";

                }
            }
            return votes;
        }

        public static void Vote(string projectName)
        {
            DirectoryInfo theFolder = new DirectoryInfo(PathShare + "/投票项目/" +projectName);
            FileInfo[] fileInfo = theFolder.GetFiles();
            FileInfo executableFile = null;
            for (int i = 0; i < fileInfo.Length; i++)
            {
                if (fileInfo[i].Name.IndexOf(".exe") != -1)
                {
                    executableFile = fileInfo[i];
                    break;
                }
            }
            if (executableFile != null)
            {
                if (executableFile.FullName.IndexOf("vote.exe") != -1)
                {
                    if (!File.Exists(executableFile.FullName.Substring(0, executableFile.FullName.Length - 9) + "/启动九天.bat"))
                    {
                        try
                        {
                            String[] Lines = { @"start vote.exe" };
                            File.WriteAllLines(executableFile.FullName.Substring(0, executableFile.FullName.Length - 9) + "/启动九天.bat", Lines, Encoding.GetEncoding("GBK"));
                        }
                        catch (Exception ex)
                        {
                            MessageBox.Show(ex.ToString());
                        }
                    }
                }
                string pathName = executableFile.FullName.Replace(Form1.GetPathShare(), Form1.GetPathShareVm());
                if (Form1.OverSwitch)
                {
                    Form1._Form1.OverSwitchPath = pathName;
                    Form1.SetOverSwitchText(true, $"到票切换{projectName}");
                }
                else
                {
                    SwitchUtil.clearAutoVote(Form1.GetPathShare());
                    SwitchUtil.swichVm(pathName, "投票项目", Form1.GetPathShare());
                }
            }
            else
            {
                MessageBox.Show("所选项目中无可执行文件，请检查！");
            }
        }
    }
    
}
