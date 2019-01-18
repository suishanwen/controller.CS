using controller.util;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;

namespace controller
{
    public partial class Form4 : Form
    {
        private static List<BlackMap> dataSource = new List<BlackMap>();
        private static Form4 _Instance;
        private static bool init;

        public Form4()
        {
            InitializeComponent();
        }

        internal static void InstanceForm()
        {
            if (_Instance == null || _Instance.IsDisposed)
            {
                _Instance = new Form4();
            }
            _Instance.Show();
        }

        private void Form4_Load(object sender, EventArgs e)
        {
            this.TopMost = true;
            init = false;
            dataSource = new List<BlackMap>();
            string data = IniReadWriter.ReadIniKeys("Command", "blackStratage",  Form1.GetPathShare() + "/AutoVote.ini");
            if (!StringUtil.isEmpty(data))
            {
                string[] arr = data.Split('|');
                foreach(string infos in arr)
                {
                    string[] info = infos.Split('-');
                    string name = info[0];
                    double price = 0;
                    try
                    {
                        price = double.Parse(info[1]);
                    }
                    catch (Exception) { };
                    dataSource.Add(new BlackMap(name,price));
                }
            }
            this.dataGridView1.DataSource = new BindingList<BlackMap>(dataSource);
            init = true;
        }

        private void setData()
        {
            if (!init)
            {
                return;
            }
            string data = "";
            dataSource.ForEach(blackMap=>{
                data += $"{blackMap.Name}-{blackMap.Price}|";
            });
            if (data.Length > 0)
            {
                data = data.Substring(0, data.Length - 1);
            }
            IniReadWriter.WriteIniKeys("Command", "blackStratage", data, Form1.GetPathShare()+ "/AutoVote.ini");
        }

        private void dataGridView1_CellValueChanged(object sender, DataGridViewCellEventArgs e)
        {
            setData();
        }

        private void dataGridView1_RowsRemoved(object sender, DataGridViewRowsRemovedEventArgs e)
        {
            setData();
        }
    }
}
