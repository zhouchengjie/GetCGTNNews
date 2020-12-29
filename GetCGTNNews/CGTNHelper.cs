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
    public class CGTNHelper
    {
        private static List<NewsContract> resultList;

        public static bool Begin()
        {
            resultList = new List<NewsContract>();
            GetCGTNTopNews();
            SendResult(resultList);
            return true;
        }

        private static void GetCGTNTopNews()
        {
            string homeUrl = "https://www.cgtn.com/";
            GetScreenPage(homeUrl, "Home");
        }

        private static void GetScreenPage(string url, string screenType)
        {
            string content = HttpHelper.GetUrlContent(url);
            using (StringReader rdr = new StringReader(content))
            {
                HtmlDocument doc = new HtmlDocument();
                doc.Load(rdr);
                GetTopNews(doc, screenType);
                GetBreakingNews(doc, screenType);
            }
            // GetFirstScreenItem(content, screenType);
            // GetTopScreenItems(content, screenType);
        }

        private static void GetTopNews(HtmlDocument doc, string screenType)
        {
            HtmlNodeCollection topNewsDiv = doc.DocumentNode.SelectNodes(".//div[@class='topNews-item']");
            foreach (HtmlNode div in topNewsDiv)
            {
                NewsContract firstcgtnDO = new NewsContract();
                firstcgtnDO.Platform = "CGTN";
                firstcgtnDO.CategoryName = div.SelectSingleNode(".//span[@class='property']").InnerText.Trim();
                firstcgtnDO.CoverImageUrl = div.SelectSingleNode(".//img[@class='swiper-lazy']").GetAttributeValue("data-src", "");
                firstcgtnDO.PositionType = "TopBannerNews";

                firstcgtnDO.ReleaseTimeStr = div.SelectSingleNode(".//span[@class='publishTime']").InnerText.Trim();
                DateTimeFormatInfo dtFormat = new DateTimeFormatInfo();
                dtFormat.ShortDatePattern = "HH:mm,dd-MM-yyyy";
                DateTime releaseTime = Convert.ToDateTime(firstcgtnDO.ReleaseTimeStr, dtFormat);
                firstcgtnDO.ReleaseTime = releaseTime.ToString();

                firstcgtnDO.ScreenType = screenType;
                firstcgtnDO.Title = div.SelectSingleNode(".//div[@class='topNews-item-content-title']/a").InnerText.Trim();
                firstcgtnDO.Url = div.SelectSingleNode(".//div[@class='topNews-item-content-title']/a").GetAttributeValue("href", "");
                firstcgtnDO.CreateTime = DateTime.Now.ToString();
                string detailsContent = HttpHelper.GetUrlContent(firstcgtnDO.Url);
                GetContentInfo(detailsContent, ref firstcgtnDO);

                firstcgtnDO.Platform = "CGTN";
                Console.WriteLine("CGTNHelper" + firstcgtnDO.Title);
                resultList.Add(firstcgtnDO);
                
            }
        }

        private static void GetBreakingNews(HtmlDocument doc, string screenType)
        {
            string positionType = "BreakingNews";
            DateTimeFormatInfo dtFormat = new DateTimeFormatInfo();
            dtFormat.ShortDatePattern = "HH:mm,dd-MM-yyyy";
            //右上角第一条
            HtmlNode node1 = doc.DocumentNode.SelectSingleNode(".//div[@class='breakingNews-item']");
            NewsContract firstcgtnDO = new NewsContract();
            firstcgtnDO.Platform = "CGTN";
            firstcgtnDO.CategoryName = node1.SelectSingleNode(".//span[@class='property']").InnerText.Trim();
            firstcgtnDO.CoverImageUrl = node1.SelectSingleNode(".//img[@class='swiper-lazy']").GetAttributeValue("data-src", "");
            firstcgtnDO.PositionType = positionType;
            firstcgtnDO.ReleaseTimeStr = node1.SelectSingleNode(".//span[@class='publishTime']").InnerText.Trim();
            DateTime releaseTime = Convert.ToDateTime(firstcgtnDO.ReleaseTimeStr, dtFormat);
            firstcgtnDO.ReleaseTime = releaseTime.ToString();
            firstcgtnDO.ScreenType = screenType;
            firstcgtnDO.Title = node1.SelectSingleNode(".//p[@class='title']/a").InnerText.Trim();
            firstcgtnDO.Url = node1.SelectSingleNode(".//p[@class='title']/a").GetAttributeValue("href", ""); ;
            firstcgtnDO.CreateTime = DateTime.Now.ToString();
            string detailsContent = HttpHelper.GetUrlContent(firstcgtnDO.Url);
            GetContentInfo(detailsContent, ref firstcgtnDO);
            firstcgtnDO.Platform = "CGTN";
            Console.WriteLine("CGTNHelper" + firstcgtnDO.Title);
            resultList.Add(firstcgtnDO);
            


            //右上角第二条
            HtmlNode node2 = doc.DocumentNode.SelectSingleNode(".//div[@class='breakingNews-content-happening-noLiveBlog']");
            NewsContract firstcgtnDO2 = new NewsContract();
            firstcgtnDO2.Platform = "CGTN";
            firstcgtnDO2.CategoryName = node2.SelectSingleNode(".//div[@class='breakingNews-property']").InnerText.Trim();
            firstcgtnDO2.PositionType = positionType;
            firstcgtnDO2.ReleaseTimeStr = node2.SelectSingleNode(".//p[@class='breakingNews-publishTime']").InnerText.Trim();  
            DateTime releaseTime2 = Convert.ToDateTime(firstcgtnDO2.ReleaseTimeStr, dtFormat);
            firstcgtnDO2.ReleaseTime = releaseTime2.ToString();
            firstcgtnDO2.ScreenType = screenType;
            firstcgtnDO2.Title = node2.SelectSingleNode(".//div[@class='breakingNews-shortHeadline-noLiveBlog']/a").InnerText.Trim();
            firstcgtnDO2.Url = node2.SelectSingleNode(".//div[@class='breakingNews-shortHeadline-noLiveBlog']/a").GetAttributeValue("href", ""); ;
            firstcgtnDO2.CreateTime = DateTime.Now.ToString();
            string detailsContent2 = HttpHelper.GetUrlContent(firstcgtnDO2.Url);
            GetContentInfo(detailsContent2, ref firstcgtnDO2);
            firstcgtnDO2.Platform = "CGTN";
            Console.WriteLine("CGTNHelper" + firstcgtnDO2.Title);
            resultList.Add(firstcgtnDO2);

            //下方四条
            HtmlNodeCollection nodes3 = doc.DocumentNode.SelectNodes(".//div[starts-with(@class, 'row col-lg-12 col-md-12 col-sm-16 col-xs-48')]");
            if (nodes3 != null)
            {
                foreach (var item in nodes3)
                {
                    NewsContract firstcgtnDO3 = new NewsContract();
                    firstcgtnDO3.Platform = "CGTN";
                    firstcgtnDO3.CategoryName = item.SelectSingleNode(".//div[@class='cg-newsCategory']").InnerText.Trim();
                    firstcgtnDO3.PositionType = positionType;
                    HtmlNode timeNode = item.SelectSingleNode(".//div[@class='cg-time']");
                    if (timeNode != null)
                    {
                        firstcgtnDO3.ReleaseTimeStr = timeNode.InnerText.Trim();
                        DateTime releaseTime3 = Convert.ToDateTime(firstcgtnDO3.ReleaseTimeStr, dtFormat);
                        firstcgtnDO3.ReleaseTime = releaseTime3.ToString();
                    }
                    HtmlNode imgNode = item.SelectSingleNode(".//img[@class='lazy']");
                    if (imgNode != null)
                    {
                        firstcgtnDO3.CoverImageUrl = imgNode.InnerText.Trim();
                    }
                    firstcgtnDO3.ScreenType = screenType;
                    firstcgtnDO3.Title = item.SelectSingleNode(".//div[@class='cg-title']/h4/a").InnerText.Trim();
                    firstcgtnDO3.Url = item.SelectSingleNode(".//div[@class='cg-title']/h4/a").GetAttributeValue("href", "");
                    firstcgtnDO3.CreateTime = DateTime.Now.ToString();
                    string detailsContent3 = HttpHelper.GetUrlContent(firstcgtnDO3.Url);
                    GetContentInfo(detailsContent3, ref firstcgtnDO3);
                    firstcgtnDO3.Platform = "CGTN";
                    Console.WriteLine("CGTNHelper" + firstcgtnDO3.Title);
                    resultList.Add(firstcgtnDO3);

                }
            }
        }

        private static void GetContentInfo(string detailsContent, ref NewsContract firstcgtnDO)
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
                        firstcgtnDO.UpdateTime = Convert.ToDateTime(firstcgtnDO.UpdateTimeStr, dtFormat).ToString();
                    }
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
