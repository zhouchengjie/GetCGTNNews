using System.IO;
using System.Drawing;
using System.Drawing.Imaging;
using System.Threading;
using System.Windows.Forms;
using System;
using OpenQA.Selenium.Chrome;
using OpenQA.Selenium;
using OpenQA.Selenium.Remote;
using System.Collections.Generic;

namespace GetCGTNNews
{
    public static class ScreenshotHelper
    {
        public static void Begin()
        {
            Dictionary<string, string> channelDict = new Dictionary<string, string>
            {
                ["CGTN"] = "https://www.cgtn.com",
                ["BBC"] = "https://www.bbc.com/news",
                ["CNN"] = "https://edition.cnn.com",
                ["AFP"] = "https://www.afp.com/en/news-hub",
                ["AP"] = "https://apnews.com/",
                ["RT"] = "https://www.rt.com/",
            };
            foreach (KeyValuePair<string, string> pair in channelDict)
            {
                string screenshotPath = MakeScreenshot(pair.Value, pair.Key);
                string paras = "type=5&platform=" + pair.Key;
                string resultUrl = System.Configuration.ConfigurationManager.AppSettings["SendResultURL"];
                try
                {
                    HttpHelper.UploadImage(screenshotPath, paras, resultUrl);
                    File.Delete(screenshotPath);
                }
                catch { }
            }
        }

        public static string MakeScreenshot(string url, string platformName)
        {
            string screenshotName = platformName + DateTime.Now.ToString("yyyyMMddhhmmss") + ".png";
            string filePath = AppDomain.CurrentDomain.BaseDirectory + screenshotName;
            string chromeDriverFolderPath = AppDomain.CurrentDomain.BaseDirectory.ToString();
            ChromeDriverService service = ChromeDriverService.CreateDefaultService(chromeDriverFolderPath);
            service.Start();

            ChromeOptions op = new ChromeOptions();
            op.AddArguments("--headless");
            ChromeDriverEx driver = new ChromeDriverEx(chromeDriverFolderPath, op, TimeSpan.FromSeconds(120));
            driver.Navigate().GoToUrl(url);
            System.Threading.Thread.Sleep(1000);
            Screenshot screenShotFile = driver.GetFullPageScreenshot();
            screenShotFile.SaveAsFile(filePath, ScreenshotImageFormat.Png);
            driver.Quit();
            service.Dispose();

            return filePath;
        }
    }

    public class ChromeDriverEx : ChromeDriver
    {
        private const string SendChromeCommandWithResult = "sendChromeCommandWithResponse";
        private const string SendChromeCommandWithResultUrlTemplate = "/session/{sessionId}/chromium/send_command_and_get_result";

        public ChromeDriverEx(string chromeDriverDirectory, ChromeOptions options, TimeSpan timeout)
            : base(chromeDriverDirectory, options, timeout)
        {
            CommandInfo commandInfoToAdd = new CommandInfo(CommandInfo.PostCommand, SendChromeCommandWithResultUrlTemplate);
            this.CommandExecutor.CommandInfoRepository.TryAddCommand(SendChromeCommandWithResult, commandInfoToAdd);
        }

        public ChromeDriverEx(ChromeDriverService service, ChromeOptions options)
            : base(service, options)
        {
            CommandInfo commandInfoToAdd = new CommandInfo(CommandInfo.PostCommand, SendChromeCommandWithResultUrlTemplate);
            this.CommandExecutor.CommandInfoRepository.TryAddCommand(SendChromeCommandWithResult, commandInfoToAdd);
        }

        public Screenshot GetFullPageScreenshot()
        {

            string metricsScript = @"({
                width: Math.max(window.innerWidth,document.body.scrollWidth,document.documentElement.scrollWidth)|0,
                height: Math.max(window.innerHeight,document.body.scrollHeight,document.documentElement.scrollHeight)|0,
                deviceScaleFactor: window.devicePixelRatio || 1,
                mobile: typeof window.orientation !== 'undefined'
                })";
            Dictionary<string, object> metrics = this.EvaluateDevToolsScript(metricsScript);
            this.ExecuteChromeCommand("Emulation.setDeviceMetricsOverride", metrics);

            Dictionary<string, object> parameters = new Dictionary<string, object>();
            parameters["format"] = "png";
            parameters["fromSurface"] = true;
            object screenshotObject = this.ExecuteChromeCommandWithResult("Page.captureScreenshot", parameters);
            Dictionary<string, object> screenshotResult = screenshotObject as Dictionary<string, object>;
            string screenshotData = screenshotResult["data"] as string;

            this.ExecuteChromeCommand("Emulation.clearDeviceMetricsOverride", new Dictionary<string, object>());

            Screenshot screenshot = new Screenshot(screenshotData);
            return screenshot;
        }

        public object ExecuteChromeCommandWithResult(string commandName, Dictionary<string, object> commandParameters)
        {
            if (commandName == null)
            {
                throw new ArgumentNullException("commandName", "commandName must not be null");
            }

            Dictionary<string, object> parameters = new Dictionary<string, object>();
            parameters["cmd"] = commandName;
            parameters["params"] = commandParameters;
            Response response = this.Execute(SendChromeCommandWithResult, parameters);
            return response.Value;
        }

        private Dictionary<string, object> EvaluateDevToolsScript(string scriptToEvaluate)
        {

            Dictionary<string, object> parameters = new Dictionary<string, object>();
            parameters["returnByValue"] = true;
            parameters["expression"] = scriptToEvaluate;
            object evaluateResultObject = this.ExecuteChromeCommandWithResult("Runtime.evaluate", parameters);
            Dictionary<string, object> evaluateResultDictionary = evaluateResultObject as Dictionary<string, object>;
            Dictionary<string, object> evaluateResult = evaluateResultDictionary["result"] as Dictionary<string, object>;


            Dictionary<string, object> evaluateValue = evaluateResult["value"] as Dictionary<string, object>;
            return evaluateValue;
        }
    }
}