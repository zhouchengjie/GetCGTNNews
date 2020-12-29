using DataLibrary;
using HtmlAgilityPack;
using Newtonsoft.Json.Converters;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace GetCGTNNews
{
    public class BBCHelper
    {
        private static List<NewsContract> resultList;

        public static bool Begin()
        {
            resultList = new List<NewsContract>();
            GetBBCNewsTopNews();
            SendResult(resultList);

            return true;
        }

        private static void GetBBCNewsTopNews()
        {
            string homeUrl = "https://www.bbc.com/news";
            string content = HttpHelper.GetUrlContent(homeUrl);
            Regex topReg = new Regex(CommonFunction.GetTagExpress("div", "id", "news-top-stories-container"), RegexOptions.Singleline);
            string topContent = topReg.Match(content).Value;

            GetBBCFirstNews(topContent);
            GetBBCTopNews(topContent);
        }

        private static void GetBBCFirstNews(string topContent)
        {
            string firstClassName = GetFullClassName(topContent, "gel-layout__item\\s+nw-c-top-stories__primary-item", "div");
            Regex firstContentRex = new Regex(CommonFunction.GetTagExpress("div", "class", firstClassName), RegexOptions.Singleline);
            string firstContent = firstContentRex.Match(topContent).Value;

            Regex firstUrlRex = new Regex("<a\\s+class=\"gs-c-promo-heading.*?\"\\s+href=\"(?<firstUrl>.*?)\">");
            Regex firstTitleRex = new Regex("<h3\\s+class=\"gs-c-promo-heading__title.*?\">(?<firstTitle>.*?)</h3>");
            Regex firstSummaryRex = new Regex("<p\\s+class=\"gs-c-promo-summary.*?\">(?<firstSummary>.*?)</p>");
           // Regex firstTimeRex = new Regex("<time\\s+class=\"gs-o-bullet__text\\s+date\\s+qa-status-date\"\\s+datetime=\"(?<firstTime>.*?)\"");
            Regex firstCategoryRex = new Regex("<span\\s+aria-hidden=\"true\">(?<firstCategory>.*?)</span>");
            string firstUrl = firstUrlRex.Match(firstContent).Groups["firstUrl"].Value;
            firstUrl = "https://www.bbc.com" + firstUrl;
            string firstTitle = firstTitleRex.Match(firstContent).Groups["firstTitle"].Value;
            string firstSummary = firstSummaryRex.Match(firstContent).Groups["firstSummary"].Value;
           // string firstTime = firstTimeRex.Match(firstContent).Groups["firstTime"].Value;
            string firstCategory = firstCategoryRex.Match(firstContent).Groups["firstCategory"].Value;

            string rexTag = string.Format("<li\\s+class=\"(?<className>nw-c-related-story.*?)\"");
            Regex relatedClassNameRex = new Regex(rexTag);
            MatchCollection matches = relatedClassNameRex.Matches(firstContent);

     
            foreach (Match mc in matches)
            {
                string className = mc.Groups["className"].Value;
                Regex relatedContentRex = new Regex(CommonFunction.GetTagExpress("div", "class", className), RegexOptions.Singleline);
                string relatedContent = relatedContentRex.Match(firstContent).Value;

                Regex relatedTitleRex = new Regex("<span\\s+class=\"nw-o-link-split__text\\s+gs-u-align-bottom\">(?<relatedTitle>)</span>");
                Regex relatedUrlRex = new Regex("<a\\s+href=\"(?<relatedUrl>.*?)\"\\s+class=\"gel-layout__item.*?\">");
                Regex relatedTypeRex = new Regex("<span\\s+class=\"qa-offscreen\\s+gs-u-vh\">(?<relatedType>.*?)</span>");
                string relatedTitle = relatedTitleRex.Match(relatedContent).Groups["relatedTitle"].Value;
                string relatedUrl = relatedUrlRex.Match(relatedContent).Groups["relatedUrl"].Value;
                relatedUrl = "https://www.bbc.com" + relatedUrl;
                string relatedType = relatedTypeRex.Match(relatedContent).Groups["relatedType"].Value;

                //Search Details Page
                NewsContract newsDO = new NewsContract();
                newsDO.ScreenType = "Home";
                newsDO.Title = firstTitle;
                newsDO.Url = relatedUrl;
                newsDO.Summary = firstSummary;
                //newsDO.ArticleCreateTimeStr = firstTime;
                newsDO.CategoryName = firstCategory;
                newsDO.PositionType = "BBCFirstNews";
                GetArticleContent(relatedUrl, newsDO);

                newsDO.Platform = "BBC";
                Console.WriteLine("BCCHelper" + newsDO.Title);
                resultList.Add(newsDO);

            }
        }

        private static void GetBBCTopNews(string topContent)
        {
            string rexTag = string.Format("<div\\s+class=\"(?<className>gel-layout__item\\s+nw-c-top-stories__secondary-item.*?)\"");
            Regex relatedClassNameRex = new Regex(rexTag);
            MatchCollection classMatches = relatedClassNameRex.Matches(topContent);
            int count = classMatches.Count;
            foreach (Match classMM in classMatches)
            {
                string className = classMM.Groups["className"].Value;
                Regex topItemsContentRex = new Regex(CommonFunction.GetTagExpress("div", "class", className), RegexOptions.Singleline);
                Match mm = topItemsContentRex.Match(topContent);
                string itemContent = mm.Value;
                Regex itemUrlRex = new Regex("<a\\s+class=\"gs-c-promo-heading.*?\"\\s+href=\"(?<itemUrl>.*?)\">");
                Regex itemTitleRex = new Regex("<h3\\s+class=\"gs-c-promo-heading__title.*?\">(?<itemTitle>.*?)</h3>");
                Regex itemSummaryRex = new Regex("<p\\s+class=\"gs-c-promo-summary.*?\">(?<itemSummary>.*?)</p>");
               // Regex itemTimeRex = new Regex("<time\\s+class=\"gs-o-bullet__text\\s+date\\s+qa-status-date\"\\s+datetime=\"(?<itemTime>.*?)\"");
                Regex itemCategoryRex = new Regex("<span\\s+aria-hidden=\"true\">(?<itemCategory>.*?)</span>");
                string itemUrl = itemUrlRex.Match(itemContent).Groups["itemUrl"].Value;
                string itemTitle = itemTitleRex.Match(itemContent).Groups["itemTitle"].Value;
                string itemSummary = itemSummaryRex.Match(itemContent).Groups["itemSummary"].Value;
               // string itemTime = itemTimeRex.Match(itemContent).Groups["itemTime"].Value;
                string itemCategory = itemCategoryRex.Match(itemContent).Groups["itemCategory"].Value;
                itemUrl = "https://www.bbc.com" + itemUrl;

                //Search Details Page
                NewsContract newsDO = new NewsContract();
                newsDO.Title = itemTitle;
                newsDO.Summary = itemSummary;
                newsDO.ScreenType = "Home";
                newsDO.CategoryName = itemCategory;
                newsDO.PositionType = "BBCTopNews";
                newsDO.Url = itemUrl;
                GetArticleContent(itemUrl, newsDO);
                newsDO.Platform = "BBC";
                Console.WriteLine("BCCHelper" + newsDO.Title);
                resultList.Add(newsDO);

            }

        }

        private static void GetArticleContent(string url, NewsContract newsDO)
        {
            string content = HttpHelper.GetUrlContent(url);
            string articleContent = null;

            Regex authorReg = new Regex("<span\\s+class=\"byline__name\">(?<author>.*?)</span>");

            using (StringReader rdr = new StringReader(content))
            {
                HtmlDocument doc = new HtmlDocument();
                doc.Load(rdr);
                HtmlNode topNewsDiv = doc.DocumentNode.SelectSingleNode(".//div[@class='vxp-media__summary']");
                if(topNewsDiv != null)
                {
                    articleContent = topNewsDiv.InnerText;
                }
               

                if (articleContent == null || articleContent == "")
                {
                    topNewsDiv = doc.DocumentNode.SelectSingleNode(".//div[@class='story-body__inner']");
                    if (topNewsDiv != null)
                    {
                        articleContent = topNewsDiv.InnerText;
                    }
               
                }

                HtmlNode timeDiv = doc.DocumentNode.SelectSingleNode(".//div[@class='date date--v2']");
                if(timeDiv != null)
                {
                    newsDO.ReleaseTimeStr = timeDiv.GetAttributeValue("data-datetime", "");
                    newsDO.UpdateTimeStr = timeDiv.GetAttributeValue("data-datetime","");
                    int dataSeconds = timeDiv.GetAttributeValue("data-seconds", "").ToInt(0);
                    newsDO.ReleaseTime = (new DateTime(1970, 1, 1, 8, 0, 0).AddSeconds(dataSeconds)).ToString();
                    newsDO.UpdateTime = (new DateTime(1970 ,1 ,1,8,0,0).AddSeconds(dataSeconds)).ToString();
                }

                newsDO.Author = authorReg.Match(content).Groups["author"].Value;
            }
            if(articleContent!=null)
            {
                articleContent = articleContent.Replace("\n", "  ");
            }

            newsDO.TextContent = articleContent;
        }

        private static string GetFullClassName(string topContent, string classNameTag, string htmlTag)
        {
            string rexTag = string.Format("<{0}\\s+class=\"(?<className>{1}.*?)\"", htmlTag, classNameTag);
            Regex firstClassNameRex = new Regex(rexTag);
            string firstClassName = firstClassNameRex.Match(topContent).Groups["className"].Value;
            return firstClassName;
        }

        //private static int GetArticleIDByUrl(string Url)
        //{
        //    BBCNewsDO newsDO = new BBCNewsDO();
        //    string sql = "Select * From BBCNewsArticles Where Url = '" + Url + "'";
        //    BusinessLogicBase.Default.Select(newsDO,sql);
        //    if(newsDO != null)
        //    {
        //        return newsDO.ID;
        //    }
        //    else
        //    {
        //        return 0;
        //    }
        //}

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
