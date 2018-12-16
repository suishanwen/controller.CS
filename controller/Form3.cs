using controller.util;
using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using System.Text.RegularExpressions;
using System.Threading;
using System.Windows.Forms;
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
        Dictionary<string, int> blackDictionary = new Dictionary<string, int>();
        private string voteProjectNameDroped;
        private string voteProjectNameDropedTemp;
        private string voteProjectNameGreen;
        private int downLoadCount;
        private bool isTop = true;
        private bool isAutoVote = false;
        private double filter = 0.1;
        private int clearBlackListHour;

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

        //委托 解决线程间操作dataGrid问题
        delegate void DelegateWindowText(String value);
        public void SetWindowText(String value)
        {
            if (this.InvokeRequired)
            {
                DelegateWindowText d = new DelegateWindowText(SetWindowText);
                this.Invoke(d, new object[] { value });
            }
            else
            {
                this.Text = value;
            }
        }
        public string WindowText
        {
            get
            {
                return Text;
            }
        }

        public Form3()
        {

        }
        public Form3(Form1 form1)
        {
            _mainForm = form1;
            InitializeComponent();
            string _isAutoVote = IniReadWriter.ReadIniKeys("Command", "isAutoVote", _mainForm.PathShare + "/CF.ini");
            if (!StringUtil.isEmpty(_isAutoVote) && _isAutoVote.Equals("1"))
            {
                isAutoVote = true;
                button2.Text = "取消自动";
            }
            timer1.Enabled = true;
            autoVote = new Thread(autoVoteSystem);
            autoVote.Start();
        }



        private bool isDropedProject(string project, int checkType)
        {
            voteProjectNameDroped = IniReadWriter.ReadIniKeys("Command", "voteProjectNameDroped", _mainForm.PathShare + "/AutoVote.ini");
            voteProjectNameDropedTemp = IniReadWriter.ReadIniKeys("Command", "voteProjectNameDropedTemp", _mainForm.PathShare + "/AutoVote.ini");
            if (checkType == 1 && voteProjectNameDroped != "")
            {
                string[] dropedProjectList = voteProjectNameDroped.Split('|');
                foreach (string dropedProject in dropedProjectList)
                {
                    if (project.IndexOf(dropedProject) != -1)
                    {
                        return true;
                    }
                }
            }
            if (checkType == 1 && voteProjectNameDropedTemp != "")
            {
                string[] dropedProjectList = voteProjectNameDropedTemp.Split('|');
                foreach (string dropedProject in dropedProjectList)
                {
                    if (project.IndexOf(dropedProject) != -1)
                    {
                        return true;
                    }
                }
            }
            return voteProjectNameDroped.IndexOf(project) != -1 && voteProjectNameDropedTemp.IndexOf(project) != -1;
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
            String isAdsl = IniReadWriter.ReadIniKeys("Command", "isAdsl", _mainForm.PathShare + "/CF.ini");
            HttpManager httpUtil = HttpManager.getInstance();
            string result = "";
            do
            {
                try
                {
                    result = httpUtil.requestHttpGet("http://butingzhuan.com/tasks.php?t=" + DateTime.Now.Millisecond.ToString(), "", "");
                    result = result.Substring(result.IndexOf("时间</td>"));
                    result = result.Substring(0, result.IndexOf("qzd_yj"));
                    result = result.Substring(result.IndexOf("<tr class='blank'>"));
                    result = result.Substring(0, result.LastIndexOf("<tr class='blank'>"));
                }
                catch (Exception)
                {
                    result = "";
                    Console.WriteLine("Request Fail!Retry in 10s...");
                    Log.writeLogs("./log.txt", "Request Fail!Retry in 10s...");
                    Thread.Sleep(10000);
                }
            } while (result == "");
            Regex regTR = new Regex(@"(?is)<tr[^>]*>(?:(?!</tr>).)*</tr>");
            Regex regTD = new Regex(@"(?is)<t[dh][^>]*>((?:(?!</td>).)*)</t[dh]>");
            MatchCollection mcTR = regTR.Matches(result);
            List<VoteProject> voteProjectList = new List<VoteProject>();
            foreach (Match mTR in mcTR)
            {
                if (!isDropedProject(mTR.Value, 1))
                {
                    if (mTR.Value.IndexOf("不换") != -1 && isAdsl != "1")
                    {
                        continue;
                    }
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
                                string text = innerTd.Substring(innerTd.IndexOf("(") + 1);
                                voteProject.Hot = int.Parse(text.Replace(")", "").Trim());
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
                                catch (Exception)
                                {
                                }
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
                                catch (Exception)
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
                                voteProject.RefreshDate = Convert.ToDateTime(DateTime.Now.Year + "-" + innerTd + ":00");
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
                if (activeVoteProject != null)
                {
                    if (activeVoteProject.Index != 0)
                    {
                        this.dataGridView1.ClearSelection();
                    }
                    if (activeVoteProject.Index != -1)
                    {
                        this.dataGridView1.CurrentCell = this.dataGridView1[0, activeVoteProject.Index];
                    }
                }
                this.dataGridView1.Refresh();
            }
        }

        private void voteProjectsAnalysis(List<VoteProject> voteProjectList)
        {
            voteProjectMonitorList.Clear();
            int i = -1;
            if (activeVoteProject != null)
            {
                activeVoteProject.Index = -1;

            }
            voteProjectList.Sort((a, b) => -a.Price.CompareTo(b.Price));
            foreach (VoteProject voteProject in voteProjectList)
            {
                //黑名单，价格过滤
                if (!isDropedProject(voteProject.ProjectName, 0) && voteProject.Price >= filter)
                {
                    i++;
                    voteProject.Index = i;
                    voteProject.setProjectType();
                    voteProjectMonitorList.Add(voteProject);
                    if (activeVoteProject != null && voteProject.ProjectName.Equals(activeVoteProject.ProjectName))
                    {
                        activeVoteProject = voteProject;
                    }
                }
            }
            //更新taskInfoDict
            TaskInfos.Update(voteProjectList);
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
                string projectName = IniReadWriter.ReadIniKeys("Command", "ProjectName", _mainForm.PathShare + "/AutoVote.ini");
                if(projectName != activeVoteProject.ProjectName)
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


        private void startVoteProject(VoteProject voteProject, bool onlyWaitOrder)
        {
            Console.WriteLine("projectName：" + voteProject.ProjectName + ",price：" + voteProject.Price + ",remains：" + voteProject.Remains);
            HttpManager httpManager = HttpManager.getInstance();
            string pathName = IniReadWriter.ReadIniKeys("Command", "Downloads", _mainForm.PathShare + "/CF.ini") + "\\" + voteProject.DownloadAddress.Substring(voteProject.DownloadAddress.LastIndexOf("/") + 1);
            string url = voteProject.DownloadAddress;
            string now = DateTime.Now.ToLocalTime().ToString();
            //Log.writeLogs("./log.txt", "开始下载:" + url);
            downLoadCount = 0;
            bool isDownloading = true;
            do
            {
                try
                {
                    downLoadCount++;
                    File.Delete(pathName);
                    httpManager.HttpDownloadFile(url, pathName);
                    isDownloading = false;
                }
                catch (Exception)
                {
                    Log.writeLogs("./log.txt", voteProject.ProjectName + "  下载异常，重新下载");
                    Thread.Sleep(10000);
                    if (downLoadCount >= 6)
                    {
                        Log.writeLogs("./log.txt", voteProject.ProjectName + "  下载异常6次，返回");
                        return;
                    }
                }
            } while (isDownloading);
            // Log.writeLogs("./log.txt", pathName + "  下载完成");
            Winrar.UnCompressRar(_mainForm.PathShare + "/投票项目/" + voteProject.ProjectName, IniReadWriter.ReadIniKeys("Command", "Downloads", _mainForm.PathShare + "/CF.ini"), voteProject.DownloadAddress.Substring(voteProject.DownloadAddress.LastIndexOf("/") + 1));
            if (voteProject.Type == "九天" &&
                !File.Exists(_mainForm.PathShare + "/投票项目/" + voteProject.ProjectName + "/启动九天.bat"))
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
            if (isAutoVote)
            {
                writeAutoVoteProject();
                setWorkerId();
            }
            _mainForm.VM3 = "";
            Log.writeLogs("./log.txt", "AutoVote: " + voteProject.ToString() + "    " + DateTime.Now.ToLocalTime().ToString());
            DirectoryInfo theFolder = new DirectoryInfo(_mainForm.PathShare + "/投票项目/" + voteProject.ProjectName);
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
            Dictionary<int, TaskInfo> vmInfo = TaskInfos.Get();
            for (int p = int.Parse(_mainForm.VM1); p <= int.Parse(_mainForm.VM2); p++)
            {
                String taskName = IniReadWriter.ReadIniKeys("Command", "TaskName" + p, _mainForm.PathShare + "/Task.ini");
                if (!onlyWaitOrder || taskName.Equals("待命"))
                {
                    if (vmInfo.ContainsKey(p))
                    {
                        if (voteProject.Price <= vmInfo[p].Price)
                        {
                            continue;
                        }
                        vmInfo[p] = new TaskInfo(voteProject.ProjectName, voteProject.Price);
                    }
                    else
                    {
                        vmInfo.Add(p, new TaskInfo(voteProject.ProjectName, voteProject.Price));
                    }
                    _mainForm.VM3 = p.ToString();
                    SwitchUtil.swichVm(_mainForm.VM1, _mainForm.VM2, _mainForm, _mainForm.PathShareVm + "\\投票项目\\" + voteProject.ProjectName + "\\" + executableFile.Name, "投票项目", _mainForm.PathShare);
                }
            }
            TaskInfos.Set(vmInfo);
        }

        private void testVoteProjectMonitorList()
        {
            for (int i = 0; i < voteProjectMonitorList.Count; i++)
            {
                VoteProject voteProject = voteProjectMonitorList[i];
                if (voteProject.Auto && voteProject.VoteRemains)
                {
                    startVoteProject(voteProject, true);
                    break;
                }
            }
        }

        private bool existWaitOrder()
        {
            bool result = false;
            string arrDrop = IniReadWriter.ReadIniKeys("Command", "ArrDrop", _mainForm.PathShare + "/CF.ini");
            for (int i = int.Parse(_mainForm.VM1); i <= int.Parse(_mainForm.VM2); i++)
            {
                if (!StringUtil.isEmpty(arrDrop))
                {
                    arrDrop = " " + arrDrop;
                }
                if (arrDrop.IndexOf(" " + i + " |") == -1)
                {
                    string taskName = IniReadWriter.ReadIniKeys("Command", "TaskName" + i, _mainForm.PathShare + "/Task.ini");
                    if (taskName.Equals("待命"))
                    {
                        result = true;
                        TaskInfos.Clear(i);
                    }
                }
            }
            return result;
        }

        private void generateBlackListTemp()
        {
            IniReadWriter.WriteIniKeys("Command", "voteProjectNameDropedTemp", "", _mainForm.PathShare + "/AutoVote.ini");
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
                    if (blackDictionary.ContainsKey(projectName))
                    {
                        blackDictionary[projectName] = blackDictionary[projectName]++;
                    }
                    else
                    {
                        blackDictionary.Add(projectName, 1);
                    }
                    //拉黑三次不再测试
                    if (blackDictionary[projectName] >= 3)
                    {
                        projectNameDroped += StringUtil.isEmpty(projectNameDroped) ? projectName : "|" + projectName;
                    }
                }
                IniReadWriter.WriteIniKeys("Command", "voteProjectNameDroped", projectNameDroped, _mainForm.PathShare + "/AutoVote.ini");
            }
        }

        private void testHighReward()
        {
            if (activeVoteProject != null && voteProjectMonitorList.Count > 0)
            {
                for (int i = 0; i < voteProjectMonitorList.Count; i++)
                {
                    VoteProject project = voteProjectMonitorList[i];
                    //价格更高
                    if (project.Price > activeVoteProject.Price && project.Auto && project.VoteRemains)
                    {
                        //排序更前 或 同项目价更高切换
                        if (i < activeVoteProject.Index || (activeVoteProject.ProjectName.Split('_')[0] == project.ProjectName.Split('_')[0]))
                        {
                            startVoteProject(project, false);
                        }
                    }
                }
            }
        }

        private void refreshWindowText()
        {
            int index = WindowText.IndexOf(" ");
            if (index != -1)
            {
                SetWindowText(WindowText.Substring(0, index) + " " + DateTime.Now.ToString());
            }
            else
            {
                SetWindowText(WindowText + " " + DateTime.Now.ToString());
            }
        }

        private void autoVoteSystem()
        {
            Log.writeLogs("./log.txt", "");
            Log.writeLogs("./log.txt", "AutoVoteSystem Thread Running");
            TaskInfos.Init(_mainForm.PathShare + "/AutoVote.ini");
            if (isAutoVote)
            {
                string projectName = IniReadWriter.ReadIniKeys("Command", "ProjectName", _mainForm.PathShare + "/AutoVote.ini");
                if (!StringUtil.isEmpty(projectName))
                {
                    try
                    {
                        double price = double.Parse(IniReadWriter.ReadIniKeys("Command", "Price", _mainForm.PathShare + "/AutoVote.ini"));
                        long remains = long.Parse(IniReadWriter.ReadIniKeys("Command", "Remains", _mainForm.PathShare + "/AutoVote.ini"));
                        string backgroundNo = IniReadWriter.ReadIniKeys("Command", "BackgroundNo", _mainForm.PathShare + "/AutoVote.ini");
                        activeVoteProject = new VoteProject(projectName, price, remains, backgroundNo);
                    }
                    catch (Exception e)
                    {
                        Log.writeLogs("./log.txt", "加载ActiveVoteProject异常:" + e.ToString());
                    }
                }

            }
            int count = 0;
            do
            {
                count++;
                try
                {
                    voteProjectsAnalysis(getVoteProjects());
                    if (isAutoVote)
                    {
                        if (DateTime.Now.Minute == 1 && DateTime.Now.Hour != clearBlackListHour)
                        {
                            clearBlackListHour = DateTime.Now.Hour;
                            Log.writeLogs("./log.txt", "Clear blackDictionary!");
                            blackDictionary.Clear();
                        }
                        if(count == 15)
                        {
                            generateBlackListTemp();
                        }
                        if (count > 30)
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
                    Thread.Sleep(20000);
                }
                catch (Exception e)
                {
                    Console.WriteLine(e.ToString());
                    Log.writeLogs("./log.txt", e.ToString());
                }
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
                    if (_mainForm.CheckBox3.Checked && !StringUtil.isEmpty(_mainForm.OverSwitchPath))
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
                string refreshD = index != -1 ? this.Text.Substring(this.Text.IndexOf(" ")) : "";
                SetWindowText("实时监控(" + voteProjectMonitorList.Count + ")" + refreshD);
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

        private void button2_Click(object sender, EventArgs e)
        {
            isAutoVote = !isAutoVote;
            IniReadWriter.WriteIniKeys("Command", "isAutoVote", isAutoVote ? "1" : "0", _mainForm.PathShare + "/CF.ini");
            button2.Text = isAutoVote ? "取消自动" : "开启自动";
        }
    }
}
