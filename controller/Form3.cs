using controller.util;
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
        private string dataSource;


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
            dataSource = IniReadWriter.ReadIniKeys("Command", "dataSource", _mainForm.PathShare + "/CF.ini");
            string _isAutoVote = IniReadWriter.ReadIniKeys("Command", "isAutoVote", _mainForm.PathShare + "/CF.ini");
            try
            {
                filter = double.Parse(IniReadWriter.ReadIniKeys("Command", "filter", _mainForm.PathShare + "/AutoVote.ini"));
                blackRate = double.Parse(IniReadWriter.ReadIniKeys("Command", "blackRate", _mainForm.PathShare + "/AutoVote.ini"));
            }
            catch (Exception) { };
            textBox1.Text = filter.ToString();
            textBox2.Text = IniReadWriter.ReadIniKeys("Command", "maxKb", _mainForm.PathShare + "/CF.ini");
            textBox3.Text = blackRate.ToString();
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
            string voteProjectNameToped = IniReadWriter.ReadIniKeys("Command", "voteProjectNameToped", _mainForm.PathShare + "/AutoVote.ini").Trim();
            if (StringUtil.isEmpty(voteProjectNameToped))
            {
                return false;
            }
            string[] topedProjectList = voteProjectNameToped.Split('|');
            foreach (string topedProject in topedProjectList)
            {
                if (project.IndexOf(topedProject) != -1)
                {
                    return true;
                }
            }
            return false;
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
            return voteProjectNameDroped.IndexOf(project) != -1 || voteProjectNameDropedTemp.IndexOf(project) != -1;
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

        private string getJsonVal(string json, string name)
        {
            json = json.Substring(json.IndexOf($"{name}:"));
            int index = json.IndexOf(",");
            if (index == -1)
            {
                return json.Substring(name.Length + 2, json.IndexOf("}") - (name.Length + 2));
            }
            return json.Substring(name.Length+2, index-(name.Length + 2));
        }

        private List<VoteProject> getVoteProjects()
        {
            HttpManager httpUtil = HttpManager.getInstance();
            string result = "";
            do
            {
                try
                {
                    result = httpUtil.requestHttpGet("http://bitcoinrobot.cn:8000/", "voteInfo", "");
                }
                catch (Exception)
                {
                    result = "";
                    Console.WriteLine("Request Fail!Retry in 10s...");
                    Log.writeLogs("./log.txt", "Request Fail!Retry in 10s...");
                    Thread.Sleep(10000);
                }
            } while (result == "" || result == "timeout");
            List<VoteProject> voteProjectList = new List<VoteProject>();
            string pat = @"(\[).*?(\])";
            Match matched = Regex.Match(result.Replace("'", ""), pat, RegexOptions.IgnoreCase);
            pat = @"(\{).*?(\})";
            MatchCollection matches = Regex.Matches(matched.Value.Replace("[", "")
                .Replace("]", ""), pat, RegexOptions.IgnoreCase);
            foreach (Match m in matches)
            {
                string json = m.Value;
                VoteProject voteProject = new VoteProject();
                voteProject.ProjectName = getJsonVal(json, "projectName");
                voteProject.Hot = int.Parse(getJsonVal(json, "hot"));
                voteProject.Price = double.Parse(getJsonVal(json, "price"));
                voteProject.FinishQuantity = long.Parse(getJsonVal(json, "finishQuantity"));
                voteProject.TotalRequire = long.Parse(getJsonVal(json, "totalRequire"));
                voteProject.Remains = long.Parse(getJsonVal(json, "remains"));
                voteProject.BackgroundAddress = getJsonVal(json, "backgroundAddress");
                voteProject.DownloadAddress = getJsonVal(json, "downloadAddress");
                voteProject.IdType = getJsonVal(json, "idType");
                voteProject.BackgroundNo = getJsonVal(json, "backgroundNo");
                voteProject.RefreshDate = Convert.ToDateTime(DateTime.Now.Year + "-" + getJsonVal(json, "refreshDate") + ":00");
                if(voteProject.Price>= filter)
                {
                    voteProjectList.Add(voteProject);
                }
            }
            return voteProjectList;
        }


        private List<VoteProject> getVoteProjectsBT()
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
                    if (voteProject.Price >= filter)
                    {
                        voteProjectList.Add(voteProject);
                    }

                }
            }
            return voteProjectList;
        }


        //委托 解决线程间操作dataGrid问题
        delegate void SetSelectedDataGridView();

        private void SetSelectedDataGrid()
        {
            if (this.dataGridView1.InvokeRequired)
            {
                SetSelectedDataGridView d = new SetSelectedDataGridView(SetSelectedDataGrid);
                this.Invoke(d, new object[] { });
            }
            else
            {
                if (activeVoteProject != null && activeVoteProject.Index != -1)
                {
                    this.dataGridView1.CurrentCell = this.dataGridView1[0, activeVoteProject.Index];
                }
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
                    SetSelectedDataGrid();
                }
                this.dataGridView1.Refresh();
            }
        }


        private void voteProjectsAnalysis(List<VoteProject> voteProjectList)
        {
            voteProjectMonitorList.Clear();
            foreach (VoteProject voteProject in voteProjectList)
            {
                voteProject.Drop = isDropedProject(voteProject.ProjectName, 0);
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
                string projectName = IniReadWriter.ReadIniKeys("Command", "ProjectName", _mainForm.PathShare + "/AutoVote.ini");
                if (projectName != activeVoteProject.ProjectName)
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
            string[] user1 = { "AQ-239356", "Q7-21173" };
            string[] user2 = { "AQ-14", "Q7-43" };
            string id = IniReadWriter.ReadIniKeys("Command", "worker", _mainForm.PathShare + "/CF.ini");
            if (id == user2[0] || id == user2[1])
            {
                if (activeVoteProject.IdType.Equals("Q7"))
                {
                    IniReadWriter.WriteIniKeys("Command", "worker", user2[1], _mainForm.PathShare + "/CF.ini");
                }
                else
                {
                    IniReadWriter.WriteIniKeys("Command", "worker", user2[0], _mainForm.PathShare + "/CF.ini");
                }
            }
            else
            {
                if (activeVoteProject.IdType.Equals("Q7"))
                {
                    IniReadWriter.WriteIniKeys("Command", "worker", user1[1], _mainForm.PathShare + "/CF.ini");
                }
                else
                {
                    IniReadWriter.WriteIniKeys("Command", "worker", user1[0], _mainForm.PathShare + "/CF.ini");
                }
            }
            IniReadWriter.WriteIniKeys("Command", "printgonghao", "1", _mainForm.PathShare + "/CF.ini");
        }


        private void startVoteProject(VoteProject voteProject, bool onlyWaitOrder)
        {
            Console.WriteLine("projectName：" + voteProject.ProjectName + ",price：" + voteProject.Price + ",remains：" + voteProject.Remains);
            string pathName = IniReadWriter.ReadIniKeys("Command", "Downloads", _mainForm.PathShare + "/CF.ini") + "\\" + voteProject.DownloadAddress.Substring(voteProject.DownloadAddress.LastIndexOf("/") + 1);
            if (!Directory.Exists(_mainForm.PathShare + "/投票项目/" + voteProject.ProjectName))
            {
                string url = voteProject.DownloadAddress;
                string now = DateTime.Now.ToLocalTime().ToString();
                Form3.SetProName(voteProject.ProjectName);
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
                Winrar.UnCompressRar(_mainForm.PathShare + "/投票项目/" + voteProject.ProjectName, IniReadWriter.ReadIniKeys("Command", "Downloads", _mainForm.PathShare + "/CF.ini"), voteProject.DownloadAddress.Substring(voteProject.DownloadAddress.LastIndexOf("/") + 1));
            }
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
                //                if (!onlyWaitOrder)
                //                {
                writeAutoVoteProject();
                //                }
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
                    Form1.SetVM3(p.ToString());
                    SwitchUtil.swichVm(_mainForm.VM1, _mainForm.VM2, _mainForm, _mainForm.PathShareVm + "\\投票项目\\" + voteProject.ProjectName + "\\" + executableFile.Name, "投票项目", _mainForm.PathShare);
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
            SetSelectedDataGrid();
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

        private bool allWaitOrder()
        {
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
                    if (!project.Drop && project.Price > activeVoteProject.Price && project.Auto && project.VoteRemains)
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
                    List<VoteProject> voteProjects;
                    if (dataSource.Equals("BT"))
                    {
                        voteProjects = getVoteProjectsBT();
                    }
                    else
                    {
                        voteProjects = getVoteProjects();
                    }
                    voteProjectsAnalysis(voteProjects);
                    if (isAutoVote)
                    {
                        //每2小时  * 倍率 解封黑名单
                        if (DateTime.Now.Minute == 1 && DateTime.Now.Hour % (2 * blackRate) == 0 && DateTime.Now.Hour != clearBlackListHour)
                        {
                            clearBlackListHour = DateTime.Now.Hour;
                            Log.writeLogs("./log.txt", "Clear blackDictionary!");
                            blackDictionary.Clear();
                        }
                        //5分钟临时黑名单解锁
                        if (count == 5 * 3)
                        {
                            generateBlackListTemp();
                        }
                        //20分钟 * 倍率 黑名单解锁
                        if (count > 20 * 3 * blackRate)
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
                    if (overAuto)
                    {
                        button3.Text = "到票自动";
                        overAuto = false;
                        setAutoVote();
                    }
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
            TopMost = isTop;
            button1.Text = TopMost ? "取消置顶" : "置顶";
        }

        private void selectVoteProject()
        {
            if (dataGridView1.SelectedRows.Count > 0)
            {
                int index = dataGridView1.SelectedRows[0].Index;
                startVoteProject(VoteProjectMonitorList[index], false);
            }
        }

        private void dataGridView1_DoubleClick(object sender, EventArgs e)
        {
            new Thread(selectVoteProject).Start();
        }

        private void textBox1_TextChanged(object sender, EventArgs e)
        {
            if (!StringUtil.isEmpty(textBox1.Text))
            {
                filter = double.Parse(textBox1.Text);
                IniReadWriter.WriteIniKeys("Command", "filter", filter.ToString(), _mainForm.PathShare + "/AutoVote.ini");
            }
        }

        private void setAutoVote()
        {
            isAutoVote = !isAutoVote;
            IniReadWriter.WriteIniKeys("Command", "isAutoVote", isAutoVote ? "1" : "0", _mainForm.PathShare + "/CF.ini");
            button2.Text = isAutoVote ? "取消自动" : "开启自动";
            button3.Visible = !isAutoVote;
        }

        private void button2_Click(object sender, EventArgs e)
        {
            setAutoVote();
        }

        private void button3_Click(object sender, EventArgs e)
        {
            overAuto = !overAuto;
            button3.Text = overAuto ? "撤销" : "到票自动";
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
                IniReadWriter.WriteIniKeys("Command", "blackRate", textBox3.Text, _mainForm.PathShare + "/AutoVote.ini");
            }
        }

        private void dataGridView1_CellContentClick(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex >= 0 && dataGridView1.Columns[e.ColumnIndex] is DataGridViewCheckBoxColumn)
            {
                string projectName = dataGridView1.Rows[e.RowIndex].Cells[0].Value.ToString();
                bool val = (bool)dataGridView1.Rows[e.RowIndex].Cells[e.ColumnIndex].Value;
                if (e.ColumnIndex == 5)
                {
                    string voteProjectNameDroped = IniReadWriter.ReadIniKeys("Command", "voteProjectNameDroped", _mainForm.PathShare + "/AutoVote.ini");
                    if (!val)
                    {
                        voteProjectNameDroped += StringUtil.isEmpty(voteProjectNameDroped) ? projectName : "|" + projectName;
                        dataGridView1.Rows[e.RowIndex].Cells[e.ColumnIndex].Value = true;
                    }
                    else
                    {
                        voteProjectNameDroped = voteProjectNameDroped.Replace("|" + projectName, "")
                            .Replace(projectName, "");
                        if (voteProjectNameDropedTemp.IndexOf(projectName) != -1)
                        {
                            string voteProjectNameDropedTemp = IniReadWriter.ReadIniKeys("Command", "voteProjectNameDropedTemp", _mainForm.PathShare + "/AutoVote.ini");
                            voteProjectNameDropedTemp = voteProjectNameDropedTemp.Replace("|" + projectName, "")
                                .Replace(projectName, "");
                            IniReadWriter.WriteIniKeys("Command", "voteProjectNameDropedTemp", voteProjectNameDropedTemp, _mainForm.PathShare + "/AutoVote.ini");

                        }
                        dataGridView1.Rows[e.RowIndex].Cells[e.ColumnIndex].Value = false;
                    }
                    IniReadWriter.WriteIniKeys("Command", "voteProjectNameDroped", voteProjectNameDroped, _mainForm.PathShare + "/AutoVote.ini");

                }
                else if (e.ColumnIndex == 6)
                {
                    string allProjectName = projectName.Split('_')[0];
                    string voteProjectNameToped = IniReadWriter.ReadIniKeys("Command", "voteProjectNameToped", _mainForm.PathShare + "/AutoVote.ini");
                    if (!val)
                    {
                        if (voteProjectNameToped.IndexOf(allProjectName) == -1)
                        {
                            voteProjectNameToped += StringUtil.isEmpty(voteProjectNameToped) ? allProjectName : "|" + allProjectName;
                        }
                        dataGridView1.Rows[e.RowIndex].Cells[e.ColumnIndex].Value = true;
                    }
                    else
                    {
                        voteProjectNameToped = voteProjectNameToped.Replace("|" + allProjectName, "")
                            .Replace(allProjectName, "");
                        dataGridView1.Rows[e.RowIndex].Cells[e.ColumnIndex].Value = false;
                    }
                    IniReadWriter.WriteIniKeys("Command", "voteProjectNameToped", voteProjectNameToped, _mainForm.PathShare + "/AutoVote.ini");

                }
            }
        }

    }
}
