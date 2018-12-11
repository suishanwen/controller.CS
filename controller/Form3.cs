using controller.util;
using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using System.Text.RegularExpressions;
using System.Threading;
using System.Windows.Forms;
using System.Data;
using System.ComponentModel;

namespace controller
{
    public partial class Form3 : Form
    {
        private static Form1 _mainForm;
        private int count;
        private int val;
        private int over;
        private int kick;
        public string arrDrop;
        private string activeVm;
        private Thread autoVote;
        private VoteProject activeVoteProject;
        private List<VoteProject> voteProjectMonitorList = new List<VoteProject>();
        Dictionary<string,int> blackDictionary = new Dictionary<string,int >();
        private string voteProjectNameDroped;
        private string voteProjectNameGreen;
        private int downLoadCount;
        private bool isTop = true;
        private bool isAutoVote = false;
        private double filter = 0.1;

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

        public DataGridView DataGridView
        {
            get
            {
                return dataGridView1;
            }
        }

        public string  WindowText
        {
            get
            {
                return Text;
            }
            set
            {
                 Text = value;
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
            autoVote = new Thread(autoVoteSystem);
            autoVote.Start();
        }



        private bool isDropedProject(string project, int checkType)
        {
            voteProjectNameDroped = IniReadWriter.ReadIniKeys("Command", "voteProjectNameDroped", _mainForm.PathShare + "/AutoVote.ini");
            if (checkType == 1 && voteProjectNameDroped!="")
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
                    result = httpUtil.requestHttpGet("http://butingzhuan.com/tasks.php?t="+ DateTime.Now.Millisecond.ToString(), "", "");
                }
                catch (Exception e)
                {
                    Console.WriteLine("Request Fail!Retry in 10s...");
                    Log.writeLogs("./log.txt", "Request Fail!Retry in 10s...");
                    Thread.Sleep(10000);
                }
            } while (result == "");
            result = result.Substring(result.IndexOf("时间</td>"));
            result = result.Substring(0, result.IndexOf("qzd_yj"));
            result = result.Substring(result.IndexOf("<tr class='blank'>"));
            result = result.Substring(0, result.LastIndexOf("<tr class='blank'>"));
            if (DateTime.Now.Minute % 30 == 0)
            {
                Log.writeLogs("./log.txt", "AutoVote: Keep Alive! Finished Request!     "+ DateTime.Now.ToString());
            }
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
                            case 3:
                                string text = innerTd.Substring(innerTd.IndexOf("(")+1);
                                voteProject.Hot = int.Parse(text.Replace(")","").Trim());
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
                                voteProject.RefreshDate = Convert.ToDateTime(DateTime.Now.Year+"-" + innerTd + ":00");
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
        
        //委托 解决线程间操作dataGrid问题
        delegate void SetDataGridView(List<VoteProject> voteProjectList);

        private void SetDataGrid(List<VoteProject> voteProjectList)
        {
            if (this.dataGridView1.InvokeRequired)
            {
                SetDataGridView d = new SetDataGridView(SetDataGrid);
                this.Invoke(d, new object[] { voteProjectList });
            }
            else
            {
                this.dataGridView1.DataSource = new BindingList<VoteProject>(voteProjectList);
                this.dataGridView1.Refresh();
            }
        }
        private void voteProjectsAnalysis(List<VoteProject> voteProjectList)
        {
            voteProjectMonitorList.Clear();
            foreach (VoteProject voteProject in voteProjectList)
            {
                //不存在于黑名单，并且是九天项目
                if (!isDropedProject(voteProject.ProjectName, 0) && voteProject.Price>= filter && voteProject.BackgroundAddress.IndexOf("http://www.jiutianvote.cn") != -1)
                {
                    voteProjectMonitorList.Add(voteProject);
                }
            }
            SetDataGrid(voteProjectMonitorList);
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


        private void startVoteProject(VoteProject voteProject,bool onlyWaitOrder)
        {
            Console.WriteLine("projectName：" + voteProject.ProjectName + ",price：" + voteProject.Price + ",remains：" + voteProject.Remains);
            HttpManager httpManager = HttpManager.getInstance();
            string pathName = IniReadWriter.ReadIniKeys("Command", "Downloads", _mainForm.PathShare + "/CF.ini") + "\\" + voteProject.DownloadAddress.Substring(voteProject.DownloadAddress.LastIndexOf("/") + 1);
            string url = voteProject.DownloadAddress;
            string now = DateTime.Now.ToLocalTime().ToString();
            Log.writeLogs("./log.txt", "开始下载:" + url);
            downLoadCount = 0;
            bool isDownloading = true;
            do
            {
                try
                {
                    File.Delete(pathName);
                    httpManager.HttpDownloadFile(url, pathName);
                    isDownloading = false;
                }
                catch (Exception)
                {
                    Log.writeLogs("./log.txt", voteProject.ProjectName + "  下载异常，重新下载");
                    Thread.Sleep(1000);
                }
            } while (isDownloading);
            Log.writeLogs("./log.txt", pathName + "  下载完成");
            Winrar.UnCompressRar(_mainForm.PathShare + "/投票项目/" + voteProject.ProjectName, IniReadWriter.ReadIniKeys("Command", "Downloads", _mainForm.PathShare + "/CF.ini"), voteProject.DownloadAddress.Substring(voteProject.DownloadAddress.LastIndexOf("/") + 1));
            if (!File.Exists(_mainForm.PathShare + "/投票项目/" + voteProject.ProjectName + "/启动九天.bat"))
            {
                String[] Lines = { @"start vote.exe" };
                File.WriteAllLines(_mainForm.PathShare + "/投票项目/" + voteProject.ProjectName + "/启动九天.bat", Lines, Encoding.GetEncoding("GBK"));
            }
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
            if (isAutoVote)
            {
                setWorkerId();
            }
            _mainForm.VM3 = "";
            Log.writeLogs("./log.txt", "AutoVote: " + voteProject.ProjectName + " " + voteProject.BackgroundNo + "    " + DateTime.Now.ToLocalTime().ToString());

            for (int p = int.Parse(_mainForm.VM1); p <= int.Parse(_mainForm.VM2); p++)
            {
                String TaskName = IniReadWriter.ReadIniKeys("Command", "TaskName" + p, _mainForm.PathShare + "/Task.ini");
                if (!onlyWaitOrder || TaskName.Equals("待命"))
                {
                    _mainForm.VM3 = p.ToString();
                    SwitchUtil.swichVm(_mainForm.VM1, _mainForm.VM2, _mainForm, _mainForm.PathShareVm + "\\投票项目\\" + voteProject.ProjectName + "\\vote.exe", "投票项目", _mainForm.PathShare);
                }
            }
        }

        private void testVoteProjectMonitorList()
        {
            for (int i = 0; i < voteProjectMonitorList.Count; i++)
            {
                VoteProject voteProject = voteProjectMonitorList[i];
                if (voteProject.Remains > 0 && (voteProject.Remains * voteProject.Price) > 100 && !voteProject.IsRestrict)
                {
                    startVoteProject(voteProject, true);
                    break;
                }
            }
        }

        private bool existWaitOrder()
        {
            string arrDrop= IniReadWriter.ReadIniKeys("Command", "ArrDrop", _mainForm.PathShare + "/CF.ini");
            for (int i = int.Parse(_mainForm.VM1); i <= int.Parse(_mainForm.VM2); i++)
            {
                if (!StringUtil.isEmpty(arrDrop))
                {
                    arrDrop = " " + arrDrop;
                }
                if (arrDrop.IndexOf(" "+i+" |") == -1)
                {
                    string taskName = IniReadWriter.ReadIniKeys("Command", "TaskName" + i, _mainForm.PathShare + "/Task.ini");
                    if (taskName.Equals("待命"))
                    {
                        return true;
                    }
                }
            }
            return false;
        }

        private void generateBlackList()
        {
            voteProjectNameDroped = IniReadWriter.ReadIniKeys("Command", "voteProjectNameDroped", _mainForm.PathShare + "/AutoVote.ini");
            if (voteProjectNameDroped != "")
            {
                string projectNameDroped = "";
                string[] dropedProjectList = voteProjectNameDroped.Split('|');
                foreach (String projectName in dropedProjectList)
                {
                    int times = 1;
                    if (blackDictionary.ContainsKey(projectName))
                    {
                        times = blackDictionary[projectName]++;
                    }
                    blackDictionary.Add(projectName, times);
                    //拉黑三次不再测试
                    if (times >= 3)
                    {
                        projectNameDroped += StringUtil.isEmpty(projectNameDroped) ? projectName : "|" + projectName;
                    }
                }
                IniReadWriter.WriteIniKeys("Command", "voteProjectNameDroped", projectNameDroped, _mainForm.PathShare + "/AutoVote.ini");
            }
        }


        private void testHighReward()
        {
            if (activeVoteProject != null)
            {
                foreach (VoteProject project in voteProjectMonitorList)
                {
                    if (project.ProjectName.Equals(activeVoteProject.ProjectName))
                    {
                        break;
                    }
                    if (project.Price > activeVoteProject.Price)
                    {
                        startVoteProject(project, false);
                    }

                }
            }
        }

        private void refreshWindowText()
        {
            int index = WindowText.IndexOf(" ");
            if (index != -1)
            {
                WindowText = WindowText.Substring(0, index) + " " + DateTime.Now.ToString();
            }
            else
            {
                WindowText = WindowText + " " + DateTime.Now.ToString();
            }
        }

        private void autoVoteSystem()
        {
            Log.writeLogs("./log.txt", "");
            Log.writeLogs("./log.txt", "AutoVoteSystem Thread Running");
            string _isAutoVote = IniReadWriter.ReadIniKeys("Command", "isAutoVote", _mainForm.PathShare + "/CF.ini");
            if (!StringUtil.isEmpty(_isAutoVote) && _isAutoVote.Equals("1"))
            {
                isAutoVote = true;
            }
            int count = 0;
            do
            {
                count++;
                voteProjectsAnalysis(getVoteProjects());
                if (isAutoVote) {
                    if (DateTime.Now.Hour  % 6 == 0 && DateTime.Now.Minute == 1)
                    {
                        Log.writeLogs("./log.txt", "Clear blackDictionary!");
                        blackDictionary.Clear();
                    }
                    if (count > 10)
                    {
                        count = 0;
                        generateBlackList();
                    }
                    if (existWaitOrder())
                    {

                        testVoteProjectMonitorList();
                    }
                    else
                    {
                        testHighReward();
                    }
                }
                refreshWindowText();
                Thread.Sleep(30000);
            }
            while (true);
        }

        private void timer1_Tick(object sender, EventArgs e)
        {
            count++;
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
            else if (count == 3)
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
                    if (_mainForm.CheckBox3.Checked&& !StringUtil.isEmpty(_mainForm.OverSwitchPath))
                    {
                        if (_mainForm.OverSwitchPath.Equals("HANGUP"))
                        {
                            SwitchUtil.swichVm(_mainForm.VM1, _mainForm.VM2, _mainForm, "", IniReadWriter.ReadIniKeys("Command", "Hangup", _mainForm.PathShare + "/CF.ini"), _mainForm.PathShare);
                        }
                        else
                        {
                            SwitchUtil.swichVm(_mainForm.VM1, _mainForm.VM2, _mainForm, _mainForm.OverSwitchPath, "投票项目", _mainForm.PathShare);
                        }
                        _mainForm.CheckBox3.Checked = false;
                    }
                }
                if (kick > 0)
                {
                    IniReadWriter.WriteIniKeys("Command", "KICK", "0", _mainForm.PathShare + "/CF.ini");
                    _mainForm.NotifyIcon1.ShowBalloonTip(0, "项目限人", DateTime.Now.ToLocalTime().ToString(), ToolTipIcon.Info);
                }

            }
            else if (count == 5)
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
                int index = this.Text.IndexOf(" ");
                string refreshD = index != -1 ? this.Text.Substring(this.Text.IndexOf(" ")) :"";
                this.Text = "实时监控(" + voteProjectMonitorList.Count + ")"+ refreshD;
            }
        }


        private void button1_Click(object sender, EventArgs e)
        {
            isTop = !isTop;
            TopMost = isTop;
            button1.Text = TopMost ? "取消置顶" : "置顶";
        }

        private void dataGridView1_DoubleClick(object sender, EventArgs e)
        {
            if (dataGridView1.SelectedRows.Count > 0)
            {
                int index = dataGridView1.SelectedRows[0].Index;
                startVoteProject(VoteProjectMonitorList[index], false);
            }
        }

        private void textBox1_TextChanged(object sender, EventArgs e)
        {
            if (!StringUtil.isEmpty(textBox1.Text))
            {
                filter = double.Parse(textBox1.Text);
            }
        }
    }
}
