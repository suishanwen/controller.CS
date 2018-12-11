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

        public VoteProject() { }

        override
        public string ToString()
        {
            string restrict = isRestrict?" 限人": " ";
            return projectName + " " + price + " " + remains + " " + idType + restrict + " " + refreshDate;
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
        public static implicit operator VoteProject(DataGridViewCell v)
        {
            throw new NotImplementedException();
        }
    }
}
