using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GetCGTNNews
{
    public class NewsContract
    {
        private string title;
        public string Title
        {
            get { return title; }
            set { title = value; }
        }

        private string source;
        public string Source
        {
            get { return source; }
            set { source = value; }
        }

        private string author;
        public string Author
        {
            get { return author; }
            set { author = value; }
        }

        private string categoryName;
        public string CategoryName
        {
            get { return categoryName; }
            set { categoryName = value; }
        }

        private string coverImageUrl;
        public string CoverImageUrl
        {
            get { return coverImageUrl; }
            set { coverImageUrl = value; }
        }

        private string summary;
        public string Summary
        {
            get { return summary; }
            set { summary = value; }
        }

        private string textContent;
        public string TextContent
        {
            get { return textContent; }
            set { textContent = value; }
        }

        private string imageUrls;
        public string ImageUrls
        {
            get { return imageUrls; }
            set { imageUrls = value; }
        }

        private string releaseTimeStr;
        public string ReleaseTimeStr
        {
            get { return releaseTimeStr; }
            set
            {
                releaseTimeStr = value;
            }
        }

        private string releaseTime;
        public string ReleaseTime
        {
            get { return releaseTime; }
            set { releaseTime = value; }
        }

        private string updateTimeStr;
        public string UpdateTimeStr
        {
            get { return updateTimeStr; }
            set { updateTimeStr = value; }
        }

        private string updateTime;
        public string UpdateTime
        {
            get { return updateTime; }
            set { updateTime = value; }
        }

        private string positionType;
        public string PositionType
        {
            get { return positionType; }
            set { positionType = value; }
        }

        private string screenType;
        public string ScreenType
        {
            get { return screenType; }
            set { screenType = value; }
        }

        private string url;
        public string Url
        {
            get { return url; }
            set { url = value; }
        }

        private string createTime;
        public string CreateTime
        {
            get { return createTime; }
            set { createTime = value; }
        }

        private string platform;
        public string Platform
        {
            get { return platform; }
            set { platform = value; }
        }

        public NewsContract()
        {
            //CreateTime = DateTime.Now.ToString();
            UpdateTime = new DateTime(2000, 1, 1).ToString();
            ReleaseTime = new DateTime(2000, 1, 1).ToString();           
        }
    }
}
