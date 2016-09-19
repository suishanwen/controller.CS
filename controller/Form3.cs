using controller.util;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.IO;
using System.Threading.Tasks;
using System.Text.RegularExpressions;
using System.Threading;
using System.Windows.Forms;

namespace controller
{
    public partial class Form3 : Form
    {
        private static Form1 _mainForm;
        private int count;
        private int val;
        private int over;
        private int kick;
        private string arrDrop;
        private string activeVm;
        private Thread autoVote;
        private VoteProject activeVoteProject;
        private List<VoteProject> voteProjectMonitorList = new List<VoteProject>();
        private string voteProjectNameDroped;
        private string voteProjectNameGreen;
        private int downLoadCount;
        private bool isDownloading;

        internal List<VoteProject> VoteProjectMonitorList
        {
            get
            {
                return voteProjectMonitorList;
            }
        }

        public Thread AutoVote
        {
            get
            {
                return autoVote;
            }
        }

        public Form3()
        {

        }
        public Form3(Form1 form1)
        {
            _mainForm = form1;
            InitializeComponent();
            timer1.Enabled = true;
            string isAutoVote= IniReadWriter.ReadIniKeys("Command", "isAutoVote", _mainForm.PathShare + "/CF.ini");
            if(!StringUtil.isEmpty(isAutoVote)&& isAutoVote .Equals("1"))
            {
                timer2.Enabled = true;
                autoVote = new Thread(autoVoteSystem);
                autoVote.Start();
            }
        }



        private bool isDropedProject(string project, int checkType)
        {
            voteProjectNameDroped = IniReadWriter.ReadIniKeys("Command", "voteProjectNameDroped", _mainForm.PathShare + "/AutoVote.ini");
            if (checkType == 1)
            {
                string[] dropedProjectList = voteProjectNameDroped.Split('|');
                foreach (string dropedProject in dropedProjectList)
                {
                    if (project.IndexOf(dropedProject) != -1)
                    {
                        return true;
                    }
                }
                return false;
            }

            return voteProjectNameDroped.IndexOf(project) != -1;
        }

        private bool isGreenProject(string project, int checkType)
        {

            voteProjectNameGreen = IniReadWriter.ReadIniKeys("Command", "voteProjectNameGreen", _mainForm.PathShare + "/AutoVote.ini");
            string[] greenProjectList = voteProjectNameGreen.Split('|');
            foreach (string greenProject in greenProjectList)
            {
                if (project.IndexOf(greenProject) != -1)
                {
                    return true;
                }
            }
            return false;

        }

        private List<VoteProject> getVoteProjects()
        {
            HttpManager httpUtil = HttpManager.getInstance();
            string result = "";
            do
            {
                try
                {
                    result = httpUtil.requestHttpGet("http://butingzhuan.com/tasks.php", "", "");
                }
                catch (Exception e)
                {
                    Log.writeLogs("./log.txt", "Request Fail!Retry in 10s...");
                    Thread.Sleep(10000);
                }
            } while (result == "");
            result = result.Substring(result.IndexOf("时间</td>"));
            result = result.Substring(0, result.IndexOf("qzd_yj"));
            result = result.Substring(result.IndexOf("<tr class='blank'>"));
            result = result.Substring(0, result.LastIndexOf("<tr class='blank'>"));
            //Log.writeLogs("./log.txt", "Finished Request!");
            Regex regTR = new Regex(@"(?is)<tr[^>]*>(?:(?!</tr>).)*</tr>");
            Regex regTD = new Regex(@"(?is)<t[dh][^>]*>((?:(?!</td>).)*)</t[dh]>");
            MatchCollection mcTR = regTR.Matches(result);
            List<VoteProject> voteProjectList = new List<VoteProject>();
            foreach (Match mTR in mcTR)
            {
                if (mTR.Value.IndexOf("不换") == -1 && !isDropedProject(mTR.Value, 1))
                {
                    MatchCollection mcTD = regTD.Matches(mTR.Value);
                    int index = 0;
                    VoteProject voteProject = new VoteProject();
                    foreach (Match mTD in mcTD)
                    {
                        string innerTd = mTD.Groups[1].Value;
                        //Log.writeLogs("./log.txt", "***"+index);
                        //Log.writeLogs("./log.txt", mTD.Value);
                        switch (index)
                        {
                            case 2:
                                voteProject.ProjectName = HtmlMatch.GetContent(innerTd, "a");
                                break;
                            case 5:
                                voteProject.Price = double.Parse(innerTd);
                                break;
                            case 7:
                                String[] quantityInfo = mTD.Value.Split('"');
                                quantityInfo = quantityInfo[1].Split('/');
                                try
                                {
                                    voteProject.Remains = long.Parse(innerTd.Trim());
                                    if (!StringUtil.isEmpty(quantityInfo[0].Trim()))
                                    {

                                        voteProject.FinishQuantity = long.Parse(quantityInfo[0]);

                                    }
                                    voteProject.TotalRequire = long.Parse(quantityInfo[1].Substring(0, quantityInfo[1].IndexOf(" ")));

                                }
                                catch (Exception e) { }
                                break;
                            case 8:
                                voteProject.BackgroundAddress = HtmlMatch.GetAttr(innerTd, "a", "href");
                                break;
                            case 9:
                                voteProject.DownloadAddress = HtmlMatch.GetAttr(innerTd, "a", "href");
                                break;
                            case 10:
                                try
                                {
                                    voteProject.IdType = HtmlMatch.GetAttr(innerTd, "input", "value").Substring(0, 2);
                                }
                                catch (Exception e)
                                {
                                    if (innerTd.IndexOf("BT-") != -1)
                                    {
                                        voteProject.IdType = "BT";
                                    }
                                    else if (innerTd.IndexOf("AQ-") != -1)
                                    {
                                        voteProject.IdType = "AQ";
                                    }
                                    else if (innerTd.IndexOf("Q7-") != -1)
                                    {
                                        voteProject.IdType = "Q7";
                                    }
                                }
                                break;
                            case 12:
                                voteProject.BackgroundNo = innerTd;
                                break;
                            case 13:
                                voteProject.RefreshDate = Convert.ToDateTime("2016-" + innerTd + ":00");
                                break;
                        }
                        index++;
                    }
                    voteProject.IsRestrict = voteProject.BackgroundNo.IndexOf("限制") != -1;
                    voteProjectList.Add(voteProject);
                }
            }
            return voteProjectList;
        }

        private void voteProjectsAnalysis(List<VoteProject> voteProjectList)
        {
            voteProjectMonitorList.Clear();
            foreach (VoteProject voteProject in voteProjectList)
            {
                //不存在于黑名单，并且是九天项目
                if (!isDropedProject(voteProject.ProjectName, 0) && voteProject.BackgroundAddress.IndexOf("http://61.153.107.108") != -1)
                {
                    voteProjectMonitorList.Add(voteProject);
                }
            }
        }


        private void writeAutoVoteProject()
        {
            if (activeVoteProject == null)
            {
                SwitchUtil.clearAutoVote(_mainForm.PathShare);
            }
            else
            {
                IniReadWriter.WriteIniKeys("Command", "ProjectName", activeVoteProject.ProjectName, _mainForm.PathShare + "/AutoVote.ini");
                IniReadWriter.WriteIniKeys("Command", "Price", activeVoteProject.Price.ToString(), _mainForm.PathShare + "/AutoVote.ini");
                IniReadWriter.WriteIniKeys("Command", "TotalRequire", activeVoteProject.TotalRequire.ToString(), _mainForm.PathShare + "/AutoVote.ini");
                IniReadWriter.WriteIniKeys("Command", "FinishQuantity", activeVoteProject.FinishQuantity.ToString(), _mainForm.PathShare + "/AutoVote.ini");
                IniReadWriter.WriteIniKeys("Command", "Remains", activeVoteProject.Remains.ToString(), _mainForm.PathShare + "/AutoVote.ini");
                IniReadWriter.WriteIniKeys("Command", "BackgroundNo", activeVoteProject.BackgroundNo, _mainForm.PathShare + "/AutoVote.ini");
                IniReadWriter.WriteIniKeys("Command", "BackgroundAddress", activeVoteProject.BackgroundAddress, _mainForm.PathShare + "/AutoVote.ini");
                IniReadWriter.WriteIniKeys("Command", "DownloadAddress", activeVoteProject.DownloadAddress, _mainForm.PathShare + "/AutoVote.ini");
                IniReadWriter.WriteIniKeys("Command", "IsRestrict", activeVoteProject.IsRestrict.ToString(), _mainForm.PathShare + "/AutoVote.ini");
                IniReadWriter.WriteIniKeys("Command", "IdType", activeVoteProject.IdType, _mainForm.PathShare + "/AutoVote.ini");
                IniReadWriter.WriteIniKeys("Command", "RefreshDate", activeVoteProject.RefreshDate.ToLocalTime().ToString(), _mainForm.PathShare + "/AutoVote.ini");
                IniReadWriter.WriteIniKeys("Command", "dropVote", "0", _mainForm.PathShare + "/AutoVote.ini");
            }
        }

        private void setWorkerId()
        {
            if (activeVoteProject.IdType.Equals("Q7"))
            {
                IniReadWriter.WriteIniKeys("Command", "worker", "Q7-21173", _mainForm.PathShare + "/CF.ini");
            }
            else
            {
                IniReadWriter.WriteIniKeys("Command", "worker", "AQ-239356", _mainForm.PathShare + "/CF.ini");
            }
        }

        private void testVoteProjectMonitorList()
        {
            for (int i = 0; i < voteProjectMonitorList.Count; i++)
            {
                VoteProject voteProject = voteProjectMonitorList[i];
                if (voteProject.Remains > 0 && (voteProject.Remains * voteProject.Price) > 100 && !voteProject.IsRestrict)
                {
                    Console.WriteLine("projectName：" + voteProject.ProjectName + ",price：" + voteProject.Price + ",remains：" + voteProject.Remains);
                    HttpManager httpManager = HttpManager.getInstance();
                    string pathName = IniReadWriter.ReadIniKeys("Command", "Downloads", _mainForm.PathShare + "/CF.ini") + "\\" + voteProject.DownloadAddress.Substring(voteProject.DownloadAddress.LastIndexOf("/") + 1);
                    string url = voteProject.DownloadAddress;
                    string now = DateTime.Now.ToLocalTime().ToString();
                    Log.writeLogs("./log.txt", "开始下载:" + url);
                    downLoadCount = 0;
                    isDownloading = true;
                    httpManager.HttpDownloadFile(url, pathName);
                    isDownloading = false;
                    Log.writeLogs("./log.txt", pathName + "  下载完成");
                    Winrar.UnCompressRar(_mainForm.PathShare + "/投票项目/" + voteProject.ProjectName, IniReadWriter.ReadIniKeys("Command", "Downloads", _mainForm.PathShare + "/CF.ini"), voteProject.DownloadAddress.Substring(voteProject.DownloadAddress.LastIndexOf("/") + 1));
                    try
                    {
                        File.Delete(pathName);
                    }
                    catch (IOException)
                    {
                        Log.writeLogs("./log.txt", pathName + "-->文件占用中，无法删除!");
                    }
                    activeVoteProject = voteProject;
                    writeAutoVoteProject();
                    setWorkerId();
                    _mainForm.VM3TextBox.Text = "";
                    Log.writeLogs("./log.txt", "AutoVote: " + voteProject.ProjectName + " " + voteProject.BackgroundNo+"    "+ DateTime.Now.ToLocalTime().ToString());
                    SwitchUtil.swichVm(_mainForm.VM1, _mainForm.VM2, _mainForm.VM3TextBox, _mainForm.PathShareVm + "\\投票项目\\" + voteProject.ProjectName + "\\vote.exe", "投票项目", _mainForm.PathShare);
                    break;
                }
            }

        }

        private bool isWaitOrder()
        {
            string arrDrop= IniReadWriter.ReadIniKeys("Command", "ArrDrop", _mainForm.PathShare + "/CF.ini");
            for (int i = int.Parse(_mainForm.VM1); i <= int.Parse(_mainForm.VM2); i++)
            {
                if (arrDrop.IndexOf(" "+i+" |") == -1)
                {
                    string taskName = IniReadWriter.ReadIniKeys("Command", "TaskName" + i, _mainForm.PathShare + "/Task.ini");
                    if (!taskName.Equals("待命"))
                    {
                        return false;
                    }
                }
            }
            return true;
        }

        private void autoVoteSystem()
        {
            Log.writeLogs("./log.txt", "");
            Log.writeLogs("./log.txt", "AutoVoteSystem Thread Running");
            do
            {
                if (isWaitOrder())
                {
                    voteProjectsAnalysis(getVoteProjects());
                    testVoteProjectMonitorList();
                }
                Thread.Sleep(30000);
            }
            while (true);
        }

        private void timer1_Tick(object sender, EventArgs e)
        {
            count++;
            Console.WriteLine(count);
            if (count == 1)
            {
                val = 0;
                over = 0;
                kick = 0;
                try
                {
                    val = int.Parse(IniReadWriter.ReadIniKeys("Command", "Val", _mainForm.PathShare + "/CF.ini"));

                }
                catch (Exception)
                {
                    IniReadWriter.WriteIniKeys("Command", "Val", "0", _mainForm.PathShare + "/CF.ini");
                }
                try
                {
                    over = int.Parse(IniReadWriter.ReadIniKeys("Command", "OVER", _mainForm.PathShare + "/CF.ini"));
                }
                catch (Exception)
                {
                    IniReadWriter.WriteIniKeys("Command", "OVER", "0", _mainForm.PathShare + "/CF.ini");
                }
                try
                {
                    kick = int.Parse(IniReadWriter.ReadIniKeys("Command", "KICK", _mainForm.PathShare + "/CF.ini"));
                }
                catch (Exception)
                {
                    IniReadWriter.WriteIniKeys("Command", "KICK", "0", _mainForm.PathShare + "/CF.ini");
                }
                arrDrop = IniReadWriter.ReadIniKeys("Command", "ArrDrop", _mainForm.PathShare + "/CF.ini");


            }
            else if (count == 2)
            {
                if (val > 0)
                {
                    _mainForm.NotifyIcon1.ShowBalloonTip(0, val + "号虚拟机网络异常", DateTime.Now.ToLocalTime().ToString(), ToolTipIcon.Info);
                    IniReadWriter.WriteIniKeys("Command", "Val", "0", _mainForm.PathShare + "/CF.ini");
                }
                else if (val < 0)
                {
                    val = 0 - val;
                    string[] strArray = arrDrop.Split('|');
                    List<int> dropList = new List<int>();
                    string drop = "";
                    foreach (string str in strArray)
                    {
                        string currentStr = str.Replace(" ", "");
                        if (!StringUtil.isEmpty(currentStr))
                        {
                            dropList.Add(int.Parse(currentStr));
                        }
                    }
                    if (!dropList.Contains(val))
                    {
                        dropList.Add(val);
                        dropList.Sort();
                    }
                    foreach (int num in dropList)
                    {
                        drop += " " + num + " |";
                    }
                    IniReadWriter.WriteIniKeys("Command", "Val", "0", _mainForm.PathShare + "/CF.ini");
                    IniReadWriter.WriteIniKeys("Command", "ArrDrop", drop, _mainForm.PathShare + "/CF.ini");
                    _mainForm.NotifyIcon1.ShowBalloonTip(0, val + "号虚拟机掉线了", DateTime.Now.ToLocalTime().ToString(), ToolTipIcon.Error);

                }
            }
            else if (count == 4)
            {
                if (over > 0)
                {
                    IniReadWriter.WriteIniKeys("Command", "OVER", "0", _mainForm.PathShare + "/CF.ini");
                    _mainForm.NotifyIcon1.ShowBalloonTip(0, "项目已结束", DateTime.Now.ToLocalTime().ToString(), ToolTipIcon.Info);
                }
                if (kick > 0)
                {
                    IniReadWriter.WriteIniKeys("Command", "KICK", "0", _mainForm.PathShare + "/CF.ini");
                    _mainForm.NotifyIcon1.ShowBalloonTip(0, "项目限人", DateTime.Now.ToLocalTime().ToString(), ToolTipIcon.Info);
                }

            }
            else if (count == 3||count == 5)
            {
                activeVm = "";
                for (int i = int.Parse(_mainForm.VM1); i <= int.Parse(_mainForm.VM2); i++)
                {
                    string state = IniReadWriter.ReadIniKeys("Command", "TaskChange" + i, _mainForm.PathShare + "/Task.ini");
                    if (state == "1")
                    {
                        activeVm += " " + i + " |";
                    }
                }
                if (arrDrop != "")
                {
                    label3.Text = arrDrop;
                }
                else
                {
                    label3.Text = "无";
                }
                if (activeVm != "")
                {
                    label4.Text = activeVm;
                }
                else
                {
                    label4.Text = "无";
                }
                count = 0;
                this.Text = "实时监控(" + voteProjectMonitorList.Count + ")";
            }
        }

        private void timer2_Tick(object sender, EventArgs e)
        {
            if (isDownloading)
            {
                downLoadCount++;
                if (downLoadCount > 15)
                {
                    Log.writeLogs("./log.txt", "Download OverTime,Thread Restart");
                    try
                    {
                        autoVote.Abort();
                        autoVote = new Thread(autoVoteSystem);
                        autoVote.Start();
                        timer2.Enabled = false;
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show(ex.ToString());
                    }
                }
            }
        }
    }
}
