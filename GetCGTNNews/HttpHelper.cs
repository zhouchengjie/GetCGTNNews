using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Security;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;

namespace GetCGTNNews
{
    public class HttpHelper
    {
        public static string GetUrlContentInstance(string url)
        {
            string bodys = "";
            string method = "GET";
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
            // httpRequest.Proxy = RMProxy.CurrentProxy.GetCurrentProxy();
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
            string result = reader.ReadToEnd();
            return result;
        }


        public static bool CheckValidationResult(object sender, X509Certificate certificate, X509Chain chain, SslPolicyErrors errors)
        {
            return true;
        }

        public static string GetUrlContent(string url)
        {
            int tryTimes = 5;
            string result = string.Empty;
            do
            {
                try
                {
                    result = GetUrlContentInstance(url);
                }
                catch
                {
                    tryTimes--;
                }
            } while (string.IsNullOrEmpty(result) && tryTimes > 0);
            return result;
        }

        static HttpHelper()
        {
            System.Net.ServicePointManager.ServerCertificateValidationCallback =
                            new System.Net.Security.RemoteCertificateValidationCallback(RemoteCertificateValidationCallback);
        }
        public static string DELETE(string url, string data, string charsSet = "utf-8")
        {
            return Response(url, data, null, "DELETE", charsSet);
        }

        public static string Get(string url, string data, HttpCookieCollection cookies, string charsSet = "utf-8")
        {
            return Response(url, data, cookies, "GET", charsSet);
        }
        public static string Get(string url, string data, string charsSet = "utf-8")
        {
            return Get(url, data, null, charsSet);
        }
        public static string Post(string url, string data, string charsSet = "utf-8", string conentType = "", NameValueCollection header = null)
        {
            return Post(url, data, null, charsSet, conentType, header);
        }
        public static string Post(string url, string data, HttpCookieCollection cookies, string charsSet = "utf-8", string contentType = "", NameValueCollection header = null)
        {
            return Response(url, data, cookies, "POST", charsSet, contentType, header);
        }
        /// <summary>
        /// 向指定地址发送POST请求
        /// </summary>
        /// <param name="getUrl">指定的网页地址</param>
        /// <param name="postData">POST的数据（格式为：p1=v1&p1=v2）</param>
        /// <param name="chars_set">可采用如UTF-8,GB2312,GBK等</param>
        /// <returns>页面返回内容</returns>
        public static string Response(string url, string postData, HttpCookieCollection cookies, string method = "POST", string charsSet = "utf-8", string contentType = "application/x-www-form-urlencoded", NameValueCollection header = null)
        {

            Encoding encoding = Encoding.GetEncoding(charsSet);
            HttpWebRequest Request;
            if (url.StartsWith("https", StringComparison.CurrentCultureIgnoreCase))
            {
                //是https请求的时候
                ServicePointManager.ServerCertificateValidationCallback = new RemoteCertificateValidationCallback(RemoteCertificateValidationCallback);
                Request = WebRequest.Create(url) as HttpWebRequest;
                Request.ProtocolVersion = HttpVersion.Version11;
                //LogHelper.LogInfo("HttpHelper Response url=" + url);
            }
            else
                Request = (HttpWebRequest)WebRequest.Create(url);
            //设置CookieContainer，商城页面使用了cookie，此处不设置CookieContainer会请求失败
            Request.CookieContainer = new CookieContainer();
            Request.Method = method;
            if (!string.IsNullOrEmpty(contentType))
            {
                Request.ContentType = contentType;//"application/x-www-form-urlencoded";
            }
            else Request.ContentType = "application/x-www-form-urlencoded";
            Request.AllowAutoRedirect = true;
            if (header != null)
            {//增加请求头
                Request.Headers.Add(header);
            }
            byte[] postdata = encoding.GetBytes(postData);
            if (!method.Equals("get", StringComparison.CurrentCultureIgnoreCase) || !string.IsNullOrEmpty(postData))
            {
                using (Stream newStream = Request.GetRequestStream())
                {
                    newStream.Write(postdata, 0, postdata.Length);
                }
            }
            using (HttpWebResponse response = (HttpWebResponse)Request.GetResponse())
            {

                using (Stream stream = response.GetResponseStream())
                {
                    using (StreamReader reader = new StreamReader(stream, encoding, true))
                    {
                        return reader.ReadToEnd();
                    }
                }
            }

        }

        public static bool RemoteCertificateValidationCallback(
            Object sender,
            X509Certificate certificate,
            X509Chain chain,
            System.Net.Security.SslPolicyErrors sslPolicyErrors)
        {
            return true;
        }

        public static byte[] Post4ReturnByteArray(string url, string para)
        {
            using (WebClient wc = new WebClient())
            {
                NameValueCollection data = new NameValueCollection();
                string[] paraArray = para.Split('&');
                foreach (string paraTemp in paraArray)
                {
                    string[] paraTempArray = paraTemp.Split('=');
                    if (paraTempArray.Length >= 2)
                    {
                        string name = paraTempArray[0];
                        string value = paraTemp.Substring(name.Length + 1);
                        data[name] = value;
                    }
                }
                byte[] bs = wc.UploadValues(url, "POST", data);
                return bs;
            }
        }


        /// <summary>
        ///获取返回到浏览器的URL地址
        /// </summary>
        /// <param name="Url">地址</param>
        /// <param name="postDataStr">参数</param>
        /// <returns></returns>
        public static string GetBrowserUrl(string Url, string postDataStr)
        {
            try
            {
                HttpWebRequest request = (HttpWebRequest)WebRequest.Create(Url);
                request.Method = "POST";
                request.ContentType = "application/x-www-form-urlencoded";
                request.ContentLength = Encoding.UTF8.GetByteCount(postDataStr);
                Stream myRequestStream = request.GetRequestStream();
                StreamWriter myStreamWriter = new StreamWriter(myRequestStream, Encoding.GetEncoding("gb2312"));
                myStreamWriter.Write(postDataStr);
                myStreamWriter.Close();
                using (HttpWebResponse response = (HttpWebResponse)request.GetResponse())
                {
                    using (Stream myResponseStream = response.GetResponseStream())
                    {
                        using (StreamReader myStreamReader = new StreamReader(myResponseStream, Encoding.GetEncoding("utf-8")))
                        {
                            string retString = myStreamReader.ReadToEnd();
                            string ret = response.ResponseUri.ToString();
                            myResponseStream.Close();
                            myStreamReader.Close();
                            return ret + "|" + retString;
                        }

                    }
                }

            }
            catch (Exception ex)
            {
                return ex.Message.ToString();
            }

        }


        //public static bool IsWeChat(HttpContextBase context)
        //{
        //    return context != null && context.Request.UserAgent.Contains("MicroMessenger");
        //    //HttpContext.Current.Request.Browser.IsMobileDevice && HttpContext.Current.Request.UserAgent.Contains("MicroMessenger");
        //}

        //public static bool isMobile(HttpContextBase context)
        //{
        //    //return true;
        //    //LogHelper.LogError(HttpContext.Current.Request.UserAgent.ToString() + HttpContext.Current.Request.Browser.IsMobileDevice+HttpContext.Current.Request.Browser.Platform +HttpContext.Current.Request.Browser.MobileDeviceModel+ "");
        //    return context != null && (context.Request.UserAgent.ToUpper().Contains("ANDROID") || context.Request.UserAgent.ToUpper().Contains("IPHONE"));
        //}

        public static bool DownloadFile(string URL, string filename)
        {
            try
            {
                System.Net.HttpWebRequest Myrq = (System.Net.HttpWebRequest)System.Net.HttpWebRequest.Create(URL);
                System.Net.HttpWebResponse myrp = (System.Net.HttpWebResponse)Myrq.GetResponse();
                System.IO.Stream st = myrp.GetResponseStream();
                System.IO.Stream so = new System.IO.FileStream(filename, System.IO.FileMode.Create);

                byte[] by = new byte[1024];
                int osize = st.Read(by, 0, (int)by.Length);
                while (osize > 0)
                {
                    so.Write(by, 0, osize);
                    osize = st.Read(by, 0, (int)by.Length);
                }
                so.Close();
                st.Close();
                myrp.Close();
                Myrq.Abort();
                return true;
            }
            catch (Exception ex)
            {
                return false;
            }
        }

        public static Stream DownloadFileGetStream(string URL, string filename)
        {

            System.Net.HttpWebRequest Myrq = (System.Net.HttpWebRequest)System.Net.HttpWebRequest.Create(URL);
            System.Net.HttpWebResponse myrp = (System.Net.HttpWebResponse)Myrq.GetResponse();
            System.IO.Stream st = myrp.GetResponseStream();
            //System.IO.Stream so = new System.IO.FileStream(filename, System.IO.FileMode.Create);
            //OSSService
            BufferedStream bufferstream = new BufferedStream(st);
            return bufferstream;
        }
    }

    public class HttpCookieCollection
    {
    }
}
