﻿using controller.util;
using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using System.Text.RegularExpressions;
using System.Threading;
using System.Windows.Forms;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using robot.core;

namespace controller
{
    public partial class Form3 : Form
    {
        private static Form3 _form3;
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
        private double blackRate = 1;
        private bool overAuto = false;
        private int clearBlackListHour;
        private int statisticDay;
        private string dataSource;
        private string isAdsl;
        private string email;


        public static List<VoteProject> VoteProjectMonitorList => _form3.voteProjectMonitorList;

        public static bool IsAutoVote
        {
            get { return _form3.isAutoVote; }
            set { _form3.isAutoVote = value; }
        }
        public static bool IsOverAuto
        {
            get { return _form3.overAuto; }
        }
        public static string ActiveVm
        {
            get { return _form3.activeVm; }
            set { _form3.activeVm = value; }
        }
        public Thread AutoVote
        {
            get { return autoVote; }
        }
        public DataGridView DataGridView
        {
            get { return dataGridView1; }
        }

        //委托 解决线程间操作问题
        delegate void DelegateAction(int type, string val = "");

        public static void DlAction(int type, string val = "")
        {
            if (_form3.InvokeRequired)
            {
                DelegateAction d = new DelegateAction(DlAction);
                _form3.Invoke(d, new object[] { type, val });
            }
            else
            {
                switch (type)
                {
                    case 1:
                        Form3.IsAutoVote = !Form3.IsAutoVote;
                        IniReadWriter.WriteIniKeys("Command", "isAutoVote", Form3.IsAutoVote ? "1" : "0",
                            _mainForm.PathShare + "/CF.ini");
                        _form3.button2.Text = Form3.IsAutoVote ? "取消自动" : "开启自动";
                        _form3.button3.Visible = !Form3.IsAutoVote;
                        break;
                    case 2:
                        _form3.overAuto = !_form3.overAuto;
                        _form3.button3.Text = _form3.overAuto ? "撤销" : "到票自动";
                        break;
                }

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
            get { return Text; }
        }

        //委托 解决线程间操作textBox4问题
        delegate void SetProNameDelegate(string value);

        public static void SetProName(string value)
        {
            if (_form3.progressBar1.InvokeRequired)
            {
                SetProNameDelegate d = new SetProNameDelegate(SetProName);
                _form3.Invoke(d, new object[] { value });
            }
            else
            {
                _form3.label8.Text = value;
            }
        }

        //委托 解决线程间操作textBox4问题
        delegate void SetProgressDelegate(int value);

        public static void SetProgress(int value)
        {
            if (_form3.progressBar1.InvokeRequired)
            {
                SetProgressDelegate d = new SetProgressDelegate(SetProgress);
                _form3.Invoke(d, new object[] { value });
            }
            else
            {
                if (value == 100)
                {
                    _form3.progressBar1.Visible = false;
                }
                else
                {
                    if (_form3.progressBar1.Visible == false)
                    {
                        _form3.progressBar1.Visible = true;
                    }

                    _form3.progressBar1.Value = value;
                }
            }
        }


        public Form3()
        {
        }

        public Form3(Form1 form1)
        {
            _mainForm = form1;
            _form3 = this;
            InitializeComponent();
            dataSource = IniReadWriter.ReadIniKeys("Command", "dataSource", _mainForm.PathShare + "/AutoVote.ini");
            string _isAutoVote = IniReadWriter.ReadIniKeys("Command", "isAutoVote", _mainForm.PathShare + "/CF.ini");
            email = IniReadWriter.ReadIniKeys("Command", "email", _mainForm.PathShare + "/CF.ini");
            isAdsl = IniReadWriter.ReadIniKeys("Command", "isAdsl", _mainForm.PathShare + "/CF.ini");
            try
            {
                filter = double.Parse(IniReadWriter.ReadIniKeys("Command", "filter",
                    _mainForm.PathShare + "/AutoVote.ini"));
                blackRate = double.Parse(IniReadWriter.ReadIniKeys("Command", "blackRate",
                    _mainForm.PathShare + "/AutoVote.ini"));
            }
            catch (Exception)
            {
            }
            textBox1.Text = filter.ToString();
            textBox2.Text = IniReadWriter.ReadIniKeys("Command", "maxKb", _mainForm.PathShare + "/CF.ini");
            textBox3.Text = blackRate.ToString();
            button4.Text = dataSource.Equals("NEW") ? "服" : "网";
            button5.Text = isAdsl.Equals("1") ? "主" : "虚";
            if (!StringUtil.isEmpty(_isAutoVote) && _isAutoVote.Equals("1"))
            {
                isAutoVote = true;
                button2.Text = "取消自动";
                button3.Visible = false;
            }

            timer1.Enabled = true;
            autoVote = new Thread(autoVoteSystem);
            autoVote.Start();
        }

        private bool isTopedProject(string project)
        {
            string voteProjectNameToped = IniReadWriter
                .ReadIniKeys("Command", "voteProjectNameToped", _mainForm.PathShare + "/AutoVote.ini").Trim();
            if (StringUtil.isEmpty(voteProjectNameToped))
            {
                return false;
            }

            string[] topedProjectList = voteProjectNameToped.Split('|');
            foreach (string topedProject in topedProjectList)
            {
                if (StringUtil.isEmpty(topedProject))
                {
                    continue;
                }

                if (project.IndexOf(topedProject) != -1)
                {
                    return true;
                }
            }

            return false;
        }

        private bool isStratageDroped(VoteProject voteProject)
        {
            string blackStratage = IniReadWriter.ReadIniKeys("Command", "blackStratage",
                _mainForm.PathShare + "/AutoVote.ini");
            if (!StringUtil.isEmpty(blackStratage))
            {
                string[] arr = blackStratage.Split('|');
                foreach (string infos in arr)
                {
                    string[] info = infos.Split('-');
                    string name = info[0];
                    double price = 0;
                    try
                    {
                        price = double.Parse(info[1]);
                    }
                    catch (Exception) { };
                    if (voteProject.ProjectName.ToLower().IndexOf(name.ToLower()) != -1 &&
                        voteProject.Price < price)
                    {
                        return true;
                    }
                }
            }
            return false;
        }

        /**
         * 是否拉黑    checkType    0 默认 1 全项目匹配
         * 
         */
        private bool isDropedProject(VoteProject voteProject, int checkType)
        {
            if (isStratageDroped(voteProject))
            {
                return true;
            }
            var project = voteProject.ProjectName;
            voteProjectNameDroped = IniReadWriter.ReadIniKeys("Command", "voteProjectNameDroped",
                _mainForm.PathShare + "/AutoVote.ini");
            voteProjectNameDropedTemp = IniReadWriter.ReadIniKeys("Command", "voteProjectNameDropedTemp",
                _mainForm.PathShare + "/AutoVote.ini");
            if (checkType == 1 && voteProjectNameDroped != "")
            {
                string[] dropedProjectList = voteProjectNameDroped.Split('|');
                var projectPrefix = project;
                if (project.IndexOf("_") > 0)
                {
                    projectPrefix = project.Substring(0, project.IndexOf("_"));
                }

                var sameProjectDropCount = 0;
                foreach (var dropedProject in dropedProjectList)
                {
                    if (StringUtil.isEmpty(dropedProject))
                    {
                        continue;
                    }

                    if (project.IndexOf(dropedProject) != -1)
                    {
                        return true;
                    }

                    if (dropedProject.IndexOf(projectPrefix) != -1)
                    {
                        sameProjectDropCount++;
                    }
                }

                if (sameProjectDropCount >= 3)
                {
                    voteProject.RelDrop = true;
                    return true;
                }
            }

            return voteProjectNameDroped.IndexOf(project) != -1 || voteProjectNameDropedTemp.IndexOf(project) != -1;
        }

        private bool isGreenProject(string project, int checkType)
        {
            voteProjectNameGreen =
                IniReadWriter.ReadIniKeys("Command", "voteProjectNameGreen", _mainForm.PathShare + "/AutoVote.ini");
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

        private string getJsonVal(string json, string name)
        {
            json = json.Substring(json.IndexOf($"{name}:"));
            int index = json.IndexOf(",");
            if (index == -1)
            {
                return json.Substring(name.Length + 1, json.IndexOf("}") - (name.Length + 1));
            }

            return json.Substring(name.Length + 1, index - (name.Length + 1));
        }

        private string getIdentify()
        {
            string id = IniReadWriter.ReadIniKeys("Command", "worker", _mainForm.PathShare + "/CF.ini");
            return $"{id}-{Form1.GetVMS()}";
        }

        private string getInfo()
        {
            string info = "";
            if (!StringUtil.isEmpty(arrDrop))
            {
                info = $"&arrDrop=掉线:{arrDrop}";
            }
            else
            {
                if (!label4.Text.Equals("无"))
                {
                    info = $"&arrDrop={label4.Text}";
                }
            }
            return info;
        }

        private List<VoteProject> getVoteProjects()
        {
            HttpManager httpUtil = HttpManager.getInstance();
            string result = "";
            do
            {
                try
                {

                    result = httpUtil.requestHttpGet("http://bitcoinrobot.cn:8000", "/voteInfo/", $"isAdsl={isAdsl}&id={getIdentify()}{getInfo()}", "utf-8");
                }
                catch (Exception)
                {
                    result = "";
                    Console.WriteLine("Request Fail!Retry in 5s...");
                    Log.writeLogs("./log.txt", "Request Fail!Retry in 5s...");
                    Thread.Sleep(5000);
                }
            } while (result == "" || result == "timeout");

            List<VoteProject> voteProjectList = new List<VoteProject>();
            string pat = @"(\[).*?(\])";
            Match matched = Regex.Match(result.Replace("\"", ""), pat, RegexOptions.IgnoreCase);
            pat = @"(\{).*?(\})";
            MatchCollection matches = Regex.Matches(matched.Value.Replace("[", "")
                .Replace("]", ""), pat, RegexOptions.IgnoreCase);
            foreach (Match m in matches)
            {
                string json = m.Value;
                VoteProject voteProject = new VoteProject();
                voteProject.ProjectName = getJsonVal(json, "projectName");
                string hot = getJsonVal(json, "hot");
                if (!StringUtil.isEmpty(hot))
                {
                    voteProject.Hot = int.Parse(hot);
                }

                string price = getJsonVal(json, "price");
                if (!StringUtil.isEmpty(price))
                {
                    voteProject.Price = double.Parse(price);
                }

                string finish = getJsonVal(json, "finishQuantity");
                if (!StringUtil.isEmpty(finish))
                {
                    voteProject.FinishQuantity = long.Parse(finish);
                }

                string require = getJsonVal(json, "totalRequire");
                if (!StringUtil.isEmpty(require))
                {
                    voteProject.TotalRequire = long.Parse(require);
                }

                string remains = getJsonVal(json, "remains");
                if (!StringUtil.isEmpty(remains))
                {
                    voteProject.Remains = long.Parse(remains);
                }

                voteProject.BackgroundAddress = getJsonVal(json, "backgroundAddress");
                voteProject.DownloadAddress = getJsonVal(json, "downloadAddress");
                voteProject.IdType = getJsonVal(json, "idType");
                voteProject.BackgroundNo = getJsonVal(json, "backgroundNo");
                voteProject.RefreshDate =
                    Convert.ToDateTime(DateTime.Now.Year + "-" + getJsonVal(json, "refreshDate") + ":00");
                if (voteProject.Price >= filter)
                {
                    voteProjectList.Add(voteProject);
                }
            }

            return voteProjectList;
        }


        private List<VoteProject> getVoteProjectsBT()
        {
            HttpManager httpUtil = HttpManager.getInstance();
            string result = "";
            do
            {
                try
                {
                    result = httpUtil.requestHttpGet(
                        "http://butingzhuan.com", "/tasks.php", "t=" + DateTime.Now.Millisecond.ToString(), "gbk");
                    result = result.Substring(result.IndexOf("时间</td>"));
                    result = result.Substring(0, result.IndexOf("qzd_yj"));
                    result = result.Substring(result.IndexOf("<tr class='blank'>"));
                    result = result.Substring(0, result.LastIndexOf("<tr class='blank'>"));
                }
                catch (Exception)
                {
                    result = "";
                    Console.WriteLine($"Request Fail!Retry in 5s...");
                    Log.writeLogs("./log.txt", $"Request Fail!Retry in 5s...");
                    Thread.Sleep(5000);
                }
            } while (result == "");

            Regex regTR = new Regex(@"(?is)<tr[^>]*>(?:(?!</tr>).)*</tr>");
            Regex regTD = new Regex(@"(?is)<t[dh][^>]*>((?:(?!</td>).)*)</t[dh]>");
            MatchCollection mcTR = regTR.Matches(result);
            List<VoteProject> voteProjectList = new List<VoteProject>();
            foreach (Match mTR in mcTR)
            {
                //if (!isDropedProject(mTR.Value, 1))
                if (true)
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

                                    voteProject.TotalRequire =
                                        long.Parse(quantityInfo[1].Substring(0, quantityInfo[1].IndexOf(" ")));
                                }
                                catch (Exception)
                                {
                                }

                                break;
                            case 8:
                                voteProject.BackgroundAddress = HtmlMatch.GetAttr(innerTd, "a", "href");
                                break;
                            case 9:
                                voteProject.DownloadAddress = HtmlMatch.GetAttr(innerTd.Replace(" ", ""), "a", "href");
                                break;
                            case 10:
                                try
                                {
                                    voteProject.IdType = HtmlMatch.GetAttr(innerTd, "input", "value").Substring(0, 2);
                                }
                                catch (Exception)
                                {
                                    if (innerTd == null || innerTd.IndexOf("AQ-") != -1)
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

                    if (voteProject.Price >= filter)
                    {
                        voteProjectList.Add(voteProject);
                    }
                }
            }

            return voteProjectList;
        }

        public List<VoteProject> GetVoteProjectsServer()
        {
            List<VoteProject> voteProjects = new List<VoteProject>();
            try
            {
                var prefix = "https://bitcoinrobot.cn/api";
                var path = "/voteProject/query";
                string json = HttpManager.getInstance().requestHttpPost(prefix, path, "");
                List<VoteProject> votes = JsonUtil.DeserializeJsonToList<VoteProject>(json);
                votes.ForEach(voteProject =>
                {
                    if (voteProject.Price >= filter && voteProject.IpDial)
                    {
                        voteProjects.Add(voteProject);
                    }
                });
                return voteProjects;
            }
            catch (ThreadInterruptedException e)
            {
                throw e;
            }
            catch (Exception)
            {
                Thread.Sleep(2000);
                return GetVoteProjectsServer();
            }
        }

        //委托 解决线程间操作dataGrid问题
        delegate void SetSelectedDataGridView(int index);

        public static void SetSelectedDataGrid(int index)
        {
            if (_form3.dataGridView1.InvokeRequired)
            {
                SetSelectedDataGridView d = new SetSelectedDataGridView(SetSelectedDataGrid);
                _form3.Invoke(d, new object[] { index });
            }
            else
            {
                _form3.dataGridView1.CurrentCell = _form3.dataGridView1[0, index];
                _form3.dataGridView1.Rows[index].Selected = true;
            }
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
                        SocketAction.AUTO_VOTE_SELECT_INDEX(activeVoteProject.Index);
                    }
                }
                this.dataGridView1.Refresh();
            }
        }


        private void voteProjectsAnalysis(List<VoteProject> voteProjectList)
        {
            voteProjectMonitorList.Clear();
            foreach (VoteProject voteProject in voteProjectList)
            {
                voteProject.Drop = isDropedProject(voteProject, 1);
                voteProject.Top = isTopedProject(voteProject.ProjectName);
                voteProject.setProjectType();
            }

            voteProjectList.Sort((a, b) =>
            {
                if (a.Top && !b.Top)
                {
                    return -1;
                }
                else if (!a.Top && b.Top)
                {
                    return 1;
                }
                else
                {
                    return -a.Price.CompareTo(b.Price);
                }
            });
            if (activeVoteProject != null)
            {
                activeVoteProject.Index = -1;
            }

            for (int i = 0; i < voteProjectList.Count; i++)
            {
                VoteProject voteProject = voteProjectList[i];
                voteProject.Index = i;
                voteProjectMonitorList.Add(voteProject);
                if (activeVoteProject != null && voteProject.ProjectName.Equals(activeVoteProject.ProjectName))
                {
                    activeVoteProject = voteProject;
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
                string projectName =
                    IniReadWriter.ReadIniKeys("Command", "ProjectName", _mainForm.PathShare + "/AutoVote.ini");
                if (projectName != activeVoteProject.ProjectName)
                {
                    IniReadWriter.WriteIniKeys("Command", "ProjectName", activeVoteProject.ProjectName,
                        _mainForm.PathShare + "/AutoVote.ini");
                    IniReadWriter.WriteIniKeys("Command", "Price", activeVoteProject.Price.ToString(),
                        _mainForm.PathShare + "/AutoVote.ini");
                    IniReadWriter.WriteIniKeys("Command", "TotalRequire", activeVoteProject.TotalRequire.ToString(),
                        _mainForm.PathShare + "/AutoVote.ini");
                    IniReadWriter.WriteIniKeys("Command", "FinishQuantity", activeVoteProject.FinishQuantity.ToString(),
                        _mainForm.PathShare + "/AutoVote.ini");
                    IniReadWriter.WriteIniKeys("Command", "Remains", activeVoteProject.Remains.ToString(),
                        _mainForm.PathShare + "/AutoVote.ini");
                    IniReadWriter.WriteIniKeys("Command", "BackgroundNo", activeVoteProject.BackgroundNo,
                        _mainForm.PathShare + "/AutoVote.ini");
                    IniReadWriter.WriteIniKeys("Command", "BackgroundAddress", activeVoteProject.BackgroundAddress,
                        _mainForm.PathShare + "/AutoVote.ini");
                    IniReadWriter.WriteIniKeys("Command", "DownloadAddress", activeVoteProject.DownloadAddress,
                        _mainForm.PathShare + "/AutoVote.ini");
                    IniReadWriter.WriteIniKeys("Command", "IsRestrict", activeVoteProject.IsRestrict.ToString(),
                        _mainForm.PathShare + "/AutoVote.ini");
                    IniReadWriter.WriteIniKeys("Command", "IdType", activeVoteProject.IdType,
                        _mainForm.PathShare + "/AutoVote.ini");
                    IniReadWriter.WriteIniKeys("Command", "RefreshDate",
                        activeVoteProject.RefreshDate.ToLocalTime().ToString(), _mainForm.PathShare + "/AutoVote.ini");
                    IniReadWriter.WriteIniKeys("Command", "dropVote", "0", _mainForm.PathShare + "/AutoVote.ini");
                }
            }
        }

        private void setWorkerId(string idType)
        {
            string[] user1 = { "TX-111", "Q7-129", "AQ-239356" };
            string[] user2 = { "TX-18", "Q7-43", "AQ-14" };
            string[][] users = { user1, user2 };
            string worker = "";
            try
            {
                worker = IniReadWriter.ReadIniKeys("Command", "worker", Form1.GetPathShare() + "/CF.ini").ToUpper();
            }
            catch (Exception e)
            {
            }
            int userIndex = worker.IndexOf(user2[0]) != -1 || worker.IndexOf(user2[1]) != -1 || worker.IndexOf(user2[2]) != -1 ? 1 : 0;
            int idIndex = 0;
            if ("Q7".Equals(idType))
            {
                idIndex = 1;
            }
            else if ("AQ".Equals(idType))
            {
                idIndex = 2;
            }
            string adaptedWorker = users[userIndex][idIndex];
            //自定义工号结尾
            string fix = worker.Replace(users[userIndex][0], "").Replace(users[userIndex][1], "").Replace(users[userIndex][2], "");
            IniReadWriter.WriteIniKeys("Command", "worker", adaptedWorker + fix, Form1.GetPathShare() + "/CF.ini");
            IniReadWriter.WriteIniKeys("Command", "printgonghao", "1", Form1.GetPathShare() + "/CF.ini");
        }

        private void addVoteProjectDroped(string projectName)
        {
            string voteProjectNameDroped = IniReadWriter.ReadIniKeys("Command", "voteProjectNameDroped", _mainForm.PathShare + "/AutoVote.ini");
            Log.writeLogs("./log.txt", $"{projectName}拉黑{blackRate * 30}分钟");
            voteProjectNameDroped +=
                StringUtil.isEmpty(voteProjectNameDroped) ? projectName : "|" + projectName;
            IniReadWriter.WriteIniKeys("Command", "voteProjectNameDroped", voteProjectNameDroped, _mainForm.PathShare + "/AutoVote.ini");

        }


        private void startVoteProject(VoteProject voteProject, bool onlyWaitOrder)
        {
            Console.WriteLine("projectName：" + voteProject.ProjectName + ",price：" + voteProject.Price + ",remains：" +
                              voteProject.Remains);
            Dictionary<int, TaskInfo> vmInfo = TaskInfos.Get();
            //先待命
            for (int p = int.Parse(Form1.VM1); p <= int.Parse(Form1.VM2); p++)
            {
                string taskName =
                    IniReadWriter.ReadIniKeys("Command", "TaskName" + p, _mainForm.PathShare + "/Task.ini");
                if (!onlyWaitOrder || taskName.Equals("待命"))
                {
                    if (vmInfo.ContainsKey(p))
                    {
                        if (isAutoVote && voteProject.Price <= vmInfo[p].Price)
                        {
                            continue;
                        }

                    }
                    SwitchUtil.SetVmState(p, "", SocketAction.TASK_SYS_WAIT_ORDER, _mainForm.PathShare);
                }
            }
            string fileName = voteProject.DownloadAddress.Substring(voteProject.DownloadAddress.LastIndexOf("/") + 1);
            string pathName = IniReadWriter.ReadIniKeys("Command", "Downloads", _mainForm.PathShare + "/CF.ini") +
                              "\\" + fileName;
            string url = voteProject.DownloadAddress;
            bool newVersion = HttpDownLoad.CheckVersion(url, pathName);
            if (newVersion || !Directory.Exists(_mainForm.PathShare + "/投票项目/" + voteProject.ProjectName))
            {
                string now = DateTime.Now.ToLocalTime().ToString();
                Form3.SetProName(voteProject.ProjectName);
                if (dataSource.Equals("NEW"))
                {
                    string checkUrl = $"http://bitcoinrobot.cn:8000";
                    HttpManager httpUtil = HttpManager.getInstance();
                    string re = "";
                    int err = 0;
                    do
                    {
                        try
                        {
                            re = httpUtil.requestHttpGet(checkUrl, "/download/", $"url={url}", "utf-8");
                            if (re.Equals("err"))
                            {
                                err++;
                            }
                            if (err > 5)
                            {
                                Log.writeLogs("./log.txt", "下载失败5次，拉黑...");
                                addVoteProjectDroped(voteProject.ProjectName);
                                return;
                            }
                        }
                        catch (Exception)
                        {
                            re = "";
                            Console.WriteLine("Request Download Fail!Retry in 5...");
                            Log.writeLogs("./log.txt", "Request Fail!Retry in 5...");
                            Thread.Sleep(5000);
                        }
                    } while (re != "ok");
                    url = $"http://bitcoinrobot.cn/vote/dl/{fileName}";
                }
                Log.writeLogs("./log.txt", "开始下载:" + url);
                downLoadCount = 0;
                bool result = true;
                do
                {
                    downLoadCount++;
                    result = HttpDownLoad.Download(url, pathName);
                    if (!result)
                    {
                        Form3.SetProName($"{voteProject.ProjectName}[{downLoadCount}次]下");
                        Log.writeLogs("./log.txt", voteProject.ProjectName + "  下载失败，5秒后重新下载");
                        Thread.Sleep(5000);
                        if (downLoadCount >= 3)
                        {
                            Log.writeLogs("./log.txt", voteProject.ProjectName + "  下载失败3次，返回");
                            return;
                        }
                    }
                } while (!result);

                Form3.SetProName("");
                Log.writeLogs("./log.txt", pathName + "  下载完成");
                Winrar.UnCompressRar(_mainForm.PathShare + "/投票项目/" + voteProject.ProjectName,
                    IniReadWriter.ReadIniKeys("Command", "Downloads", _mainForm.PathShare + "/CF.ini"),
                    voteProject.DownloadAddress.Substring(voteProject.DownloadAddress.LastIndexOf("/") + 1));
            }

            if (voteProject.Type == "九天" &&
                !File.Exists(_mainForm.PathShare + "/投票项目/" + voteProject.ProjectName + "/启动九天.bat"))
            {
                String[] Lines = { @"start vote.exe" };
                File.WriteAllLines(_mainForm.PathShare + "/投票项目/" + voteProject.ProjectName + "/启动九天.bat", Lines,
                    Encoding.GetEncoding("GBK"));
            }

            activeVoteProject = voteProject;
            setWorkerId(voteProject.IdType);
            if (isAutoVote)
            {
                //                if (!onlyWaitOrder)
                //                {
                writeAutoVoteProject();
                //                }
            }
            Log.writeLogs("./log.txt",
                "AutoVote: " + voteProject.ToString() + "    " + DateTime.Now.ToLocalTime().ToString());
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
            for (int p = int.Parse(Form1.VM1); p <= int.Parse(Form1.VM2); p++)
            {
                String taskName =
                    IniReadWriter.ReadIniKeys("Command", "TaskName" + p, _mainForm.PathShare + "/Task.ini");
                if (!onlyWaitOrder || taskName.Equals("待命"))
                {
                    if (vmInfo.ContainsKey(p))
                    {
                        if (isAutoVote && voteProject.Price <= vmInfo[p].Price)
                        {
                            continue;
                        }

                        vmInfo[p] = new TaskInfo(voteProject.ProjectName, voteProject.Price);
                    }
                    else
                    {
                        vmInfo.Add(p, new TaskInfo(voteProject.ProjectName, voteProject.Price));
                    }
                    string customPath = _mainForm.PathShareVm + "\\投票项目\\" + voteProject.ProjectName + "\\" + executableFile.Name;
                    SwitchUtil.SetVmState(p, customPath, SocketAction.TASK_VOTE_PROJECT, _mainForm.PathShare);
                }
            }

            if (isAutoVote)
            {
                bool allSameProject = true;
                foreach (int key in vmInfo.Keys)
                {
                    if (vmInfo[key].ProjectName != activeVoteProject.ProjectName)
                    {
                        allSameProject = false;
                        break;
                    }
                }

                if (allSameProject)
                {
                    writeAutoVoteProject();
                }
            }

            TaskInfos.Set(vmInfo);
            SocketAction.AUTO_VOTE_SELECT_INDEX(activeVoteProject.Index);
        }

        private void testVoteProjectMonitorList()
        {
            for (int i = 0; i < voteProjectMonitorList.Count; i++)
            {
                VoteProject voteProject = voteProjectMonitorList[i];
                if (voteProject.Auto && !voteProject.Drop && voteProject.VoteRemains)
                {
                    startVoteProject(voteProject, !allWaitOrder());
                    break;
                }
            }
        }

        private bool existWaitOrder()
        {
            bool result = false;
            arrDrop = IniReadWriter.ReadIniKeys("Command", "ArrDrop", _mainForm.PathShare + "/CF.ini");
            for (int i = int.Parse(Form1.VM1); i <= int.Parse(Form1.VM2); i++)
            {
                if (!StringUtil.isEmpty(arrDrop))
                {
                    arrDrop = " " + arrDrop;
                }

                if (arrDrop.IndexOf(" " + i + " |") == -1)
                {
                    string taskName =
                        IniReadWriter.ReadIniKeys("Command", "TaskName" + i, _mainForm.PathShare + "/Task.ini");
                    if (taskName.Equals("待命"))
                    {
                        result = true;
                        TaskInfos.Clear(i);
                    }
                }
            }

            return result;
        }

        private bool allWaitOrder()
        {
            arrDrop = IniReadWriter.ReadIniKeys("Command", "ArrDrop", _mainForm.PathShare + "/CF.ini");
            for (int i = int.Parse(Form1.VM1); i <= int.Parse(Form1.VM2); i++)
            {
                if (!StringUtil.isEmpty(arrDrop))
                {
                    arrDrop = " " + arrDrop;
                }

                if (arrDrop.IndexOf(" " + i + " |") == -1)
                {
                    string taskName =
                        IniReadWriter.ReadIniKeys("Command", "TaskName" + i, _mainForm.PathShare + "/Task.ini");
                    if (!taskName.Equals("待命"))
                    {
                        return false;
                    }
                }
            }

            return true;
        }

        private void generateBlackListTemp()
        {
            IniReadWriter.WriteIniKeys("Command", "voteProjectNameDropedTemp", "",
                _mainForm.PathShare + "/AutoVote.ini");
        }

        private void generateBlackList()
        {
            voteProjectNameDroped = IniReadWriter.ReadIniKeys("Command", "voteProjectNameDroped",
                _mainForm.PathShare + "/AutoVote.ini");
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

                IniReadWriter.WriteIniKeys("Command", "voteProjectNameDroped", projectNameDroped,
                    _mainForm.PathShare + "/AutoVote.ini");
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
                    if (!project.Drop && project.Price > activeVoteProject.Price && project.Auto && project.VoteRemains)
                    {
                        //排序更前 或 同项目价更高切换
                        if (i < activeVoteProject.Index ||
                            (activeVoteProject.ProjectName.Split('_')[0] == project.ProjectName.Split('_')[0]))
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

        private void ResolveDependency()
        {
            string dllPath = $"{PathCore.WorkingPath}\\Newtonsoft.Json.dll";
            if (!File.Exists(dllPath))
            {
                HttpDownLoad.Download("https://bitcoinrobot.cn/file/Newtonsoft.Json.dll", dllPath);
            }
        }

        private void autoVoteSystem()
        {
            Log.writeLogs("./log.txt", "");
            Log.writeLogs("./log.txt", "AutoVoteSystem Thread Running");
            ResolveDependency();
            TaskInfos.Init(_mainForm.PathShare + "/AutoVote.ini");
            if (isAutoVote)
            {
                string projectName =
                    IniReadWriter.ReadIniKeys("Command", "ProjectName", _mainForm.PathShare + "/AutoVote.ini");
                if (!StringUtil.isEmpty(projectName))
                {
                    try
                    {
                        double price = double.Parse(IniReadWriter.ReadIniKeys("Command", "Price",
                            _mainForm.PathShare + "/AutoVote.ini"));
                        long remains = long.Parse(IniReadWriter.ReadIniKeys("Command", "Remains",
                            _mainForm.PathShare + "/AutoVote.ini"));
                        string backgroundNo = IniReadWriter.ReadIniKeys("Command", "BackgroundNo",
                            _mainForm.PathShare + "/AutoVote.ini");
                        activeVoteProject = new VoteProject(projectName, price, remains, backgroundNo);
                    }
                    catch (Exception e)
                    {
                        Log.writeLogs("./log.txt", "加载ActiveVoteProject异常:" + e.StackTrace);
                    }
                }
            }

            int count = 0;
            do
            {
                count++;
                try
                {
                    List<VoteProject> voteProjects;
                    if (dataSource.Equals("NEW"))
                    {
                        voteProjects = getVoteProjects();
                    }
                    else
                    {
                        voteProjects = GetVoteProjectsServer();
                    }

                    voteProjectsAnalysis(voteProjects);
                    if (isAutoVote)
                    {
                        //8点 发送收益统计
                        if (DateTime.Now.Hour == 11 && DateTime.Now.Day != statisticDay && !StringUtil.isEmpty(email))
                        {
                            Log.writeLogs("./log.txt", $"发送收益统计至{email}!");
                            if (Email.Send(email, "收益统计", Statistic.GenerateStatistic()))
                            {
                                statisticDay = DateTime.Now.Day;
                                Statistic.Reset();
                            }
                            else
                            {
                                Log.writeLogs("./log.txt", $"发送失败！");
                            }
                        }
                        //每2小时  * 倍率 解封黑名单
                        if (DateTime.Now.Hour % (2 * blackRate) == 0 &&
                            DateTime.Now.Hour != clearBlackListHour)
                        {
                            clearBlackListHour = DateTime.Now.Hour;
                            Log.writeLogs("./log.txt", "Clear blackDictionary!");
                            blackDictionary.Clear();
                        }

                        //2分钟临时黑名单解锁
                        if (count == 2 * 4)
                        {
                            generateBlackListTemp();
                        }

                        //20分钟 * 倍率 黑名单解锁
                        if (count > 20 * 4 * blackRate)
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
                    Random rd = new Random();
                    refreshWindowText();
                    Thread.Sleep((8 + rd.Next(1, 10)) * 1000);
                }
                catch (Exception e)
                {
                    Console.WriteLine(e.StackTrace);
                    Log.writeLogs("./log.txt", e.StackTrace);
                }
            } while (true);
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
                    _mainForm.NotifyIcon1.ShowBalloonTip(0, val + "号虚拟机网络异常", DateTime.Now.ToLocalTime().ToString(),
                        ToolTipIcon.Info);
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
                    SocketAction.EXEC_REPLUG(val.ToString());
                    IniReadWriter.WriteIniKeys("Command", "ArrDrop", drop, _mainForm.PathShare + "/CF.ini");
                    _mainForm.NotifyIcon1.ShowBalloonTip(0, val + "号虚拟机掉线了", DateTime.Now.ToLocalTime().ToString(),
                        ToolTipIcon.Error);
                }
            }
            else if (count == 4)
            {
                if (over > 0)
                {
                    IniReadWriter.WriteIniKeys("Command", "OVER", "0", _mainForm.PathShare + "/CF.ini");
                    _mainForm.NotifyIcon1.ShowBalloonTip(0, "项目已结束", DateTime.Now.ToLocalTime().ToString(),
                        ToolTipIcon.Info);
                    if (overAuto)
                    {
                        button3.Text = "到票自动";
                        overAuto = false;
                        SocketAction.AUTO_VOTE_SET(1);
                    }

                    if (_mainForm.CheckBox3.Checked && !StringUtil.isEmpty(_mainForm.OverSwitchPath))
                    {
                        if (_mainForm.OverSwitchPath.Equals("HANGUP"))
                        {
                            SwitchUtil.swichVm("", IniReadWriter.ReadIniKeys("Command", "Hangup", _mainForm.PathShare + "/CF.ini"),
                                _mainForm.PathShare);
                        }
                        else
                        {
                            SwitchUtil.swichVm(_mainForm.OverSwitchPath, "投票项目", _mainForm.PathShare);
                        }
                        _mainForm.CheckBox3.Checked = false;
                    }
                }

                if (kick > 0)
                {
                    IniReadWriter.WriteIniKeys("Command", "KICK", "0", _mainForm.PathShare + "/CF.ini");
                    _mainForm.NotifyIcon1.ShowBalloonTip(0, "项目限人", DateTime.Now.ToLocalTime().ToString(),
                        ToolTipIcon.Info);
                }
            }
            else if (count == 5)
            {
                ActiveVm = "";
                for (int i = int.Parse(Form1.VM1); i <= int.Parse(Form1.VM2); i++)
                {
                    string state =
                        IniReadWriter.ReadIniKeys("Command", "TaskChange" + i, _mainForm.PathShare + "/Task.ini");
                    if (state == "1")
                    {
                        ActiveVm += " " + i + " |";
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

                if (ActiveVm != "")
                {
                    label4.Text = ActiveVm;
                }
                else
                {
                    label4.Text = TaskInfos.Active();
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
            this.TopMost = isTop;
            button1.Text = this.TopMost ? "取消置顶" : "置顶";
        }

        public static void StartSelectedVoteProject()
        {
            if (_form3.dataGridView1.SelectedRows.Count > 0)
            {
                int index = _form3.dataGridView1.SelectedRows[0].Index;
                _form3.startVoteProject(Form3.VoteProjectMonitorList[index], false);
            }
        }

        private void dataGridView1_DoubleClick(object sender, EventArgs e)
        {
            SocketAction.AUTO_VOTE_START_NAME_INDEX();
        }

        private void textBox1_TextChanged(object sender, EventArgs e)
        {
            if (!StringUtil.isEmpty(textBox1.Text))
            {
                filter = double.Parse(textBox1.Text);
                IniReadWriter.WriteIniKeys("Command", "filter", filter.ToString(),
                    _mainForm.PathShare + "/AutoVote.ini");
            }
        }

        private void button2_Click(object sender, EventArgs e)
        {
            SocketAction.AUTO_VOTE_SET(1);
        }

        private void button3_Click(object sender, EventArgs e)
        {
            SocketAction.AUTO_VOTE_SET(2);
        }


        private void textBox2_TextChanged(object sender, EventArgs e)
        {
            if (!StringUtil.isEmpty(textBox2.Text))
            {
                IniReadWriter.WriteIniKeys("Command", "maxKb", textBox2.Text, _mainForm.PathShare + "/CF.ini");
            }
        }

        private void textBox3_TextChanged(object sender, EventArgs e)
        {
            if (!StringUtil.isEmpty(textBox3.Text))
            {
                blackRate = double.Parse(textBox3.Text);
                IniReadWriter.WriteIniKeys("Command", "blackRate", textBox3.Text,
                    _mainForm.PathShare + "/AutoVote.ini");
            }
        }

        private void dataGridView1_CellContentClick(object sender, DataGridViewCellEventArgs e)
        {
            try
            {
                if (dataGridView1.Rows.Count > e.RowIndex && e.RowIndex >= 0 && dataGridView1.Columns[e.ColumnIndex] is DataGridViewCheckBoxColumn)
                {
                    string projectName = dataGridView1.Rows[e.RowIndex].Cells[0].Value.ToString();
                    bool val = (bool)dataGridView1.Rows[e.RowIndex].Cells[e.ColumnIndex].Value;
                    if (e.ColumnIndex == 5)
                    {
                        //关联拉黑 禁用button
                        if (voteProjectMonitorList[e.RowIndex].RelDrop)
                        {
                            return;
                        }
                        string voteProjectNameDroped = IniReadWriter.ReadIniKeys("Command", "voteProjectNameDroped",
                            _mainForm.PathShare + "/AutoVote.ini");
                        if (!val)
                        {
                            voteProjectNameDroped +=
                                StringUtil.isEmpty(voteProjectNameDroped) ? projectName : "|" + projectName;
                            dataGridView1.Rows[e.RowIndex].Cells[e.ColumnIndex].Value = true;
                        }
                        else
                        {
                            voteProjectNameDroped = voteProjectNameDroped.Replace("|" + projectName, "")
                                .Replace(projectName, "");
                            if (voteProjectNameDropedTemp.IndexOf(projectName) != -1)
                            {
                                string voteProjectNameDropedTemp = IniReadWriter.ReadIniKeys("Command",
                                    "voteProjectNameDropedTemp", _mainForm.PathShare + "/AutoVote.ini");
                                voteProjectNameDropedTemp = voteProjectNameDropedTemp.Replace("|" + projectName, "")
                                    .Replace(projectName, "");
                                IniReadWriter.WriteIniKeys("Command", "voteProjectNameDropedTemp",
                                    voteProjectNameDropedTemp, _mainForm.PathShare + "/AutoVote.ini");
                            }

                            dataGridView1.Rows[e.RowIndex].Cells[e.ColumnIndex].Value = false;
                        }

                        IniReadWriter.WriteIniKeys("Command", "voteProjectNameDroped", voteProjectNameDroped,
                            _mainForm.PathShare + "/AutoVote.ini");
                    }
                    else if (e.ColumnIndex == 6)
                    {
                        string allProjectName = projectName.Split('_')[0];
                        string voteProjectNameToped = IniReadWriter.ReadIniKeys("Command", "voteProjectNameToped",
                            _mainForm.PathShare + "/AutoVote.ini");
                        if (!val)
                        {
                            if (voteProjectNameToped.IndexOf(allProjectName) == -1)
                            {
                                voteProjectNameToped += StringUtil.isEmpty(voteProjectNameToped)
                                    ? allProjectName
                                    : "|" + allProjectName;
                            }

                            dataGridView1.Rows[e.RowIndex].Cells[e.ColumnIndex].Value = true;
                        }
                        else
                        {
                            voteProjectNameToped = voteProjectNameToped.Replace("|" + allProjectName, "")
                                .Replace(allProjectName, "");
                            dataGridView1.Rows[e.RowIndex].Cells[e.ColumnIndex].Value = false;
                        }

                        IniReadWriter.WriteIniKeys("Command", "voteProjectNameToped", voteProjectNameToped,
                            _mainForm.PathShare + "/AutoVote.ini");
                    }
                }
            }
            catch (Exception)
            {
                MessageBox.Show("数据刷新中，请重新操作！");
            }
        }

        private void button4_Click(object sender, EventArgs e)
        {
            if (dataSource.Equals("NEW"))
            {
                dataSource = "";
                this.button4.Text = "网";
            }
            else
            {
                dataSource = "NEW";
                this.button4.Text = "服";
            }

            IniReadWriter.WriteIniKeys("Command", "dataSource", dataSource, _mainForm.PathShare + "/AutoVote.ini");
        }

        private void dataGridView1_DataSourceChanged(object sender, EventArgs e)
        {
            BindingList<VoteProject> voteProjects = (BindingList<VoteProject>)this.dataGridView1.DataSource;
            for (var i = 0; i < voteProjects.Count; i++)
            {
                if (voteProjects[i].RelDrop)
                {
                    this.dataGridView1.Rows[i].Cells[5].Style.BackColor = Color.LightGray;
                }
            }
        }

        private void button5_Click(object sender, EventArgs e)
        {
            if (isAdsl.Equals("1"))
            {
                isAdsl = "0";
                this.button5.Text = "虚";
            }
            else
            {
                isAdsl = "1";
                this.button5.Text = "主";
            }
            IniReadWriter.WriteIniKeys("Command", "isAdsl", isAdsl, _mainForm.PathShare + "/CF.ini");
        }

        private void button6_Click(object sender, EventArgs e)
        {
            Form4.InstanceForm();
        }
    }
}