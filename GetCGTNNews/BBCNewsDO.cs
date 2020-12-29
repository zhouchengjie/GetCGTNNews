using DataLibrary;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GetCGTNNews
{
    public class BBCNewsDO : DataObjectBase
    {
        private int iD;
        public int ID
        {
            get { return iD; }
            set { iD = value; }
        }

        private string title;
        public string Title
        {
            get { return title; }
            set { title = value; }
        }

        public string Summary { set; get; }
        public string Category { set; get; }
        public string Content { set; get; }
        public string RelatedType { set; get; }
        public string ArticleCreateTimeStr { set; get; }
        public string ScreenType { set; get; }

        private string url;
        public string Url
        {
            get { return url; }
            set { url = value; }
        }

        private DateTime createTime;
        public DateTime ArticleCreateTime
        {
            get { return createTime; }
            set { createTime = value; }
        }
        public DateTime DataCreateTime { get; set; }

        public BBCNewsDO()
        {
            this.BO_Name = "BBCNewsArticles";
            this.PK_Name = "ID";
            this.DataCreateTime = DateTime.Now;
        }

        private string platform;
        public string Platform
        {
            get { return platform; }
            set { platform = value; }
        }


    }
}
