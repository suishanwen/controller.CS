using controller.util;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
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
        

        public Form3()
        {
            InitializeComponent();
        }
        public Form3(Form1 form1)
        {
            _mainForm = form1;
            timer1.Enabled = true;
        }

        private void timer1_Tick(object sender, EventArgs e)
        {
            count++;
            Console.WriteLine(count);
            if (count == 3)
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
            else if (count == 5)
            {
                if (val > 0)
                {
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

                }
            }
            else if (count == 7)
            {
                if (over > 0)
                {
                    IniReadWriter.WriteIniKeys("Command", "OVER", "0", _mainForm.PathShare + "/CF.ini");
                }
                if (kick > 0)
                {
                    IniReadWriter.WriteIniKeys("Command", "KICK", "0", _mainForm.PathShare + "/CF.ini");
                }

            }
            else if (count == 9)
            {
                for (int i = int.Parse(_mainForm.VM1); i <= int.Parse(_mainForm.VM2); i++)
                {
                    string state = IniReadWriter.ReadIniKeys("Command", "TaskChange" + i, _mainForm.PathShare + "/Task.ini");
                    if (state == "1")
                    {
                    }
                }
            }
            else if (count == 12)
            {
                count = 0;
            }
        }

    }
}
