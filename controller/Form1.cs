using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using controller.util;
using System.IO;

namespace controller
{
    //mouse_event(MOUSEEVENTF_MOVE, 0, -30, 0, 0);
    //mouse_event(MOUSEEVENTF_LEFTUP|MOUSEEVENTF_LEFTDOWN , 0, -30, 0, 0);
    //String test = IniReadWriter.ReadIniKeys("sc", "name", "c:/test.ini");
    //MessageBox.Show(test);
    //IniReadWriter.WriteIniKeys("sc", "name", textBox1.Text, "c:/test.ini");
    //test = IniReadWriter.ReadIniKeys("sc", "name", "c:/test.ini");
    //MessageBox.Show(test);
    public partial class Form1 : Form
    {
        private string pathShare; //主机共享路径
        private string downloads; //下载路径，用于RAR一键解压
        private string votePath; //投票路径，用于一键解压与OPT一键清空
        private string pathShareVm; //虚拟机内的共享路径，用于分割字符串将主机路径转换为虚拟机路径

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
        public TextBox VM3TextBox
        {
            get
            {
                return textBox4;
            }
        }

        public Form1()
        {
            InitializeComponent();
            initConfig();
            new Form3().Show();
        }


        private void initConfig()
        {
            PathShare = IniReadWriter.ReadIniKeys("Form", "pathShare", "./controller.ini");
            textBox1.Text = PathShare;
            textBox2.Text = IniReadWriter.ReadIniKeys("Form", "vm1", "./controller.ini");
            textBox3.Text = IniReadWriter.ReadIniKeys("Form", "vm2", "./controller.ini");
            textBox5.Text = IniReadWriter.ReadIniKeys("Command", "worker", PathShare + "/CF.ini");
            textBox6.Text = IniReadWriter.ReadIniKeys("Command", "cishu", PathShare + "/CF.ini");
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
                SwitchUtil.swichVm(textBox2.Text, textBox3.Text, textBox4, "", "待命", PathShare);
            }
        }
        //关机点击
        private void button3_Click(object sender, EventArgs e)
        {
            DialogResult dr = MessageBox.Show("确定要‘关机’吗?", "关机", MessageBoxButtons.OKCancel);
            if (dr == DialogResult.OK)
            {
                SwitchUtil.swichVm(textBox2.Text, textBox3.Text, textBox4, "", "关机", PathShare);
            }
        }
        //重启点击
        private void button4_Click(object sender, EventArgs e)
        {
            DialogResult dr = MessageBox.Show("确定要‘重启’吗?", "重启", MessageBoxButtons.OKCancel);
            if (dr == DialogResult.OK)
            {
                SwitchUtil.swichVm(textBox2.Text, textBox3.Text, textBox4, "", "重启", PathShare);
            }
        }
        //升级点击
        private void button5_Click(object sender, EventArgs e)
        {
            DialogResult dr = MessageBox.Show("确定要‘升级’吗?", "升级", MessageBoxButtons.OKCancel);
            if (dr == DialogResult.OK)
            {
                SwitchUtil.swichVm(textBox2.Text, textBox3.Text, textBox4, "", "Update", PathShare);
            }
        }
        //网络测试点击
        private void button6_Click(object sender, EventArgs e)
        {
            DialogResult dr = MessageBox.Show("确定要‘网络测试’吗?", "网络测试", MessageBoxButtons.OKCancel);
            if (dr == DialogResult.OK)
            {
                SwitchUtil.swichVm(textBox2.Text, textBox3.Text, textBox4, "", "网络测试", PathShare);
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
                catch (System.UnauthorizedAccessException)
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



    }
}
