using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;

namespace DataLibrary
{

    public class CGTNNewsDO : DataObjectBase
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
            set { releaseTimeStr = value;
            }
        }

        private DateTime releaseTime;
        public DateTime ReleaseTime
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

        private DateTime updateTime;
        public DateTime UpdateTime
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

        private DateTime createTime;
        public DateTime CreateTime
        {
            get { return createTime; }
            set { createTime = value; }
        }


        public CGTNNewsDO()
        {
            this.BO_Name = "CGTNNews";
            this.PK_Name = "ID";
            this.CreateTime = DateTime.Now;
        }

        private string platform;
        public string Platform
        {
            get { return platform; }
            set {platform = value;}
        }
    }
}
