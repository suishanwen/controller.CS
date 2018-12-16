using System;
using System.Windows.Forms;
using controller.util;
using System.IO;
using System.Threading;
using System.Management;
using System.Text;
using System.Diagnostics;
using System.Collections.Generic;

namespace controller
{
    //mouse_event(MOUSEEVENTF_MOVE, 0, -30, 0, 0);
    //mouse_event(MOUSEEVENTF_LEFTUP|MOUSEEVENTF_LEFTDOWN , 0, -30, 0, 0);

    public partial class Form1 : Form
    {
        private static Form3 _Form3;
        private string pathShare; //主机共享路径
        private string downloads; //下载路径，用于RAR一键解压
        private string votePath; //投票路径，用于一键解压与OPT一键清空
        private string pathShareVm; //虚拟机内的共享路径，用于分割字符串将主机路径转换为虚拟机路径
        private string user;//用户ID
        private string overSwitchPath;//到票切换路径

        //取CPU编号   
        public String GetCpuID()
        {
            try
            {
                ManagementClass mc = new ManagementClass("Win32_Processor");
                ManagementObjectCollection moc = mc.GetInstances();

                String strCpuID = null;
                foreach (ManagementObject mo in moc)
                {
                    strCpuID = mo.Properties["ProcessorId"].Value.ToString();
                    break;
                }
                return strCpuID;
            }
            catch
            {
                return textBox5.Text + "-unknown";
            }
        }

        public String GetHardDiskID()
        {
            try
            {
                ManagementObjectSearcher searcher = new ManagementObjectSearcher("SELECT * FROM Win32_PhysicalMedia");
                String strHardDiskID = null;
                foreach (ManagementObject mo in searcher.Get())
                {
                    strHardDiskID = mo["SerialNumber"].ToString().Trim();
                    break;
                }
                return strHardDiskID;
            }
            catch
            {
                return textBox5.Text + "-unknown";
            }
        }

        public string PathShare
        {
            get
            {
                return pathShare;
            }

            set
            {
                pathShare = value;
            }
        }

        public string Downloads
        {
            get
            {
                return downloads;
            }

            set
            {
                downloads = value;
            }
        }

        public string VotePath
        {
            get
            {
                return votePath;
            }

            set
            {
                votePath = value;
            }
        }

        public string PathShareVm
        {
            get
            {
                return pathShareVm;
            }

            set
            {
                pathShareVm = value;
            }
        }


        public string VM1
        {
            get
            {
                return textBox2.Text;
            }
        }
        public string VM2
        {
            get
            {
                return textBox3.Text;
            }
        }
        //委托 解决线程间操作textBox4问题
        delegate void SetTextBox4(String value);
        private void SetVM3(String value)
        {
            if (this.textBox4.InvokeRequired)
            {
                SetTextBox4 d = new SetTextBox4(SetVM3);
                this.Invoke(d, new object[] { value });
            }
            else
            {
                textBox4.Text = value;
            }
        }

        public string VM3
        {
            get
            {
                return textBox4.Text;
            }
            set
            {
                SetVM3(value);
            }
        }
        public TextBox VM3TextBox
        {
            get
            {
                return textBox4;
            }
        }
        public NotifyIcon NotifyIcon1
        {
            get
            {
                return notifyIcon1;
            }
        }
        public CheckBox CheckBox3
        {
            get
            {
                return checkBox3;
            }
        }

        public string OverSwitchPath
        {
            get
            {
                return overSwitchPath;
            }

            set
            {
                overSwitchPath = value;
            }
        }

        public Form1()
        {
            InitializeComponent();
            initConfig();
            _Form3 = new Form3(this);
            _Form3.Show();
        }


        //关闭程序，结束自动挂票线程
        private void Form1_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (DialogResult.OK == MessageBox.Show("你确定要关闭应用程序吗？", "关闭提示", MessageBoxButtons.OKCancel, MessageBoxIcon.Question))
            {
                if (_Form3.AutoVote != null)
                {
                    _Form3.AutoVote.Abort();
                }
                this.FormClosing -= new FormClosingEventHandler(this.Form1_FormClosing);//为保证Application.Exit();时不再弹出提示，所以将FormClosing事件取消
                Application.Exit();//退出整个应用程序
            }
            else
            {
                e.Cancel = true;  //取消关闭事件
            }
        }

        private void initConfig()
        {
            PathShare = IniReadWriter.ReadIniKeys("Form", "pathShare", "./controller.ini");
            textBox1.Text = PathShare;
            textBox2.Text = IniReadWriter.ReadIniKeys("Form", "vm1", "./controller.ini");
            textBox3.Text = IniReadWriter.ReadIniKeys("Form", "vm2", "./controller.ini");
            textBox5.Text = IniReadWriter.ReadIniKeys("Command", "worker", PathShare + "/CF.ini");
            textBox6.Text = IniReadWriter.ReadIniKeys("Command", "cishu", PathShare + "/CF.ini");
            user = IniReadWriter.ReadIniKeys("Command", "USER", PathShare + "/CF.ini");
            Downloads = IniReadWriter.ReadIniKeys("Command", "downloads", PathShare + "/CF.ini");
            VotePath = IniReadWriter.ReadIniKeys("Command", "votePath", PathShare + "/CF.ini");
            PathShareVm = IniReadWriter.ReadIniKeys("Command", "gongxiang", PathShare + "/CF.ini");
            string idSelect = IniReadWriter.ReadIniKeys("Command", "IDSelect", PathShare + "/CF.ini");
            string printgonghao = IniReadWriter.ReadIniKeys("Command", "printgonghao", PathShare + "/CF.ini");
            if (!StringUtil.isEmpty(printgonghao) && printgonghao == "1")
            {
                checkBox1.Checked = true;
            }
            string tail = IniReadWriter.ReadIniKeys("Command", "tail", PathShare + "/CF.ini");
            if (!StringUtil.isEmpty(tail) && tail == "1")
            {
                checkBox2.Checked = true;
            }
            if (!StringUtil.isEmpty(idSelect))
            {
                string[] strArray = idSelect.Split('|');
                for (int i = 0; i < strArray.Length; i++)
                {
                    comboBox1.Items.Insert(i, strArray[i]);
                }
            }
            notifyIcon1.ShowBalloonTip(3000, "监控启动", "当前时间：" + DateTime.Now.ToLocalTime().ToString(), ToolTipIcon.Info);

        }
        //选择共享button
        private void button1_Click(object sender, EventArgs e)
        {
            FolderBrowserDialog fd = new FolderBrowserDialog();
            if (fd.ShowDialog() == DialogResult.OK)
            {
                textBox1.Text = fd.SelectedPath;
                PathShare = fd.SelectedPath;
                IniReadWriter.WriteIniKeys("Form", "pathShare", fd.SelectedPath, "./controller.ini");
            }
        }
        //vm1改变
        private void textBox2_TextChanged(object sender, EventArgs e)
        {
            IniReadWriter.WriteIniKeys("Form", "vm1", textBox2.Text, "./controller.ini");
        }
        //vm2改变
        private void textBox3_TextChanged(object sender, EventArgs e)
        {
            IniReadWriter.WriteIniKeys("Form", "vm2", textBox3.Text, "./controller.ini");
        }
        //改变工号
        private void textBox5_TextChanged(object sender, EventArgs e)
        {
            IniReadWriter.WriteIniKeys("Command", "worker", textBox5.Text, PathShare + "/CF.ini");
        }
        //选择工号
        private void comboBox1_SelectedIndexChanged(object sender, EventArgs e)
        {
            textBox5.Text = comboBox1.Text;
            IniReadWriter.WriteIniKeys("Command", "worker", comboBox1.Text, PathShare + "/CF.ini");
        }

        //开关工号
        private void checkBox1_CheckedChanged(object sender, EventArgs e)
        {
            if (checkBox1.Checked)
            {
                IniReadWriter.WriteIniKeys("Command", "printgonghao", "1", PathShare + "/CF.ini");
            }
            else
            {
                IniReadWriter.WriteIniKeys("Command", "printgonghao", "0", PathShare + "/CF.ini");
            }
        }
        //开关分号
        private void checkBox2_CheckedChanged(object sender, EventArgs e)
        {
            if (checkBox2.Checked)
            {
                IniReadWriter.WriteIniKeys("Command", "tail", "1", PathShare + "/CF.ini");
            }
            else
            {
                IniReadWriter.WriteIniKeys("Command", "tail", "0", PathShare + "/CF.ini");
            }
        }
        //待命点击
        private void button2_Click(object sender, EventArgs e)
        {
            DialogResult dr = MessageBox.Show("确定要‘待命’吗?", "待命", MessageBoxButtons.OKCancel);
            if (dr == DialogResult.OK)
            {

                IniReadWriter.WriteIniKeys("Command", "Copy", "0", PathShare + "/CF.ini");
                IniReadWriter.WriteIniKeys("Command", "Delete", "0", PathShare + "/CF.ini");
                IniReadWriter.WriteIniKeys("Command", "Cookie", "0", PathShare + "/CF.ini");
                SwitchUtil.swichVm(textBox2.Text, textBox3.Text, this, "", "待命", PathShare);
            }
        }
        //关机点击
        private void button3_Click(object sender, EventArgs e)
        {
            DialogResult dr = MessageBox.Show("确定要‘关机’吗?", "关机", MessageBoxButtons.OKCancel);
            if (dr == DialogResult.OK)
            {
                SwitchUtil.swichVm(textBox2.Text, textBox3.Text, this, "", "关机", PathShare);
            }
        }
        //重启点击
        private void button4_Click(object sender, EventArgs e)
        {
            DialogResult dr = MessageBox.Show("确定要‘重启’吗?", "重启", MessageBoxButtons.OKCancel);
            if (dr == DialogResult.OK)
            {
                SwitchUtil.swichVm(textBox2.Text, textBox3.Text, this, "", "重启", PathShare);
            }
        }
        //升级点击
        private void button5_Click(object sender, EventArgs e)
        {
            DialogResult dr = MessageBox.Show("确定要‘升级’吗?", "升级", MessageBoxButtons.OKCancel);
            if (dr == DialogResult.OK)
            {
                SwitchUtil.swichVm(textBox2.Text, textBox3.Text, this, "", "Update", PathShare);
            }
        }
        //网络测试点击
        private void button6_Click(object sender, EventArgs e)
        {
            DialogResult dr = MessageBox.Show("确定要‘网络测试’吗?", "网络测试", MessageBoxButtons.OKCancel);
            if (dr == DialogResult.OK)
            {
                SwitchUtil.swichVm(textBox2.Text, textBox3.Text, this, "", "网络测试", PathShare);
            }
        }
        //改变超时
        private void textBox6_TextChanged(object sender, EventArgs e)
        {
            IniReadWriter.WriteIniKeys("Command", "cishu", textBox6.Text, PathShare + "/CF.ini");
        }
        //解压文件
        private void button7_Click(object sender, EventArgs e)
        {
            if (StringUtil.isEmpty(Downloads))
            {
                MessageBox.Show("请选择您的下载路径（一键解压必须安装WINRAR，并且已经设置环境变量【将Winrar安装目录复制到系统属性-高级系统设置-环境变量-系统变量-path】）", "一键解压");
                FolderBrowserDialog fd = new FolderBrowserDialog();
                if (fd.ShowDialog() == DialogResult.OK)
                {
                    Downloads = fd.SelectedPath;
                    IniReadWriter.WriteIniKeys("Command", "Downloads", fd.SelectedPath, PathShare + "/CF.ini");
                }
            }
            else
            {
                DirectoryInfo theFolder = new DirectoryInfo(Downloads);
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
        }
        //清空文件
        private void button8_Click(object sender, EventArgs e)
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
        }
        //投票
        private void button9_Click(object sender, EventArgs e)
        {
            DirectoryInfo theFolder = new DirectoryInfo(PathShare + "/投票项目");
            DirectoryInfo[] allDir = theFolder.GetDirectories();
            Form2.InstanceForm(this, allDir);
        }

        private void notifyIcon1_MouseDoubleClick(object sender, MouseEventArgs e)
        {
            if (this.WindowState == FormWindowState.Minimized)
            {
                this.WindowState = FormWindowState.Normal;
            }
        }

        private void button10_Click(object sender, EventArgs e)
        {
            if (CheckBox3.Checked)
            {
                OverSwitchPath = "HANGUP";
                CheckBox3.Text = "到票切换" + IniReadWriter.ReadIniKeys("Command", "Hangup", PathShare + "/CF.ini");
            }else
            {
                DialogResult dr = MessageBox.Show("确定要‘挂机’吗?", "挂机", MessageBoxButtons.OKCancel);
                if (dr == DialogResult.OK)
                {
                    SwitchUtil.swichVm(textBox2.Text, textBox3.Text, this, "", IniReadWriter.ReadIniKeys("Command", "Hangup", PathShare + "/CF.ini"), PathShare);
                }
            }
        }

        //通过路径启动进程
        private  void StartProcess(string pathName)
        {
            ProcessStartInfo info = new ProcessStartInfo();
            info.FileName = pathName;
            info.Arguments = "";
            info.WorkingDirectory = pathName.Substring(0, pathName.LastIndexOf("\\"));
            info.WindowStyle = ProcessWindowStyle.Normal;
            Process pro = Process.Start(info);
            Thread.Sleep(500);
        }

        private void button11_Click(object sender, EventArgs e)
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

        private void checkBox3_CheckedChanged(object sender, EventArgs e)
        {
            if (!checkBox3.Checked)
            {
                OverSwitchPath = "";
                checkBox3.Text = "到票切换";
            }
        }

    }
}
