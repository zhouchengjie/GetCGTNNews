using DataLibrary;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataLibrary
{
    public class TopScreenshotDO : DataObjectBase
    {
        public int ID { set; get; }
        public string Platform { set; get; }
        public string FileName { set; get; }
        public string WebPath { set; get; }
        public string DiskPath { set; get; }
        public DateTime CreateTime { set; get; }


        public TopScreenshotDO()
        {
            this.BO_Name = " TopScreenshot";
            this.PK_Name = "ID";
            this.CreateTime = DateTime.Now;
        }
    }
}
