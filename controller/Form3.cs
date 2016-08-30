using controller.util;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
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
        private string voteProjectNameDroped = IniReadWriter.ReadIniKeys("Command", "voteProjectNameDroped", "./project.ini");
        private string voteProjectNameGreen = IniReadWriter.ReadIniKeys("Command", "voteProjectNameGreen", "./project.ini");

        public Form3()
        {

        }
        public Form3(Form1 form1)
        {
            _mainForm = form1;
            InitializeComponent();
            timer1.Enabled = true;
            autoVote = new Thread(autoVoteSystem);
            autoVote.Start();
        }


        //关闭程序，结束自动挂票线程
        private void form3_FormClosing(object sender, FormClosingEventArgs e)
        {
            autoVote.Abort();
        }

        private bool isDropedProject(string project, int checkType)
        {
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
            string result = httpUtil.requestHttpGet("http://butingzhuan.com/tasks.php", "", "");
            result = result.Substring(result.IndexOf("时间</td>"));
            result = result.Substring(0, result.IndexOf("qzd_yj"));
            result = result.Substring(result.IndexOf("<tr class='blank'>"));
            result = result.Substring(0, result.LastIndexOf("<tr class='blank'>"));
            Log.writeLogs("./log.txt", "Finished Request!");
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
                        Log.writeLogs("./log.txt", "***"+index);
                        Log.writeLogs("./log.txt", mTD.Value);
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

                                }catch(Exception e) { }
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
            foreach (VoteProject voteProject in voteProjectList)
            {
                //不存在于黑名单，并且是九天项目
                if (!isDropedProject(voteProject.ProjectName, 0) && voteProject.BackgroundAddress.IndexOf("http://61.153.107.108") != -1)
                {
                    bool exist = false;
                    for (int i = 0; i < voteProjectMonitorList.Count; i++)
                    {
                        if (voteProject.ProjectName.Equals(voteProjectMonitorList[i].ProjectName))
                        {
                            exist = true;
                            break;
                        }
                    }
                    if (!exist)
                    {
                        voteProjectMonitorList.Add(voteProject);
                    }
                }
            }
        }

        private void testVoteProjectMonitorList()
        {
            int i = 0;
            foreach (VoteProject voteProject in voteProjectMonitorList)
            {
                if (i == 0)
                {
                    HttpManager httpManager = HttpManager.getInstance();
                    httpManager.HttpDownloadFile(voteProject.DownloadAddress, IniReadWriter.ReadIniKeys("Command", "Downloads", _mainForm.PathShare+"/CF.ini")+ voteProject.DownloadAddress.Substring(voteProject.DownloadAddress.LastIndexOf("/")));
                }
                Console.WriteLine("projectName：" + voteProject.ProjectName + ",price：" + voteProject.Price + ",remains：" + voteProject.Remains);
                i++;
            }
        }

        private void autoVoteSystem()
        {
            Log.writeLogs("./log.txt", "");
            Log.writeLogs("./log.txt", "AutoVoteSystem Thread Running");
            do
            {
                voteProjectsAnalysis(getVoteProjects());
                testVoteProjectMonitorList();
                Thread.Sleep(30000);
            }
            while (true);
        }

        private void timer1_Tick(object sender, EventArgs e)
        {
            count++;
            Console.WriteLine(count);
            if (count == 2)
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
            else if (count == 4)
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
            else if (count == 6)
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
            else if (count == 8)
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

            }
        }
    }
}
