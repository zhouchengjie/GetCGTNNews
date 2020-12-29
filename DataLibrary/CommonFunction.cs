using System;
using System.Data;
using System.Data.SqlClient;
using System.Configuration;
using System.Web;

using System.Globalization;
using System.Collections.Generic;
using System.Reflection;
using System.IO;
using System.Collections;
using System.Text;
using System.Text.RegularExpressions;
using System.Linq;
using System.Net;
using System.Net.Security;
using System.Security.Cryptography.X509Certificates;

namespace DataLibrary
{
    /// <summary>
    /// Summary description for CommonFunction
    /// </summary>
    public class CommonFunction
    {
        public CommonFunction()
        {
            //
            // TODO: Add constructor logic here
            //
        }

        public static decimal GetDeciamlFromCurrency(string currency, bool dividePercentage)
        {
            decimal rtn = GetDeciamlFromCurrency(currency);
            if (currency.Contains("%") == true)
            {
                if (dividePercentage == true)
                {
                    rtn = rtn / 100;
                }
            }
            return rtn;
        }
        public static decimal GetDeciamlFromCurrency(string currency)
        {
            decimal amount = 0;
            string stramount = currency;
            stramount = stramount.Replace("$", "");
            stramount = stramount.Replace("%", "");
            stramount = stramount.Replace(",", "");
            decimal.TryParse(stramount, out amount);
            return amount;
        }

        public static string GetNewPathForDuplicates(string path)
        {
            string directory = Path.GetDirectoryName(path);
            string filename = Path.GetFileNameWithoutExtension(path);
            string extension = Path.GetExtension(path);
            int counter = 1;
            string newFullPath = path;
            while (System.IO.File.Exists(newFullPath))
            {
                string newFilename = string.Format("{0}({1}){2}", filename, counter, extension);
                newFullPath = Path.Combine(directory, newFilename);
                counter++;
            }
            return newFullPath;
        }

        public static string GetStringValue(object obj)
        {
            if (obj == null)
            {
                return null;
            }
            return obj.ToString();
        }

        public static int GetIntValue(object obj)
        {
            if (obj == null)
            {
                return 0;
            }
            int val = 0;
            int.TryParse(obj.ToString(), out val);
            return val;
        }

        public static bool GetBoolValue(object obj)
        {
            if (obj == null)
            {
                return false;
            }
            bool val = false;
            bool.TryParse(obj.ToString(), out val);
            return val;
        }

        public static DateTime GetDateTimeValue(object obj) 
        {
            if (obj == null)
            {
                return Constants.Date_Null;
            }
            string strValue = obj.ToString();
            DateTime createTime = Constants.Date_Null;
            DateTime.TryParse(strValue, out createTime);
            if (createTime <= Constants.Date_Min)
            {
                double dbValue = 0;
                double.TryParse(strValue, out dbValue);
                try
                {
                    createTime = DateTime.FromOADate(dbValue);
                }
                catch { }
            }
            return createTime;
        }

        public static DateTime GetDateTimeValueForWeiboAcc(object obj)
        {
            if (obj == null)
            {
                return Constants.Date_Null;
            }
            string strValue = obj.ToString();
            string pattern = "ddd MMM dd HH':'mm':'ss zzz yyyy";
            DateTime createTime = Constants.Date_Null;
            DateTime.TryParseExact(strValue, pattern, System.Globalization.CultureInfo.InvariantCulture, System.Globalization.DateTimeStyles.None, out createTime);
            return createTime;
        }

        public static DateTime GetDateTimeValueForCGTN(object obj)
        {
            if (obj == null)
            {
                return Constants.Date_Null;
            }
            string strValue = obj.ToString();
            strValue = strValue.Replace("GMT+8", "").TrimEnd() + ":00";
            DateTime createTime = Constants.Date_Null;
            DateTime.TryParse(strValue, out createTime);
            return createTime;
        } 

        public static void StopProcess(string processName)
        {
            try
            {
                System.Diagnostics.Process[] ps = System.Diagnostics.Process.GetProcessesByName(processName);
                foreach (System.Diagnostics.Process p in ps)
                {
                    p.Kill();
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public static bool CheckValidationResult(object sender, X509Certificate certificate, X509Chain chain, SslPolicyErrors errors)
        {
            return true;
        }

        public static int GetFileType(string fileName)
        {
            int extensionIndex = fileName.LastIndexOf('.');
            string extension = fileName.Substring(extensionIndex).ToLower();
            int fileTypeID = 5;
            switch (extension)
            {
                case ".jpg":
                case ".png":
                case ".bmp":
                case ".gif":
                    fileTypeID = 1;
                    break;
                case ".mp3":
                case ".wav":
                case ".wma":
                case ".ogg":
                case ".acc":
                case ".aac":
                case ".ape":
                    fileTypeID = 2;
                    break;
                case ".mp4":
                case ".avi":
                case ".mpg":
                case ".mpeg":
                case ".mov":
                case ".wmv":
                case ".rm":
                case ".rmvb":
                case ".mkv":
                case ".3gp":
                case ".asf":
                case ".flv":
                case ".swf":
                    fileTypeID = 3;
                    break;
                case ".doc":
                case ".docx":
                case ".ppt":
                case ".pptx":
                case ".xls":
                case ".xlsx":
                case ".txt":
                case ".pdf":
                case ".xml":
                case ".csv":
                    fileTypeID = 4;
                    break;
                default:
                    fileTypeID = 5;
                    break;
            }
            return fileTypeID;
        }

        /// <summary>
        /// 正则取出content里面class=classORidvalue,或id=classORidvalue的值
        /// 大小写，是否含引号都可以匹配出来
        /// 需要转义字符的要处理
        /// 调用例子GetTagExpress("div","id","110","<div id='110'>***********</div>")
        /// </summary>
        /// <param name="htmltag">标签名，如div，p</param>
        /// <param name="classorid">输入"class"或"id"</param>
        /// <param name="classORidvalue">class或id的值</param>
        /// <returns></returns>
        public static string GetTagExpress(string htmltag, string classorid, string classORidvalue)
        {
            //<(?<ul>[\w]+)[^>]*\s[cC][lL][aA][sS][sS]=(?<Quote>["']?)productgroup-list(?(Quote)\k<Quote>)["']?[^>]*>((?<Nested><\k<ul>[^>]*>)|</\k<ul>>(?<-Nested>)|.*?)*</\k<ul>>
            string reclassORid = "[cC][lL][aA][sS][sS]";
            if (classorid.ToLower() == "id")
            {
                reclassORid = "[iI][dD]";
            }
            return string.Format("<(?<{0}>[\\w]+)[^>]*\\s{1}=(?<Quote>[\"']?){2}(?(Quote)\\k<Quote>)[\"']?[^>]*>((?<Nested><\\k<{0}>[^>]*>)|</\\k<{0}>>(?<-Nested>)|.*?)*</\\k<{0}>>", htmltag, reclassORid, classORidvalue);
        }

        public static string ReplaceHtmlTag(string html, int length = 0)
        {
            string strText = System.Text.RegularExpressions.Regex.Replace(html, "<[^>]+>", "");
            strText = System.Text.RegularExpressions.Regex.Replace(strText, "&[^;]+;", "");

            if (length > 0 && strText.Length > length)
                return strText.Substring(0, length);
            return strText;
        }

        public static string RemoveConvertChars(string result) 
        {
            string theString = result;
            theString = theString.Replace("&amp;", "&");
            theString = theString.Replace("&apos;", "'");
            theString = theString.Replace("&gt;", ">");
            theString = theString.Replace("&lt;", "<");
            theString = theString.Replace("&nbsp;", " ");
            theString = theString.Replace("&quot;", "\"");
            theString = theString.Replace("&#39;", "\'");
            theString = theString.Replace("\\\\", "\\");
            theString = theString.Replace("\\n", "\n");
            theString = theString.Replace("\\r", "\r");
            return theString;
        }

		public static string GetWeiboUrlContent(string url, string referUrl)
		{
			string method = "POST";
			string bodys = "";

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
			httpRequest.Accept = "application/json, text/javascript, */*; q=0.01";
			httpRequest.ContentLength = 0;
			httpRequest.KeepAlive = true;
			httpRequest.Host = "crawlerservice.ictr.cn";
			httpRequest.Referer = referUrl;

			httpRequest.UserAgent = "Mozilla/5.0 (Windows NT 10.0; WOW64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/69.0.3497.100 Safari/537.36";
			httpRequest.Headers.Add(HttpRequestHeader.AcceptEncoding, "gzip, deflate, br");
			httpRequest.Headers.Add(HttpRequestHeader.AcceptLanguage, "zh-CN,zh;q=0.9");
			string cookie = ConfigurationManager.AppSettings["Cookie"];
			httpRequest.Headers.Add(HttpRequestHeader.Cookie, cookie);

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
			bodys = reader.ReadToEnd();
			return bodys;
		}

		public static void GetUrlFile(string path, string url, string referUrl)
		{
			string method = "GET";
			string bodys = "";

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
			httpRequest.Accept = "text/html,application/xhtml+xml,application/xml;q=0.9,image/webp,image/apng,*/*;q=0.8";
			httpRequest.ContentLength = 0;
			httpRequest.KeepAlive = true;
			httpRequest.Host = "crawlerservice.ictr.cn";
			httpRequest.Referer = referUrl;

			httpRequest.UserAgent = "Mozilla/5.0 (Windows NT 10.0; WOW64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/69.0.3497.100 Safari/537.36";
			httpRequest.Headers.Add(HttpRequestHeader.AcceptEncoding, "gzip, deflate");
			httpRequest.Headers.Add(HttpRequestHeader.AcceptLanguage, "zh-CN,zh;q=0.9");
			string cookie = ConfigurationManager.AppSettings["Cookie"];
			httpRequest.Headers.Add(HttpRequestHeader.Cookie, cookie);

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
			if (st.CanRead == true)
			{
				byte[] buffer = new byte[1024];
				int count = st.Read(buffer, 0, 1024);
				if (count > 0)
				{
					using (FileStream fs = new FileStream(path, FileMode.OpenOrCreate, FileAccess.ReadWrite))
					{

						while (count > 0)
						{
							fs.Write(buffer, 0, count);
							count = st.Read(buffer, 0, 1024);
						}
						fs.Close();
					}
				}
			}
		}

        ///   <summary>
        ///   去除HTML标记
        ///   </summary>
        ///   <param   name=”NoHTML”>包括HTML的源码   </param>
        ///   <returns>已经去除后的文字</returns>
        public static string NoHTML(string Htmlstring)
        {
            //删除脚本
            Htmlstring = Regex.Replace(Htmlstring, @"<script[^>]*?>.*?</script>", "",
            RegexOptions.IgnoreCase);
            //删除HTML 
            Htmlstring = Regex.Replace(Htmlstring, @"<(.[^>]*)>", "",
            RegexOptions.IgnoreCase);
            Htmlstring = Regex.Replace(Htmlstring, @"([\r\n])[\s]+", "",
            RegexOptions.IgnoreCase);
            Htmlstring = Regex.Replace(Htmlstring, @"–>", "", RegexOptions.IgnoreCase);
            Htmlstring = Regex.Replace(Htmlstring, @"<!–.*", "", RegexOptions.IgnoreCase);
            Htmlstring = Regex.Replace(Htmlstring, @"&(quot|#34);", "\"",
            RegexOptions.IgnoreCase);
            Htmlstring = Regex.Replace(Htmlstring, @"&(amp|#38);", "&",
            RegexOptions.IgnoreCase);
            Htmlstring = Regex.Replace(Htmlstring, @"&(lt|#60);", "<",
            RegexOptions.IgnoreCase);
            Htmlstring = Regex.Replace(Htmlstring, @"&(gt|#62);", ">",
            RegexOptions.IgnoreCase);
            Htmlstring = Regex.Replace(Htmlstring, @"&(nbsp|#160);", "   ",
            RegexOptions.IgnoreCase);
            Htmlstring = Regex.Replace(Htmlstring, @"&(iexcl|#161);", "\xa1", RegexOptions.IgnoreCase);
            Htmlstring = Regex.Replace(Htmlstring, @"&(cent|#162);", "\xa2", RegexOptions.IgnoreCase);
            Htmlstring = Regex.Replace(Htmlstring, @"&(pound|#163);", "\xa3", RegexOptions.IgnoreCase);
            Htmlstring = Regex.Replace(Htmlstring, @"&(copy|#169);", "\xa9", RegexOptions.IgnoreCase);
            Htmlstring = Regex.Replace(Htmlstring, @"&#(\d+);", "", RegexOptions.IgnoreCase);

            Htmlstring = Htmlstring.Replace("&lt;", "<");
            Htmlstring = Htmlstring.Replace("&gt;", ">");
            Htmlstring = Htmlstring.Replace("&apos;", "'");
            Htmlstring = Htmlstring.Replace("&quot;", "\"");
            Htmlstring = Htmlstring.Replace("&amp;", "&");
            Htmlstring = Htmlstring.Replace("&hellip;", "...");

            Htmlstring = Htmlstring.Replace("<", "");
            Htmlstring = Htmlstring.Replace(">", "");
            Htmlstring = Htmlstring.Replace("\r\n", "");
            Htmlstring = Htmlstring.Replace("\n", "");

            return Htmlstring.Trim();
        }

    }
}