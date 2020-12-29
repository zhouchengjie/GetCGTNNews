using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Security;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;

namespace GetCGTNNews
{
    public class OxylabsProxy
    {
        //https://oxylabs.cn/documentation-dcp 
        //https://intro.oxylabs.io/hc/en-us/categories/360000828819-Data-center-proxies-documentation 
        private static string GETPORTURL = "https://proxy.oxylabs.io/key/181ac4366d0ea902f02e7d783c3c76b3";
        private static string USERNAME = "chjzhou0127";
        private static string PASSWORD = "MpAMRd3yaz";
        private static string WHITELIST = "https://stats.oxylabs.io/index";

        public static OxylabsProxy CurrentProxy;
        static OxylabsProxy()
        {
            CurrentProxy = new OxylabsProxy();
        }

        private WebProxy webProxy = null;
        public DateTime lastUpdateTime = DateTime.MinValue;
        public WebProxy GetCurrentProxy()
        {
            if (CurrentProxy.webProxy == null )
            {
           
                string ip = "46.4.203.0";
                string port = "7777";
                int _port = 0;
                int.TryParse(port, out _port);
                WebProxy proxyObject = new WebProxy(ip, _port);//str为IP地址 port为端口号 代理类
                proxyObject.Credentials = new NetworkCredential(USERNAME, PASSWORD);
                CurrentProxy.webProxy = proxyObject;
                CurrentProxy.lastUpdateTime = DateTime.Now;
            }
            return CurrentProxy.webProxy;
        }

        private static string GetProxyIP(string url)
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
            if (httpResponse == null)
            {
                return bodys;
            }
            Stream st = httpResponse.GetResponseStream();
            StreamReader reader = new StreamReader(st, Encoding.GetEncoding("utf-8"));
            bodys = reader.ReadToEnd();
            return bodys;
        }

        public static bool CheckValidationResult(object sender, X509Certificate certificate, X509Chain chain, SslPolicyErrors errors)
        {
            return true;
        }
    }

}
