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

        public static bool CheckValidationResult(object sender, X509Certificate certificate, X509Chain chain, SslPolicyErrors errors)
        {
            return true;
        }
    }
