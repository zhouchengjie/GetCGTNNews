using DataLibrary;
using HtmlAgilityPack;
using Newtonsoft.Json.Converters;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


namespace GetCGTNNews
{
    public class CGTNGetChannelNews
    {
        private static List<CGTNNewsDO> resultList;
        public static void Begin()
        {
            Dictionary<string, string> channelDict = new Dictionary<string, string>
            {
                ["index"] = "https://www.cgtn.com",
                ["china"] = "https://www.cgtn.com/china",
                ["world"] = "https://www.cgtn.com/world",
                ["europe"] = "https://www.cgtn.com/europe",
                ["politics"] = "https://www.cgtn.com/politics",
                ["business"] = "https://www.cgtn.com/business",
                ["opinions"] = "https://www.cgtn.com/opinions",
                ["tech-sci"] = "https://www.cgtn.com/tech-sci",
                ["culture"] = "https://www.cgtn.com/culture",
                ["sports"] = "https://www.cgtn.com/sports",
                ["travel"] = "https://www.cgtn.com/travel",
            };
            foreach (KeyValuePair<string, string> pair in channelDict)
            {
                resultList = new List<CGTNNewsDO>();
                GetCGTNChannelNews(pair);
                SendResult(resultList);
            }
        }

        private static void GetCGTNChannelNews(KeyValuePair<string, string> pair)
        {
            string content = HttpHelper.GetUrlContent(pair.Value);

            using (StringReader rdr = new StringReader(content))
            {
                HtmlDocument doc = new HtmlDocument();
                doc.Load(rdr);
                HtmlNodeCollection articleNodes = doc.DocumentNode.SelectNodes(".//div[@class='cg-title']/h4/a");
                if (articleNodes != null)
                {
                    foreach (HtmlNode articleNode in articleNodes)
                    {
                        CGTNNewsDO newsDO = new CGTNNewsDO();
                        newsDO.Url = articleNode.GetAttributeValue("href","");
                        newsDO.Title = articleNode.InnerText;
                        string detailsContent = HttpHelper.GetUrlContent(newsDO.Url);
                        GetContentInfo(detailsContent,ref newsDO);
                        resultList.Add(newsDO);
                    }
                }
            }         
        }

        private static void GetContentInfo(string detailsContent, ref CGTNNewsDO firstcgtnDO)
        {
            using (StringReader rdr = new StringReader(detailsContent))
            {
                HtmlDocument doc = new HtmlDocument();
                doc.Load(rdr);

                HtmlNode authorNode = doc.DocumentNode.SelectSingleNode(".//div[@class='news-author news-text']");
                if (authorNode != null)
                {
                    firstcgtnDO.Author = authorNode.InnerText.Trim();
                }

                HtmlNode sectionNode = doc.DocumentNode.SelectSingleNode(".//span[@class='section']");
                if (sectionNode != null)
                {
                    firstcgtnDO.ScreenType = sectionNode.InnerText.Trim();
                }

                HtmlNodeCollection imgNodes = doc.DocumentNode.SelectNodes(".//div[@id='cmsMainContent']/div[@class='cmsImage']/img");
                if (imgNodes != null)
                {
                    foreach (var img in imgNodes)
                    {
                        firstcgtnDO.ImageUrls += img.GetAttributeValue("src", "");
                    }
                }

                HtmlNode sourceNode = doc.DocumentNode.SelectSingleNode(".//div[@class='sourceTextDiv']");
                if (sourceNode != null)
                {
                    firstcgtnDO.Source = sourceNode.InnerText.Trim();
                }

                HtmlNodeCollection textNodes = doc.DocumentNode.SelectNodes(".//div[@id='cmsMainContent']/div[@class='text  en']/p");
                if (textNodes != null)
                {
                    foreach (var textNode in textNodes)
                    {
                        firstcgtnDO.TextContent += textNode.InnerText;
                    }
                }

                HtmlNode dateNode = doc.DocumentNode.SelectSingleNode(".//span[@class='date']");
                if (dateNode != null)
                {
                    firstcgtnDO.UpdateTimeStr = dateNode.InnerText.Trim();
                    if (!string.IsNullOrWhiteSpace(firstcgtnDO.UpdateTimeStr))
                    {
                        DateTimeFormatInfo dtFormat = new DateTimeFormatInfo();
                        dtFormat.ShortDatePattern = "HH:mm,dd-MM-yyyy";
                        firstcgtnDO.UpdateTime = Convert.ToDateTime(firstcgtnDO.UpdateTimeStr, dtFormat);
                    }
                }
            }
        }

        private static void SendResult(List<CGTNNewsDO> resultList)
        {
            string resultUrl = System.Configuration.ConfigurationManager.AppSettings["SendResultURL"] + "?type=1";
            string postbody = JSONHelper.ObjectToJSON(resultList);
            string result = HttpHelper.Post(resultUrl, postbody);
        }
    }
}
