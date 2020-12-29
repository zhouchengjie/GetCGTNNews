using DataLibrary;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GetCGTNNews
{
    public class CNNNewsDO : DataObjectBase
    {
        public int ID { set; get; }
        public string Title { set; get; }
        public string ArticleCreateTimeStr{set;get;}
        public DateTime ArticleCreateTime { set; get; }
        public string Author { set; get; }
        public string TextContent { set; get; }
        public DateTime DataCreateTime { set; get; }
        public string Url { set; get; }
        public string CategoryName { set; get; }
        public string Platform { set; get; }
        public string ImageUrls { set; get; }
        public string Source { set; get; }

        public CNNNewsDO()
        {
            this.Title = "";
            this.BO_Name = " CNNNewsArticle";
            this.PK_Name = "ID";
            this.DataCreateTime = DateTime.Now;
            this.Platform = "CNN";
        }
    }
}
