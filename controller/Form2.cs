using controller.util;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace controller
{
    public partial class Form2 : Form
    {
        public Form2()
        {
            InitializeComponent();
        }
        private static Form1 _mainForm;
        private static Form2 _Instance;


        public static Form2 InstanceForm(Form1 form1, DirectoryInfo[] allDir)
        {
            if (_Instance == null || _Instance.IsDisposed)
            {
                _Instance = new Form2();
            }
            _mainForm = form1;
            _mainForm.Hide();
            _Instance.button1.Visible = false;
            _Instance.Show();
            _Instance.listBox1.Items.Clear();
            for (int i = 0; i < allDir.Length; i++)
            {
                _Instance.listBox1.Items.Insert(i, allDir[i]);
            }
            return _Instance;
        }

        private void Form2_FormClosing(object sender, FormClosingEventArgs e)
        {
            _Instance = null;
            _mainForm.Show();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            DirectoryInfo theFolder = new DirectoryInfo(_mainForm.PathShare + "/投票项目/" + listBox1.SelectedItem.ToString());
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
                        catch (Exception ex) {
                            MessageBox.Show(ex.ToString());
                        }
                    }
                }
                string pathName = executableFile.FullName.Replace(_mainForm.PathShare, _mainForm.PathShareVm);
                if (_mainForm.CheckBox3.Checked)
                {
                    _mainForm.OverSwitchPath = pathName;
                    _mainForm.CheckBox3.Text = "到票切换" + listBox1.SelectedItem.ToString();
                }
                else
                {
                    SwitchUtil.clearAutoVote(_mainForm.PathShare);
                    SwitchUtil.swichVm(_mainForm.VM1, _mainForm.VM2, _mainForm, pathName, "投票项目", _mainForm.PathShare);
                }
                _mainForm.Show();
                _Instance.Hide();
            }
            else
            {
                MessageBox.Show("所选项目中无可执行文件，请检查！");
            }
        }

        private void listBox1_SelectedIndexChanged(object sender, EventArgs e)
        {
            button1.Visible = true;
        }
    }
}
