using DataLibrary;
using HtmlAgilityPack;
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
    public class CNNNewsHelper
    {
        public static List<NewsContract> resultList;
        public static bool Begin()
        {
            resultList = new List<NewsContract>();
            GetCNNNewsIndexNews();
            SendResult(resultList);
            return true;
        }

        private static void GetCNNNewsIndexNews()
        {
            ChromeOptions op = new ChromeOptions();
            op.AddArguments("--headless");
            //op.AddArguments("--window-size=1920,1080");
            ChromeDriver driver = new ChromeDriver(AppDomain.CurrentDomain.BaseDirectory.ToString(), op, TimeSpan.FromSeconds(300));

            string homeUrl = "https://edition.cnn.com/";
            driver.Navigate().GoToUrl(homeUrl);
            driver.ExecuteScript("window.scrollTo(0, document.body.scrollHeight);");

            string content = driver.PageSource;

            Regex firstSectionReg = new Regex("(?s)<section\\s+class=\"zn\\s+zn-intl_homepage1-zone-1.*?>.*?</section>");
            string firstSection = firstSectionReg.Match(content).Value;

            using (StringReader rdr = new StringReader(firstSection))
            {
                HtmlDocument doc = new HtmlDocument();
                doc.Load(rdr);
                HtmlNodeCollection headlineCollection = doc.DocumentNode.SelectNodes(".//h3[@class='cd__headline']");

                foreach (HtmlNode node in headlineCollection)
                {
                    string aticleUrl = node.SelectSingleNode("./a").GetAttributeValue("href","");
                    GetArticleDetail("https://edition.cnn.com" + aticleUrl);
                }
            }

            driver.Quit();
        }

        private static void GetArticleDetail(string url)
        {
            string content = HttpHelper.GetUrlContent(url);
            NewsContract newsDO = new NewsContract();
            newsDO.Url = url;

            if (url.Contains("/videos/"))
            {
                using (StringReader rdr = new StringReader(content))
                {
                    HtmlDocument doc = new HtmlDocument();
                    doc.Load(rdr);

                    HtmlNode titleNode = doc.DocumentNode.SelectSingleNode(".//h1[@class='media__video-headline']");
                    if (titleNode != null)
                    {
                        newsDO.Title = titleNode.InnerText;
                        Console.WriteLine("CNNHelper" + newsDO.Title);
                    }
                    HtmlNode articleCreateTimeStrNode = doc.DocumentNode.SelectSingleNode(".//p[@class='update-time']");
                    if (articleCreateTimeStrNode != null)
                    {
                        newsDO.UpdateTimeStr = articleCreateTimeStrNode.InnerText;
                        if (newsDO.UpdateTimeStr != null && newsDO.UpdateTimeStr != ""&& newsDO.UpdateTimeStr.IndexOf(')')>0)
                        {
                            string timeStr = newsDO.UpdateTimeStr.Split(')')[1];
                            newsDO.UpdateTime = timeStr.ToDateTime().ToString();
                        }
                       
                    }
                    HtmlNode authorNode = doc.DocumentNode.SelectSingleNode(".//p[@class='metadata__byline/span']");
                    if (authorNode != null)
                    {
                        newsDO.Author = authorNode.InnerText;
                    }
                    HtmlNode contentNode = doc.DocumentNode.SelectSingleNode(".//div[@class='media__video-description media__video-description--inline']");
                    if (contentNode != null)
                    {
                        newsDO.TextContent = contentNode.InnerText;
                    }
                    HtmlNodeCollection imgNodes = doc.DocumentNode.SelectNodes(".//img[@class='media__image']");
                    foreach (HtmlNode imgNode in imgNodes)
                    {
                        newsDO.ImageUrls += imgNode.GetAttributeValue("src", "") + ";";
                    }
                }
            }
            else
            {
                Regex headlineTitleReg = new Regex("<h1.*?>(?<title>.*?)</h1>");
                Regex updateTimeReg = new Regex("(?s)<span>Updated</span>(?<updateTimeStr>.*?)</div>");
                Regex authorReg = new Regex("<p\\s+data-type=\"byline-area\">(?<author>.*?)</p>");

                if (headlineTitleReg.IsMatch(content))
                {
                    newsDO.Title = headlineTitleReg.Match(content).Groups["title"].Value;
                }

                using (StringReader rdr = new StringReader(content))
                {
                    HtmlDocument doc = new HtmlDocument();
                    doc.Load(rdr);
                    //HtmlNode titleNode = doc.DocumentNode.SelectSingleNode(".//h1[@class='pg-headline']");
                    //if (titleNode != null)
                    //{
                    //    newsDO.Title = titleNode.InnerText;
                    //}
                    HtmlNode articleCreateTimeStrNode = doc.DocumentNode.SelectSingleNode(".//p[@class='update-time']");
                    string updateStr = "";

                    if (articleCreateTimeStrNode != null)
                    {
                        updateStr = articleCreateTimeStrNode.InnerText;
                    }
                    else
                    {
                        if (updateTimeReg.IsMatch(content))
                        {
                            updateStr = updateTimeReg.Match(content).Groups["updateTimeStr"].Value;
                        }

                    }
                    if (string.IsNullOrEmpty(updateStr) == false)
                    {
                        newsDO.ReleaseTimeStr = updateStr;
                        newsDO.UpdateTimeStr = updateStr;
                        if (newsDO.UpdateTimeStr != null && newsDO.UpdateTimeStr != "" && newsDO.UpdateTimeStr.IndexOf(')') > 0)
                        {
                            string timeStr = newsDO.UpdateTimeStr.Split(')')[1];
                            newsDO.ReleaseTime = timeStr.ToDateTime().ToString();
                            newsDO.UpdateTime = timeStr.ToDateTime().ToString();
                        }
                        else if (updateStr.IndexOf("ET,") > 0)
                        {
                            string timeStr = updateStr.Substring(updateStr.IndexOf("ET,") + 4);
                            newsDO.ReleaseTime = timeStr.ToDateTime().ToString();
                            newsDO.UpdateTime = timeStr.ToDateTime().ToString();
                        }
                    }
                    HtmlNode authorNode = doc.DocumentNode.SelectSingleNode(".//span[@class='metadata__byline__author']");
                    if (authorNode != null)
                    {
                        newsDO.Author = authorNode.InnerText;
                    }
                    if (string.IsNullOrEmpty(newsDO.Author))
                    {
                        if (authorReg.IsMatch(content))
                        {
                            string author = authorReg.Match(content).Groups["author"].Value;
                            author = CommonFunction.ReplaceHtmlTag(author);
                            newsDO.Author = author;
                        }
                    }

                    HtmlNode contentNode = doc.DocumentNode.SelectSingleNode(".//div[@class='l-container']");
                    if (contentNode != null)
                    {
                        newsDO.TextContent = contentNode.InnerText;
                    }
                    HtmlNodeCollection imgNodes = doc.DocumentNode.SelectNodes(".//img[@class='media__image']");
                    if(imgNodes!= null)
                    {
                        foreach (HtmlNode imgNode in imgNodes)
                        {
                            newsDO.ImageUrls += imgNode.GetAttributeValue("src", "") + ";";
                        }
                    }
                }
            }

            Regex channelNameReg = new Regex("https://edition.cnn.com/\\d{4}/\\d{2}/\\d{2}/(?<channelName>.*?)/");
            string channelName = channelNameReg.Match(content).Groups["channelName"].Value;
            newsDO.CategoryName = channelName;
            newsDO.Platform = "CNN";
            newsDO.ScreenType = "Home";
            newsDO.PositionType = "CNNTopNews";
          
            resultList.Add(newsDO);

            Console.WriteLine("CNNHelper" + newsDO.Title);
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
