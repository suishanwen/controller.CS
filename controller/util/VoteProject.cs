using System;
using System.Windows.Forms;

namespace controller.util
{
    class VoteProject
    {
        private string projectName;
        private double price;
        private long totalRequire;
        private long finishQuantity;
        private long remains;
        private string backgroundNo;
        private string backgroundAddress;
        private string downloadAddress;
        private bool isRestrict; // true 限人 false 不限
        private string idType;
        private int hot;
        private DateTime refreshDate;
        private int index;
        private string type;
        private bool auto;
        private bool drop;//是否拉黑
        private bool top;//是否置顶


        public VoteProject() { }

        public VoteProject(string projectName, double price, long remains, string backgroundNo)
        {
            this.projectName = projectName;
            this.price = price;
            this.remains = remains;
            this.backgroundNo = backgroundNo;
            this.index = -1;
        }


        public void setProjectType()
        {
            if (this.BackgroundAddress.IndexOf("jiutianvote.cn") != -1)
            {
                this.Type = "九天";
            }
            else if (this.BackgroundAddress.IndexOf("120.25.13.127") != -1)
            {
                this.Type = "圆球";
            }
            else if (this.BackgroundAddress.IndexOf("mmtp.com") != -1)
            {
                this.Type = "MM";
            }
            else if (this.BackgroundAddress.IndexOf("jzlsoft.com") != -1)
            {
                this.Type = "JZ";
            }
            else if (this.BackgroundAddress.IndexOf("hinyun.com") != -1)
            {
                this.Type = "HY";
            }
            else
            {
                this.Type = "未定";
            }
            this.auto = this.type == "九天" || this.type == "MM" || this.Type == "圆球";
        }

        public bool VoteRemains
        {
            get
            {
                return remains * price > 100;
            }
        }

        override
        public string ToString()
        {
            return projectName + " " + backgroundNo + " " + price + " " + remains ;
        }



        public bool Drop { get => drop; set => drop = value; }
        public bool Top { get => top; set => top = value; }
        public string ProjectName { get => projectName; set => projectName = value; }
        public double Price { get => price; set => price = value; }
        public long TotalRequire { get => totalRequire; set => totalRequire = value; }
        public long FinishQuantity { get => finishQuantity; set => finishQuantity = value; }
        public long Remains { get => remains; set => remains = value; }
        public string BackgroundNo { get => backgroundNo; set => backgroundNo = value; }
        public string BackgroundAddress { get => backgroundAddress; set => backgroundAddress = value; }
        public string DownloadAddress { get => downloadAddress; set => downloadAddress = value; }
        public bool IsRestrict { get => isRestrict; set => isRestrict = value; }
        public string IdType { get => idType; set => idType = value; }
        public int Hot { get => hot; set => hot = value; }
        public DateTime RefreshDate { get => refreshDate; set => refreshDate = value; }
        public int Index { get => index; set => index = value; }
        public string Type { get => type; set => type = value; }
        public bool Auto { get => auto; set => auto = value; }

        public static implicit operator VoteProject(DataGridViewCell v)
        {
            throw new NotImplementedException();
        }
    }
}
