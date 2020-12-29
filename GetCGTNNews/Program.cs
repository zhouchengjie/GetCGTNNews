using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net;
using System.Net.Security;
using System.IO;
using System.Security.Cryptography.X509Certificates;
using System.Text.RegularExpressions;
using DataLibrary;
using Newtonsoft.Json;
using System.Globalization;
using System.Data;
using HtmlAgilityPack;
using System.IO.Compression;
using System.Threading;

namespace GetCGTNNews
{
    class Program
    {
        static void Main(string[] args)
        {
            //首页头条数据，每2小时跑一次
            do
            {
                try
                {

                    //获取首页截图
                    Console.WriteLine("ScreenshotHelper.Begin");
                    LogHelper.Debug("", "ScreenshotHelper.Begin");
                    ScreenshotHelper.Begin();

                    //CGTN头条
                    Console.WriteLine("CGTNHelper.Begin");
                    LogHelper.Debug("", "CGTNHelper.Begin");
                    CGTNHelper.Begin();
                    Console.WriteLine("CGTNHelper.End");

                    ////BBC头条
                    Console.WriteLine("BBCHelper.Begin");
                    LogHelper.Debug("", "BBCHelper.Begin");
                    BBCHelper.Begin();
                    Console.WriteLine("BBCHelper.End");

                    //CNN头条
                    Console.WriteLine("CNNNewsHelper.Begin");
                    LogHelper.Debug("", "CNNNewsHelper.Begin");
                    CNNNewsHelper.Begin();
                    Console.WriteLine("CNNNewsHelper.End");

                    Console.WriteLine("AFPHelper.Begin");
                    LogHelper.Debug("", "AFPHelper.Begin");
                    AFPHelper.Begin();
                    Console.WriteLine("AFPHelper.End");

                    Console.WriteLine("APNewsHelper.Begin");
                    LogHelper.Debug("", "APNewsHelper.Begin");
                    APNewsHelper.Begin();
                    Console.WriteLine("APNewsHelper.End");

                    Console.WriteLine("RTHelper.Begin");
                    LogHelper.Debug("", "RTHelper.Begin");
                    RTHelper.Begin();
                    Console.WriteLine("RTHelper.End");

                    //CGTN频道
                    //Console.WriteLine("CGTNGetChannelNews.Begin");
                    //LogHelper.Debug("", "CGTNGetChannelNews.Begin");
                    //CGTNGetChannelNews.Begin();

                    //每2小时跑一次
                    Thread.Sleep(1000 * 60 * 60 * 2);
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.ToString());
                    LogHelper.Debug("", ex.ToString());
                    Thread.Sleep(1000 * 60 * 60 * 2);
                }
            }
            while (true);

            //账号粉丝数，每24小时跑一次
            //do
            //{
            //    try
            //    {
            //        //获取账号信息
            //        Console.WriteLine("AccountDataHelper.Begin");
            //        LogHelper.Debug("", "AccountDataHelper.Begin");
            //        AccountDataHelper.Begin();

            //        //每24小时跑一次
            //        Thread.Sleep(1000 * 60 * 60 * 24);
            //    }
            //    catch (Exception ex)
            //    {
            //        Console.WriteLine(ex.ToString());
            //        LogHelper.Debug("", ex.ToString());
            //        Thread.Sleep(1000 * 60 * 60 * 24);
            //        continue;
            //    }
            //} while (true);
        }
    }
}
