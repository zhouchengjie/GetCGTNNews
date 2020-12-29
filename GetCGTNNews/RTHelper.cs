using DataLibrary;
using HtmlAgilityPack;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using OpenQA.Selenium.Chrome;
using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;


namespace GetCGTNNews
{
    public class RTHelper
    {
        public static List<NewsContract> resultList;
        public static bool Begin()
        {
            resultList = new List<NewsContract>();
            GetRTNews();
            SendResult(resultList);
            return true;
        }

        private static void GetRTNews()
        {
            string homeUrl = "https://www.rt.com";
            string content = HttpHelper.GetUrlContent(homeUrl);
            
            Regex listReg = new Regex("<a class=\"main-promobox__link\" href=\"(?<url>.*?)\"");
            Regex titleReg = new Regex("\"headline\": \"(?<title>.*?)\"");
            Regex authorReg = new Regex("<meta name=\"article:author\" content=\"(?<author>.*?)\" />");
            Regex dateReg = new Regex("\"dateModified\": \"(?<date>.*?)\"");
            Regex bodyReg = new Regex("<p>(?<body>.*?)</p>");
            Regex categoryReg = new Regex("<meta name=\"article:section\" content=\"(?<category>.*?)\" />");

            MatchCollection mc = listReg.Matches(content);
            foreach (Match mm in mc) 
            {
                string url = mm.Groups["url"].Value;
                url = homeUrl + url;
                string itemContent = HttpHelper.GetUrlContent(url);

                string title = titleReg.Match(itemContent).Groups["title"].Value;
                string date = dateReg.Match(itemContent).Groups["date"].Value;
                string author = authorReg.Match(itemContent).Groups["author"].Value;
                string category = categoryReg.Match(itemContent).Groups["category"].Value;

                string body = string.Empty;
                MatchCollection mcBody = bodyReg.Matches(itemContent);
                foreach (Match mbody in mcBody)
                {
                    string strP = mbody.Groups["body"].Value;
                    strP = CommonFunction.NoHTML(strP);

                    body += (strP + "\r\n");
                }
                DateTime updatedTime = Constants.Date_Min;
                DateTime.TryParse(date, out updatedTime);
                //updatedTime = updatedTime.AddHours(8);

                string fixedUpdated = updatedTime.ToString("yyyy-MM-dd HH:mm:ss");

                NewsContract newsDO = new NewsContract();
                newsDO.Url = url;
                newsDO.Author = author;
                newsDO.Title = title;
                newsDO.CategoryName = category;
                newsDO.TextContent = body;
                newsDO.ReleaseTimeStr = fixedUpdated;
                newsDO.ReleaseTime = fixedUpdated;
                newsDO.Platform = "RT";
                newsDO.ScreenType = "Home";
                newsDO.PositionType = "Top";
                resultList.Add(newsDO);

                Console.WriteLine("RTHelper：" + newsDO.Title);
            }
        }

        private static void SendResult(List<NewsContract> resultList)
        {
            string resultUrl = System.Configuration.ConfigurationManager.AppSettings["SendResultURL"] + "?type=1";

            Newtonsoft.Json.Converters.IsoDateTimeConverter timeFormat = new Newtonsoft.Json.Converters.IsoDateTimeConverter();
            timeFormat.DateTimeFormat = "yyyy-MM-dd HH:mm:ss";
            string postbody = Newtonsoft.Json.JsonConvert.SerializeObject(resultList, Newtonsoft.Json.Formatting.Indented, timeFormat);

            string result = HttpHelper.Post(resultUrl, postbody);
        }
    }
}
