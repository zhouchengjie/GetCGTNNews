using DataLibrary;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.IO;
using System.Linq;
using System.Text;
using System.Web;
using System.Web.SessionState;

namespace DataPublish.Logic
{

    public class GetNewsFromSpider : IHttpHandler, IRequiresSessionState
    {
        public bool IsReusable
        {
            get
            {
                return false;
            }
        }

        public void ProcessRequest(HttpContext context)
        {
            if (string.IsNullOrEmpty(context.Request["type"]))
            {
                return;
            }
            try
            {
                string json = "";
                LogHelper.Debug("GetDataFromSpider", "Log");
                StringBuilder rsb = new StringBuilder();
                var mRequest = context.Request;
                int bytelengg = (int)mRequest.InputStream.Length;
                using (var reader = new StreamReader(mRequest.InputStream, Encoding.UTF8))
                {
                    var read = new Char[bytelengg];
                    var count = reader.Read(read, 0, bytelengg);
                    while (count > 0)
                    {
                        var str = new string(read, 0, count);
                        rsb.Append(str);
                        count = reader.Read(read, 0, bytelengg);
                    }
                    reader.Close();
                    reader.Dispose();
                    mRequest.InputStream.Close();
                    mRequest.InputStream.Dispose();
                }
                json = rsb.ToString();

                ////去除首未双引号
                //if (json[0] == '\"')
                //{
                //    json = json.Substring(1, json.Length - 1);
                //}
                //if (json[json.Length - 1] == '\"')
                //{
                //    json = json.Substring(0, json.Length - 1);
                //}
                //json = json.Replace("\\\"", "\"");//去除字符串中自带的转义字符 但当数据中有英文"时会导致解析错误
                //json = json.Replace(".0", "");
                //json = json.Replace("null", "\"\"");

                int requestType = Convert.ToInt32(context.Request["type"]);
             
                switch (requestType)
                {
                    //CGTN头条
                    case 1:
                        GetCGTNNewsFromSpider(json);
                        break;
                    //BCC头条
                    case 2:
                         GetBBCNewsFromSpider(json);
                        break;
                    //CNN头条
                    case 3:
                        GetCNNNewsFromSpider(json);
                        break;
                     //账号信息
                    case 4:
                         GetAccountData(json);
                        break;
                    //首页截图
                    case 5:
                         GetScreenshot(context);
                        break;
                    default:
                        break;
                }
                string result = "{\"code\": 0, \"msg\": \"success\", \"count\": 0 }"; ;
                HttpContext.Current.Response.ContentType = "text/plain";
                HttpContext.Current.Response.Write(result);
            }
            catch (Exception ex)
            {
                LogHelper.Debug("GetDataFromSpider", ex.ToString());
                string result = "{\"code\": 0, \"msg\": \"false\", \"count\": 0 }"; ;
                HttpContext.Current.Response.ContentType = "text/plain";
                HttpContext.Current.Response.Write(result);
            }
        }

        private static void GetCGTNNewsFromSpider(string json)
        {
            //List<CGTNNewsDO> resultList = Newtonsoft.Json.JsonConvert.DeserializeObject<List<CGTNNewsDO>>(json);
            //List<CGTNNewsDO> resultList = JSONHelper.JSONToObject<List<CGTNNewsDO>>(json);
            var listObj = Newtonsoft.Json.JsonConvert.DeserializeObject<dynamic>(json);
            foreach (var newsObj in listObj)
            {
                CGTNNewsDO newsDO = new CGTNNewsDO();
                newsDO.Author = CommonFunction.GetStringValue(newsObj.Author);
                newsDO.Title = CommonFunction.GetStringValue(newsObj.Title);
                newsDO.Source = CommonFunction.GetStringValue(newsObj.Source);
                newsDO.CategoryName = CommonFunction.GetStringValue(newsObj.CategoryName);
                newsDO.CoverImageUrl = CommonFunction.GetStringValue(newsObj.CoverImageUrl);
                //newsDO.CreateTime = CommonFunction.GetDateTimeValue(newsObj.CreateTime);
                newsDO.ImageUrls = CommonFunction.GetStringValue(newsObj.ImageUrls);
                newsDO.Platform = CommonFunction.GetStringValue(newsObj.Platform);
                newsDO.PositionType = CommonFunction.GetStringValue(newsObj.PositionType);
                newsDO.ReleaseTime = CommonFunction.GetDateTimeValue(newsObj.ReleaseTime);
                newsDO.ReleaseTimeStr = CommonFunction.GetStringValue(newsObj.ReleaseTimeStr);
                newsDO.ScreenType = CommonFunction.GetStringValue(newsObj.ScreenType);
                newsDO.Summary = CommonFunction.GetStringValue(newsObj.Summary);
                newsDO.TextContent = CommonFunction.GetStringValue(newsObj.TextContent);
                newsDO.UpdateTime = CommonFunction.GetDateTimeValue(newsObj.UpdateTime);
                newsDO.UpdateTimeStr = CommonFunction.GetStringValue(newsObj.UpdateTimeStr);
                newsDO.Url = CommonFunction.GetStringValue(newsObj.Url);
                if (GetCGTNArticleIDByUrl(newsDO.Url)<=0)
                {
                    BusinessLogicBase.Default.Insert(newsDO);
                }
            }
        }

        private static int GetCGTNArticleIDByUrl(string Url)
        {
            CGTNNewsDO newsDO = new CGTNNewsDO();
            string sql = "Select * From CGTNNews Where Url = '" + Url + "'";
            BusinessLogicBase.Default.Select(newsDO, sql);
            if (newsDO != null)
            {
                return newsDO.ID;
            }
            else
            {
                return 0;
            }
        }

        private static void GetBBCNewsFromSpider(string json)
        {
            List<BBCNewsDO> resultList = Newtonsoft.Json.JsonConvert.DeserializeObject<List<BBCNewsDO>>(json);
            foreach (BBCNewsDO news in resultList)
            {
                if (GetBBCArticleIDByUrl(news.Url) <= 0)
                {
                    BusinessLogicBase.Default.Insert(news);
                }
            }
        }

        private static int GetBBCArticleIDByUrl(string Url)
        {
            CGTNNewsDO newsDO = new CGTNNewsDO();
            string sql = "Select * From BBCNewsArticles Where Url = '" + Url + "'";
            BusinessLogicBase.Default.Select(newsDO, sql);
            if (newsDO != null)
            {
                return newsDO.ID;
            }
            else
            {
                return 0;
            }
        }

        private static void GetCNNNewsFromSpider(string json)
        {
            List<CNNNewsDO> resultList = Newtonsoft.Json.JsonConvert.DeserializeObject<List<CNNNewsDO>>(json);
            foreach (CNNNewsDO news in resultList)
            {
                if (GetCNNArticleIDByUrl(news.Url) <= 0)
                {
                    BusinessLogicBase.Default.Insert(news);
                }
            }
        }

        private static int GetCNNArticleIDByUrl(string Url)
        {
            CGTNNewsDO newsDO = new CGTNNewsDO();
            string sql = "Select * From CNNNewsArticle Where Url = '" + Url + "'";
            BusinessLogicBase.Default.Select(newsDO, sql);
            if (newsDO != null)
            {
                return newsDO.ID;
            }
            else
            {
                return 0;
            }
        }

        private static void GetAccountData(string json)
        {
            List<AccountInfoDataDO> resultList = Newtonsoft.Json.JsonConvert.DeserializeObject<List<AccountInfoDataDO>>(json);
            foreach (AccountInfoDataDO account in resultList)
            {
                account.CreateTime = DateTime.Now;
                BusinessLogicBase.Default.Insert(account);
            }
        }

        private static void GetScreenshot(HttpContext context)
        {
            if (context.Request.Files.Count > 0)
            {
                string platform = context.Request["platform"];
                var file = context.Request.Files[0];
                string originalfileName = file.FileName;
                string extension = Path.GetExtension(originalfileName);

                DateTime createTime = DateTime.Now;
                string strDateTime = createTime.ToString("yyyyMMddhhmmss");
                string fileName = platform + "_" + strDateTime + extension;

                //保存到本地或服务器
                string screenshotFolderPath = HttpContext.Current.Server.MapPath(Constants.RelevantScreenshotPath);
                if (System.IO.Directory.Exists(screenshotFolderPath) == false)
                {
                    System.IO.Directory.CreateDirectory(screenshotFolderPath);
                }
                string screenshotPath = screenshotFolderPath + fileName;
                file.SaveAs(screenshotPath);

                TopScreenshotDO screenshotDO = new TopScreenshotDO();
                screenshotDO.CreateTime = createTime;
                screenshotDO.FileName = fileName;
                screenshotDO.Platform = platform;
                screenshotDO.DiskPath = screenshotPath;
                screenshotDO.WebPath = Constants.RelevantScreenshotPath.Replace("~", "..") + fileName;
                BusinessLogicBase.Default.Insert(screenshotDO);
            }
        }
    }
}