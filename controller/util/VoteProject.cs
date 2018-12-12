using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
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
        public VoteProject() { }

        public VoteProject(string projectName, double price, long remains, string backgroundNo)
        {
            this.projectName = projectName;
            this.price = price;
            this.remains = remains;
            this.backgroundNo = backgroundNo;
            this.index = -1;
        }

        override
        public string ToString()
        {
            return projectName + " " + backgroundNo + " " + price + " " + remains ;
        }

        public string ProjectName
        {
            get
            {
                return projectName;
            }

            set
            {
                projectName = value;
            }
        }


        public double Price
        {
            get
            {
                return price;
            }

            set
            {
                price = value;
            }
        }

        public long TotalRequire
        {
            get
            {
                return totalRequire;
            }

            set
            {
                totalRequire = value;
            }
        }

        public long FinishQuantity
        {
            get
            {
                return finishQuantity;
            }

            set
            {
                finishQuantity = value;
            }
        }

        public string BackgroundNo
        {
            get
            {
                return backgroundNo;
            }

            set
            {
                backgroundNo = value;
            }
        }

        public string BackgroundAddress
        {
            get
            {
                return backgroundAddress;
            }

            set
            {
                backgroundAddress = value;
            }
        }

        public string DownloadAddress
        {
            get
            {
                return downloadAddress;
            }

            set
            {
                downloadAddress = value;
            }
        }

        public bool IsRestrict
        {
            get
            {
                return isRestrict;
            }

            set
            {
                isRestrict = value;
            }
        }

        public string IdType
        {
            get
            {
                return idType;
            }

            set
            {
                idType = value;
            }
        }

        public DateTime RefreshDate
        {
            get
            {
                return refreshDate;
            }

            set
            {
                refreshDate = value;
            }
        }

        public long Remains
        {
            get
            {
                return remains;
            }

            set
            {
                remains = value;
            }
        }


        public int Hot
        {
            get
            {
                return hot;
            }

            set
            {
                hot = value;
            }
        }
        public int Index
        {
            get
            {
                return index;
            }

            set
            {
                index = value;
            }
        }
        public static implicit operator VoteProject(DataGridViewCell v)
        {
            throw new NotImplementedException();
        }
    }
}
