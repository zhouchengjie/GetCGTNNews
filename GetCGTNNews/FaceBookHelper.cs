using DataLibrary;
using HtmlAgilityPack;
using OpenQA.Selenium.Chrome;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GetCGTNNews
{
    public class FaceBookHelper
    {
        public static void Begin()
        {
            //string filePath = @"C:\Users\zhanghong3168\Desktop\脸书.xlsx";
            //List<string> urlList = ExcelHelper.LoadExcel(filePath, 9);
            //foreach (string url in urlList)
            //{
            //    GetArtilce(url);
            //}
        }

        private static void GetArtilce(string url)
        {
            ChromeOptions op = new ChromeOptions();
            //op.AddArguments("--headless");
            //op.AddArguments("--window-size=1920,1080");
            ChromeDriver driver = new ChromeDriver(AppDomain.CurrentDomain.BaseDirectory.ToString(), op);
            driver.Navigate().GoToUrl(url);
            string content = driver.PageSource;
            using (StringReader rdr = new StringReader(content))
            {
                HtmlDocument doc = new HtmlDocument();
                doc.Load(rdr); 
                HtmlNode contentNode = doc.DocumentNode.SelectSingleNode(".//div[@class='userContentWrapper']");
                if(contentNode!=null)
                {
                    HtmlNode likeSpan = contentNode.SelectSingleNode(".//span[@class='_81hb']");
                    if (likeSpan != null)
                    {              
                        string likeNum = likeSpan.InnerText;
                    }
                    HtmlNode commentSpan = contentNode.SelectSingleNode(".//span[@class='_3hg- _42ft']");
                    if (commentSpan != null)
                    {
                        string commentNum = commentSpan.InnerText;
                    }
                    HtmlNode shareSpan = contentNode.SelectSingleNode(".//span[@class='_355t _4vn2']");
                    if (shareSpan != null)
                    {
                        string shareNum = shareSpan.InnerText;
                    }
                }         
            }

        }
    }
}
