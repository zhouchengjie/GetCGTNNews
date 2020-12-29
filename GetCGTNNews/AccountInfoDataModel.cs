using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace DataLibrary
{

    public class AccountInfoDataModel
    {
        private int iD;
        public int ID
        {
            get { return iD; }
            set { iD = value; }
        }

        private string platform;
        public string Platform
        {
            get { return platform; }
            set { platform = value; }
        }

        private string account;
        public string Account
        {
            get { return account; }
            set { account = value; }
        }

        private string url;
        public string Url
        {
            get { return url; }
            set { url = value; }
        }

        private Int64 followerCount;
        public Int64 FollowerCount
        {
            get { return followerCount; }
            set { followerCount = value; }
        }

        private int postCount;
        public int PostCount
        {
            get { return postCount; }
            set { postCount = value; }
        }

        private Int64 viewCount;
        public Int64 ViewCount
        {
            get { return viewCount; }
            set { viewCount = value; }
        }

        private Int64 likeCount;
        public Int64 LikeCount
        {
            get { return likeCount; }
            set { likeCount = value; }
        }

        private DateTime createTime;
        public DateTime CreateTime
        {
            get { return createTime; }
            set { createTime = value; }
        }

        public AccountInfoDataModel()
        {

        }
    }
}
