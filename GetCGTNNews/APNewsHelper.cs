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
    public class APNewsHelper
    {
        public static List<NewsContract> resultList;
        public static bool Begin()
        {
            resultList = new List<NewsContract>();
            GetAPNews();
            SendResult(resultList);
            return true;
        }

        private static void GetAPNews()
        {
            string homeUrl = "https://apnews.com/";
            string content = HttpHelper.GetUrlContent(homeUrl);
            int startIndex = content.LastIndexOf("window['titanium-state']") + 27;
            int endIndex = content.LastIndexOf("window['titanium-cacheConfig']");
            string jsonContent = content.Substring(startIndex, endIndex - startIndex);
            var listObj = JsonConvert.DeserializeObject<dynamic>(jsonContent);
            var feedList = listObj.hub.data["/"]["cards"];
            foreach (var feed in feedList) 
            {
                string categoryName = CommonFunction.GetStringValue(feed.cardTitle);
                foreach (var contentObj in feed.contents) 
                {
                    string id = CommonFunction.GetStringValue(contentObj.shortId);
                    string title = CommonFunction.GetStringValue(contentObj.headline);
                    string author = CommonFunction.GetStringValue(contentObj.bylines);
                    string url = CommonFunction.GetStringValue(contentObj.localLinkUrl);
                    string body = CommonFunction.GetStringValue(contentObj.storyHTML);
                    body = CommonFunction.NoHTML(body);
                    //string published = CommonFunction.GetStringValue(contentObj.published);
                    string updated = CommonFunction.GetStringValue(contentObj.updated);

                    DateTime updatedTime = Constants.Date_Min;
                    DateTime.TryParse(updated, out updatedTime);
                    updatedTime = updatedTime.AddHours(8);
                    string fixedUpdated = updatedTime.ToString("yyyy-MM-dd HH:mm:ss");

                    NewsContract newsDO = new NewsContract();
                    newsDO.Url = url;
                    newsDO.Author = author;
                    newsDO.Title = title;
                    newsDO.CategoryName = categoryName;
                    newsDO.TextContent = body;
                    newsDO.ReleaseTimeStr = fixedUpdated;
                    newsDO.ReleaseTime = fixedUpdated;
                    newsDO.Platform = "APNews";
                    newsDO.ScreenType = "Home";
                    newsDO.PositionType = "Most Recent";
                    resultList.Add(newsDO);

                    Console.WriteLine("APHelper" + newsDO.Title);
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
