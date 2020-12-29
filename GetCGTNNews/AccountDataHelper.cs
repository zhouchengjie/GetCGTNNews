using DataLibrary;
using Newtonsoft.Json.Converters;
using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Net;
using System.Net.Security;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace GetCGTNNews
{
    public  class AccountDataHelper
    {
        public static List<AccountInfoDataModel> resultList;

        public static void Begin()
        {
            resultList = new List<AccountInfoDataModel>();
            GetAllAccountData();
            SendResult(resultList);
        }

        private static void GetAllAccountData()
        {
            string resultUrl = System.Configuration.ConfigurationManager.AppSettings["GetAccountURL"];
            string accountJson = HttpHelper.Get(resultUrl, "");
            List<AccountInfoDO> accountList = JSONHelper.JSONToObject<List<AccountInfoDO>>(accountJson);
            //DataTable dt = BusinessLogicBase.Default.Select("select * from AccountInfo order by Platform");
            foreach (AccountInfoDO row in accountList)
            {
                string platform = row.Platform;
                string account = row.AccountName;
                string url = row.Url;
                if (platform == "Facebook")
                {
                    GetFacebookData(platform, account, url);
                }
                else if (platform == "Twitter")
                {
                    GetTwitterData(platform, account, url);
                }
                else if (platform == "YouTube")
                {
                    GetYouTubeData(platform, account, url);
                }
                else if (platform == "Instagram")
                {
                    GetInstagramData(platform, account, url);
                }
            }
        }


        private static void GetFacebookData(string platform, string account, string url)
        {
            string content = GetUrlContentFacebook(url);

            string followCount = string.Empty;
            string likeCount = string.Empty;
            if (string.IsNullOrEmpty(content) == false)
            {
                string followTag = "<div>(?<FollowCount>(\\d+|\\d,)+)\\s*位用户关注了</div>";
                string likeTag = "<div>(?<LikeCount>(\\d+|\\d,)+)\\s*位用户赞了</div>";
                Regex followReg = new Regex(followTag);
                Regex likeReg = new Regex(likeTag);
                followCount = followReg.Match(content).Groups["FollowCount"].Value;
                likeCount = likeReg.Match(content).Groups["LikeCount"].Value;
                followCount = FixNumber(followCount);
                likeCount = FixNumber(likeCount);
            }
            AccountInfoDataModel ado = new AccountInfoDataModel();
            ado.Account = account;
            //ado.CreateTime = DateTime.Now;
            ado.FollowerCount = followCount.ToInt64(-1);
            ado.Platform = platform;
            ado.Url = url;
            ado.LikeCount = likeCount.ToInt64(-1);
            resultList.Add(ado);
        }

        private static void GetTwitterData(string platform, string account, string url)
        {
            string screenName = url.Substring(url.LastIndexOf("/") + 1);
            string content = GetUrlContentTwitter(screenName);

            Regex followReg = new Regex("\"followers_count\":(?<followerCount>\\d+)");
            Regex postReg = new Regex("\"statuses_count\":(?<postCount>\\d+)");

            string postCount = string.Empty;
            string followCount = string.Empty;

            if (followReg.IsMatch(content))
            {
                followCount = followReg.Match(content).Groups["followerCount"].Value;
            }
            if (postReg.IsMatch(content))
            {
                postCount = postReg.Match(content).Groups["postCount"].Value;
            }

            AccountInfoDataModel ado = new AccountInfoDataModel();
            ado.Account = account;
            //ado.CreateTime = DateTime.Now;
            ado.FollowerCount = followCount.ToInt64(-1);
            ado.Platform = platform;
            ado.Url = url;
            ado.PostCount = postCount.ToInt(-1);
            resultList.Add(ado);
        }

        private static void GetYouTubeData(string platform, string account, string url)
        {
            string content = GetUrlContentFacebook(url);

            string followCount = string.Empty;
            string viewCount = string.Empty;
            if (string.IsNullOrEmpty(content) == false)
            {
                string followTag = "\"subscriberCountText\":\\{\"simpleText\":\"(?<FollowCount>.*?)位订阅者\"";
                string viewCountTag = "\"viewCountText\":\\{\"simpleText\":\"(?<ViewCount>.*?)次观看\"";
                Regex followReg = new Regex(followTag);
                Regex viewCountReg = new Regex(viewCountTag);
                followCount = followReg.Match(content).Groups["FollowCount"].Value;
                viewCount = viewCountReg.Match(content).Groups["ViewCount"].Value;
                followCount = FixNumber(followCount);
                if (followCount.Contains("万"))
                {
                    followCount = followCount.Replace("万", "");
                    double temp = followCount.ToDouble(-1) * 10000;
                    followCount = temp.ToString();
                }

                viewCount = FixNumber(viewCount);
            }

            AccountInfoDataModel ado = new AccountInfoDataModel();
            ado.Account = account;
            //ado.CreateTime = DateTime.Now;
            ado.FollowerCount = followCount.ToInt64(-1);
            ado.Platform = platform;
            ado.Url = url;
            ado.ViewCount = viewCount.ToInt64(-1);
            resultList.Add(ado);
        }

        private static void GetInstagramData(string platform, string account, string url)
        {
            string content = GetUrlContentFacebook(url);

            string followCount = string.Empty;
            string postCount = string.Empty;
            if (string.IsNullOrEmpty(content) == false)
            {
                string postTag = "\"edge_owner_to_timeline_media\":\\{\"count\":(?<PostCount>\\d+),\"page_info\":";
                string followTag = "\"userInteractionCount\":\"(?<FollowCount>\\d+)\"\\}\\}";
                Regex followReg = new Regex(followTag);
                Regex postReg = new Regex(postTag);
                followCount = followReg.Match(content).Groups["FollowCount"].Value;
                postCount = postReg.Match(content).Groups["PostCount"].Value;
            }
            AccountInfoDataModel ado = new AccountInfoDataModel();
            ado.Account = account;
            //ado.CreateTime = DateTime.Now;
            ado.FollowerCount = followCount.ToInt64(-1);
            ado.Platform = platform;
            ado.Url = url;
            ado.PostCount = postCount.ToInt(-1);
            resultList.Add(ado);
        }

        private static string FixNumber(string str)
        {
            return str.Replace(",", "").Trim();
        }


        public static string GetUrlContentFacebook(string url)
        {
            int tryTimes = 5;
            string result = string.Empty;
            do
            {
                try
                {
                    result = GetUrlContentFacebookInstance(url);
                }
                catch
                {
                    tryTimes--;
                }
            } while (string.IsNullOrEmpty(result) && tryTimes > 0);
            return result;
        }

        public static string GetUrlContentFacebookInstance(string url)
        {
            string bodys = "";
            string method = "GET";
            HttpWebRequest httpRequest = null;
            HttpWebResponse httpResponse = null;
            if (url.Contains("https://"))
            {
                ServicePointManager.ServerCertificateValidationCallback = new RemoteCertificateValidationCallback(CheckValidationResult);
                httpRequest = (HttpWebRequest)WebRequest.CreateDefault(new Uri(url));
            }
            else
            {
                httpRequest = (HttpWebRequest)WebRequest.Create(url);
            }
            httpRequest.Method = method;
            httpRequest.UserAgent = "Mozilla/5.0 (Windows NT 10.0; WOW64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/77.0.3865.90 Safari/537.36";
            httpRequest.Headers.Add("accept-language", "zh-CN,zh;q=0.9");
            if (0 < bodys.Length)
            {
                byte[] data = Encoding.UTF8.GetBytes(bodys);
                using (Stream stream = httpRequest.GetRequestStream())
                {
                    stream.Write(data, 0, data.Length);
                }
            }
            try
            {
                httpResponse = (HttpWebResponse)httpRequest.GetResponse();
            }
            catch (WebException ex)
            {
                httpResponse = (HttpWebResponse)ex.Response;
            }
            Stream st = httpResponse.GetResponseStream();
            StreamReader reader = new StreamReader(st, Encoding.GetEncoding("utf-8"));
            string result = reader.ReadToEnd();
            return result;
        }

        public static string GetUrlContentTwitter(string url)
        {
            int tryTimes = 5;
            string result = string.Empty;
            do
            {
                try
                {
                    result = GetUrlContentTwitterInstance(url);
                }
                catch (Exception ex)
                {
                    LogHelper.Error("GetUrlContentTwitterInstance", ex.ToString());
                    tryTimes--;
                }
            } while (string.IsNullOrEmpty(result) && tryTimes > 0);
            return result;
        }

        public static string GetUrlContentTwitterInstance(string screenName)
        {
            string url = "https://api.twitter.com/graphql/P8ph10GzBbdMqWZxulqCfA/UserByScreenName?variables=%7B%22screen_name%22%3A%22" + screenName + "%22%2C%22withHighlightedLabel%22%3Afalse%7D";
            string result = string.Empty;
            string bodys = "";
            string method = "GET";
            HttpWebRequest httpRequest = null;
            HttpWebResponse httpResponse = null;
            if (url.Contains("https://"))
            {
                ServicePointManager.ServerCertificateValidationCallback = new RemoteCertificateValidationCallback(CheckValidationResult);
                httpRequest = (HttpWebRequest)WebRequest.CreateDefault(new Uri(url));
            }
            else
            {
                httpRequest = (HttpWebRequest)WebRequest.Create(url);
            }
            httpRequest.Method = method;
            //httpRequest.KeepAlive = false;
            //httpRequest.AllowAutoRedirect = true;
            //httpRequest.CookieContainer = new System.Net.CookieContainer();
            //httpRequest.AllowAutoRedirect = false;
            //httpRequest.UseDefaultCredentials = true;
            //httpRequest.UserAgent = "Mozilla/5.0 (Windows NT 10.0; WOW64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/77.0.3865.90 Safari/537.36";
            //httpRequest.Host = "twitter.com";
            //httpRequest.Headers.Add("Cookie", "_twitter_sess=BAh7CSIKZmxhc2hJQzonQWN0aW9uQ29udHJvbGxlcjo6Rmxhc2g6OkZsYXNo%250ASGFzaHsABjoKQHVzZWR7ADoPY3JlYXRlZF9hdGwrCODApgxvAToMY3NyZl9p%250AZCIlOWJiNjU3MjdhZWYwOGU4NWQzOWU0OTk2YTc3MWI3ZDU6B2lkIiVlNTE3%250AOGVmODk1MWQ4NWI4ZmFlNmE5MTgyM2YxMmQwMw%253D%253D--710d8ec24b8ad75066994dc867eebd43213a017f; personalization_id=\"v1_EdydNvDf7yoBNFuLIHVRHA == \"; guest_id=v1%3A157646525257216177; external_referer=padhuUp37zjgzgv1mFWxJ12Ozwit7owX|0|8e8t2xd8A2w%3D; ct0=50c5e126482425a5eef31d69ce005865; _ga=GA1.2.1581139571.1576465262; _gid=GA1.2.1187619765.1576465262; gt=1206409346055315457; lang=zh-cn");
            //httpRequest.Headers.Add("upgrade-insecure-requests", "1");
            //httpRequest.Accept = "text/html,application/xhtml+xml,application/xml;q=0.9,image/webp,image/apng,*/*;q=0.8,application/signed-exchange;v=b3";
            //httpRequest.Headers.Add("accept-encoding", "gzip, deflate, br");
            //httpRequest.Headers.Add("accept-language", "zh-CN,zh;q=0.9");
            //httpRequest.Headers.Add("cache-control", "max-age=0");
            //httpRequest.Headers.Add("sec-fetch-mode", "navigate");
            //httpRequest.Headers.Add("sec-fetch-site", "same-origin");
            //httpRequest.Headers.Add("sec-fetch-user", "?1");

            httpRequest.Referer = "https://twitter.com/" + screenName;
            httpRequest.Headers.Add("origin", "https://twitter.com");
            httpRequest.Headers.Add("authorization", "Bearer AAAAAAAAAAAAAAAAAAAAANRILgAAAAAAnNwIzUejRCOuH5E6I8xnZz4puTs%3D1Zv7ttfk8LF81IUq16cHjhLTvJu4FA33AGWWjCpTnA");

            if (0 < bodys.Length)
            {
                byte[] data = Encoding.UTF8.GetBytes(bodys);
                using (Stream stream = httpRequest.GetRequestStream())
                {
                    stream.Write(data, 0, data.Length);
                }
            }
            try
            {
                httpResponse = (HttpWebResponse)httpRequest.GetResponse();
            }
            catch (WebException ex)
            {
                httpResponse = (HttpWebResponse)ex.Response;
                LogHelper.Error("2222", ex.ToString());
            }
            Stream st = httpResponse.GetResponseStream();
            StreamReader reader = new StreamReader(st, Encoding.GetEncoding("utf-8"));
            result = reader.ReadToEnd();
            //Stream st = httpResponse.GetResponseStream();
            //GZipStream gzip = new GZipStream(st, CompressionMode.Decompress);//解压缩
            //using (StreamReader reader = new StreamReader(gzip, Encoding.GetEncoding("utf-8")))//中文编码处理
            //{
            //    result = reader.ReadToEnd();
            //}
            return result;
        }

        public static string GetUrlContentTwitter2(string url)
        {
            try
            {
                WebClient webClient = new WebClient();
                Byte[] pageData = webClient.DownloadData(url);
                return Encoding.GetEncoding("utf-8").GetString(pageData);
            }
            catch (Exception ec)
            {
                throw new Exception(ec.Message.ToString());
            }
        }

        public static bool CheckValidationResult(object sender, X509Certificate certificate, X509Chain chain, SslPolicyErrors errors)
        {
            return true;
        }

        public static void SendResult(List<AccountInfoDataModel> resultList)
        {
            string resultUrl = System.Configuration.ConfigurationManager.AppSettings["SendResultURL"] + "?type=4";

            Newtonsoft.Json.Converters.IsoDateTimeConverter timeFormat = new Newtonsoft.Json.Converters.IsoDateTimeConverter();
            timeFormat.DateTimeFormat = "yyyy-MM-dd HH:mm:ss";
            string postbody = Newtonsoft.Json.JsonConvert.SerializeObject(resultList, Newtonsoft.Json.Formatting.Indented, timeFormat);
            string result = HttpHelper.Post(resultUrl, postbody);
        }
    }
}
