using DataLibrary;
using HtmlAgilityPack;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using OpenQA.Selenium.Chrome;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;


namespace GetCGTNNews
{
    public class AFPHelper
    {
        public static List<NewsContract> resultList;
        public static bool Begin()
        {
            resultList = new List<NewsContract>();
            GetAFPNewsIndexNews();
            SendResult(resultList);
            return true;
        }

        private static void GetAFPNewsIndexNews()
        {
            Dictionary<string, string> requestList = new Dictionary<string, string>();
            requestList.Add("Lastest Wires", "https://www.afp.com/en/afp/production/getfeedajax/14/3954/grid_feed/0/50/166");
            requestList.Add("Sport", "https://www.afp.com/en/afp/production/getfeedajax/14/3955/grid_feed/0/50/193");

            foreach (KeyValuePair<string, string> pair in requestList) 
            {
                string channelName = pair.Key;
                string requestUrl = pair.Value;

                Regex itemContentReg = new Regex("(?s)<div id=\"afp_news.*?</div>.*?</div>");
                Regex titleReg = new Regex("<h3.*?>(?<title>.*?)</h3>");
                Regex dateReg = new Regex("<span class=\"dateblock.*?>(?<time>.*?)</span>");
                Regex urlReg = new Regex("<a href=\"(?<url>/en/news/.*?)\"");
                Regex bodyReg = new Regex("(?s)<p>.*?</p>");

                string pageContent = HttpHelper.GetUrlContent(requestUrl);
                var listObj = JsonConvert.DeserializeObject<dynamic>(pageContent);
                string divContent = listObj.data;
                MatchCollection itemColl = itemContentReg.Matches(divContent);
                foreach (Match mm in itemColl)
                {
                    string itemContent = mm.Value;
                    string title = titleReg.Match(itemContent).Groups["title"].Value;
                    string time = dateReg.Match(itemContent).Groups["time"].Value;
                    string url = urlReg.Match(itemContent).Groups["url"].Value;
                    title = CommonFunction.ReplaceHtmlTag(title);
                    url = "https://www.afp.com" + url;
                    string[] timeParts = time.Split('-');
                    time = timeParts[0].Trim() + " " + timeParts[1].Trim() + ":00";
                    DateTime publishDate = Constants.Date_Min;
                    DateTime.TryParse(time, out publishDate);//法国时间
                    publishDate = publishDate.AddHours(7);//北京时间

                    string body = string.Empty;
                    string itemDetailContent = HttpHelper.GetUrlContent(url);
                    MatchCollection mc = bodyReg.Matches(itemDetailContent);
                    foreach (Match pm in mc)
                    {
                        string paragraph = pm.Value;
                        if (paragraph.Contains("<p><strong>AFP</strong> is"))
                        {
                            continue;
                        }
                        paragraph = CommonFunction.ReplaceHtmlTag(paragraph);
                        body = body + paragraph + "\r\n";
                    }

                    NewsContract newsDO = new NewsContract();
                    newsDO.Url = url;
                    newsDO.Title = title;
                    newsDO.CategoryName = channelName;
                    newsDO.TextContent = body;
                    newsDO.ReleaseTimeStr = time;
                    newsDO.ReleaseTime = time;
                    newsDO.Platform = "AFP";
                    newsDO.ScreenType = "Home";
                    newsDO.PositionType = "AFPNews";
                    resultList.Add(newsDO);

                    Console.WriteLine("AFPHelper" + newsDO.Title);
                }
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
