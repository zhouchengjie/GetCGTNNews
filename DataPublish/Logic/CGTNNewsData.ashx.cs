using DataLibrary;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.SessionState;

namespace DataPublish.Logic
{
	/// <summary>
	/// CGTNNewsData 的摘要说明
	/// </summary>
	public class CGTNNewsData : IHttpHandler, IRequiresSessionState
	{

		public void ProcessRequest(HttpContext context)
		{
			if (string.IsNullOrEmpty(context.Request["type"]))
			{
				return;
			}
			try
			{
				int requestType = Convert.ToInt32(context.Request["type"]);
				switch (requestType)
				{
					case 1:
						//获取国外社交平台账号
						GetSocialPlatformAccount();
						break;
					case 2:
						//下载国外社交平台账号
						DownloadSocialPlatformAccount();
						break;
					case 3:
						//保存账号信息
						SaveAccountInfo();
						break;
					case 4:
						//删除账号信息
						DeleteAccountInfo();
						break;
					case 20:
						//获取今日数据更新状态
						GetTodayUpdateStatus();
						break;
					case 21:
						//获取后台微博系统的ID
						GetWeiboScheduleID();
						break;
					case 22:
						//获取国外新闻头条数据
						GetCGTNNewsData();
						break;
					case 23:
						//下载国外新闻头条数据
						DownloadCGTNNewsData();
						break;
					case 24:
						//获取国外社交平台粉丝数据
						GetSocialPlatformData();
						break;
					case 25:
						//下载国外社交平台粉丝数据
						DownloadSocialPlatformData();
						break;
                    case 26:
                        //下载国外社交平台粉丝数据
                        GetScreenshotData();
                        break;
                    case 27:
                        GetAllAccountData();
                        break;
                    default:
						break;
				}
			}
			catch (Exception ex){ }
		}

		public bool IsReusable
		{
			get
			{
				return false;
			}
		}

        public void GetScreenshotData()
        {
            string page = HttpContext.Current.Request.QueryString["page"];
            string limit = HttpContext.Current.Request.QueryString["limit"];
            string typeID = HttpContext.Current.Request.QueryString["typeID"];
            string period = HttpContext.Current.Request.QueryString["period"];

            int _typeID = 0;
            int.TryParse(typeID, out _typeID);

            string typeName = "";
            if (_typeID == 1)
            {
                typeName = "CGTN";
            }
            else if (_typeID == 2)
            {
                typeName = "BBC";
            }
            else if (_typeID == 3)
            {
                typeName = "CNN";
            }

            DateTime tempFromDate = Constants.Date_Min;
            DateTime tempToDate = Constants.Date_Max;
            if (string.IsNullOrEmpty(period) == false)
            {
                int splitIndex = period.IndexOf(" - ");
                string strFromDate = period.Substring(0, splitIndex);
                string strToDate = period.Substring(splitIndex + 3);
                DateTime.TryParse(strFromDate, out tempFromDate);
                DateTime.TryParse(strToDate, out tempToDate);
                if (tempToDate.Hour == 0 && tempToDate.Minute == 0 && tempToDate.Second == 0)
                {
                    tempToDate = tempToDate.AddDays(1);
                }
                else
                {
                    tempToDate = tempToDate.AddSeconds(1);
                }
            }
            else
            {
                tempFromDate = DateTime.Today;
                tempToDate = DateTime.Today.AddDays(1);
            }

            int totalCount = 0;

            string sqlCount = string.Format(@"
select WebPath into #temp
from TopScreenshot with(nolock) 
where [Platform]=isnull(@typeName, [Platform])
and CreateTime>=@fromDate
and CreateTime<@toDate

select Count(*) as DataCount from #temp

drop table #temp
");
            SqlParameter[] parasCount = new SqlParameter[3];
            if (_typeID <= 0)
            {
                parasCount[0] = new SqlParameter("@typeName", DBNull.Value);
            }
            else
            {
                parasCount[0] = new SqlParameter("@typeName", typeName);
            }
            parasCount[1] = new SqlParameter("@fromDate", tempFromDate);
            parasCount[2] = new SqlParameter("@toDate", tempToDate);
            DataTable dtCount = BusinessLogicBase.Default.Select(sqlCount, parasCount);
            if (dtCount != null && dtCount.Rows.Count > 0)
            {
                int.TryParse(dtCount.Rows[0][0].ToString(), out totalCount);
            }

            int pageIndex = 0;
            int pageLimit = 0;
            int.TryParse(page, out pageIndex);
            int.TryParse(limit, out pageLimit);
            int pageFrom = (pageIndex - 1) * pageLimit + 1;
            int pageTo = pageFrom + pageLimit - 1;

            string sql = string.Format(@"

/*
declare @typeName nvarchar(50)
declare @fromDate datetime
declare @toDate datetime
declare @pageFrom int
declare @pageTo int

set @typeName=null
set @fromDate='2019-1-20'
set @toDate='2019-1-24'
set @pageFrom=1
set @pageTo=10
*/

select 
[Platform]
,[FileName]
,[CreateTime]
,[WebPath]
into #temp
from TopScreenshot with(nolock) 
where [Platform]=isnull(@typeName,[Platform])
and CreateTime>=@fromDate
and CreateTime<@toDate
order by CreateTime,[Platform]

select * from (
	select row_number() over (order by CreateTime,[Platform]) as rownumber,
    *,
    CONVERT(varchar(100), CreateTime, 20) as CreateTimeText
    from #temp
) a
where rownumber>=@pageFrom and rownumber<=@pageTo 
order by rownumber

drop table #temp
");
            SqlParameter[] paras = new SqlParameter[5];
            if (_typeID <= 0)
            {
                paras[0] = new SqlParameter("@typeName", DBNull.Value);
            }
            else
            {
                paras[0] = new SqlParameter("@typeName", typeName);
            }
            paras[1] = new SqlParameter("@fromDate", tempFromDate);
            paras[2] = new SqlParameter("@toDate", tempToDate);
            paras[3] = new SqlParameter("@pageFrom", pageFrom);
            paras[4] = new SqlParameter("@pageTo", pageTo);
            DataTable dt = BusinessLogicBase.Default.Select(sql, paras);

            string jsonResult = JSONHelper.DataTableToJSON(dt);
            string result = "{\"code\": 0, \"msg\": \"\", \"count\": " + totalCount + ",\"data\": " + jsonResult + "}";
            HttpContext.Current.Response.ContentType = "application/json";
            HttpContext.Current.Response.Write(result);
        }

        public void GetWeiboScheduleID()
		{
			string selectedDate = HttpContext.Current.Request.QueryString["selectedDate"];
			string cbxIsNoon = HttpContext.Current.Request.QueryString["cbxIsNoon"];

			bool isNoon = false;
			bool.TryParse(cbxIsNoon, out isNoon);

			string relevantPath = string.Empty;
			string diskPath = string.Empty;
			string scheduleID = string.Empty;
			string strDate = selectedDate;
			if (string.IsNullOrEmpty(strDate))
			{
				strDate = DateTime.Today.AddDays(-1).ToString("yyyy-MM-dd");
			}
			if (isNoon)
			{
				strDate = strDate + "-12";
			}
			string planID = string.Empty;
			string planName = string.Empty;

			try
			{

				string listUrl = "https://crawlerservice.ictr.cn/AccountPlan/GetAccountPlanList";
				string listReferUrl = "https://crawlerservice.ictr.cn/AccountPlan/AccountPlanList";
				string listContent = CommonFunction.GetWeiboUrlContent(listUrl, listReferUrl);
				var listObj = Newtonsoft.Json.JsonConvert.DeserializeObject<dynamic>(listContent);
				foreach (var itemObj in listObj.rows)
				{
					string id = itemObj.PlanID;
					string name = itemObj.PlanName;
					if (name.EndsWith(strDate))
					{
						planID = id;
						planName = name;
						break;
					}
				}
				if (string.IsNullOrEmpty(planID) == false)
				{
					string planUrl = string.Format("https://crawlerservice.ictr.cn/AccountPlan/GetScheduleInfoList?PageIndex=1&SubStatus=0&PlanID={0}&PlanStatus=0", planID);
					string referUrl = string.Format("https://crawlerservice.ictr.cn/AccountPlan/AccountSubtask?PlanID={0}&PlanStatus=5", planID);
					string body = CommonFunction.GetWeiboUrlContent(planUrl, referUrl);

					var jsonObj = Newtonsoft.Json.JsonConvert.DeserializeObject<dynamic>(body);

					scheduleID = jsonObj.value.rows[0].CollectScheduleID;
					string fileUrl = "https://crawlerservice.ictr.cn/AccountPlan/GetExcelFile?scheduleId=" + scheduleID;
					relevantPath = Constants.RelevantTempPath + planName + ".xlsx";
					diskPath = HttpContext.Current.Server.MapPath(relevantPath);
					if (File.Exists(diskPath) == false)
					{
						CommonFunction.GetUrlFile(diskPath, fileUrl, referUrl);
					}
				}
			}
			catch { }
			string result = "0";
			if (System.IO.File.Exists(diskPath))
			{
				result = System.Web.VirtualPathUtility.ToAbsolute(relevantPath);
			}
			HttpContext.Current.Response.ContentType = "text/plain";
			HttpContext.Current.Response.Write(result);
		}

		public void GetTodayUpdateStatus()
		{
			DateTime curDate = DateTime.Today.AddDays(-1);
			string sql = @"
        
declare @temp table(
TypeID int,
APP nvarchar(50),
ReleaseDate datetime,
StatusName nvarchar(50)
)

insert into @temp (TypeID, APP, ReleaseDate, StatusName)
select 1, '央视新闻',@CurDate,'未开始'
union
select 2, '人民日报',@CurDate,'未开始'
union
select 3, '新华社',@CurDate,'未开始'
union
select 5, '央广新闻',@CurDate,'未开始'
union
select 6, '环球资讯+',@CurDate,'未开始'
union
select 7, 'ChinaNews',@CurDate,'未开始'

select tt.APP,
tt.ReleaseDate,
nn.CreateTime,
case when nn.Status=1 then '正在更新' 
when nn.Status=0 then '已完成' 
else tt.StatusName end as 'StatusName'
from @temp tt
left join [dbo].[NewsDataTask] nn on nn.TypeID=tt.TypeID and nn.ReleaseDate=@CurDate and nn.BUpdate=1";
			SqlParameter[] paras = new SqlParameter[1];
			paras[0] = new SqlParameter("@CurDate", curDate);
			DataTable dt = BusinessLogicBase.Default.Select(sql, paras);
			string jsonResult = JSONHelper.DataTableToJSON(dt);
			HttpContext.Current.Response.ContentType = "application/json";
			HttpContext.Current.Response.Write(jsonResult);
		}

		public void DownloadSocialPlatformData()
		{
			string typeID = HttpContext.Current.Request.QueryString["typeID"];
			string period = HttpContext.Current.Request.QueryString["period"];
			string keyword = HttpContext.Current.Request.QueryString["keyword"];

			DateTime tempFromDate = Constants.Date_Min;
			DateTime tempToDate = Constants.Date_Max;
			if (string.IsNullOrEmpty(period) == false)
			{
				int splitIndex = period.IndexOf(" - ");
				string strFromDate = period.Substring(0, splitIndex);
				string strToDate = period.Substring(splitIndex + 3);
				DateTime.TryParse(strFromDate, out tempFromDate);
				DateTime.TryParse(strToDate, out tempToDate);
				if (tempToDate.Hour == 0 && tempToDate.Minute == 0 && tempToDate.Second == 0)
				{
					tempToDate = tempToDate.AddDays(1);
				}
				else
				{
					tempToDate = tempToDate.AddSeconds(1);
				}
			}
			else
			{
				tempFromDate = DateTime.Today;
				tempToDate = DateTime.Today.AddDays(1);
			}


			string sql = string.Format(@"
SELECT [ID]
      ,[Platform]
      ,[Account]
      ,[Url]
      ,[FollowerCount]
      ,[PostCount]
      ,[ViewCount]
      ,[LikeCount]
      ,[CreateTime]
      ,CONVERT(varchar(100), CreateTime, 20) as CreateTimeText
FROM [dbo].[AccountInfoData] with(nolock) 
where [Platform]=isnull(@typeID,[Platform])
and CreateTime>=@fromDate
and CreateTime<@toDate
and (Account like '%'+@keyword+'%')
order by CreateTime,[Platform]");
			SqlParameter[] paras = new SqlParameter[4];
			if (string.IsNullOrEmpty(typeID))
			{
				paras[0] = new SqlParameter("@typeID", DBNull.Value);
			}
			else
			{
				paras[0] = new SqlParameter("@typeID", typeID);
			}
			paras[1] = new SqlParameter("@fromDate", tempFromDate);
			paras[2] = new SqlParameter("@toDate", tempToDate);
			paras[3] = new SqlParameter("@keyword", keyword);
			DataTable dt = BusinessLogicBase.Default.Select(sql, paras);

			DataTable dtOutput = new DataTable();
			dtOutput.TableName = "SocialPlatformFans";
			dtOutput.Columns.Add("Platform", typeof(string));
			dtOutput.Columns.Add("Account", typeof(string));
			dtOutput.Columns.Add("FollowerCount", typeof(string));
			dtOutput.Columns.Add("PostCount", typeof(string));
			dtOutput.Columns.Add("ViewCount", typeof(string));
			dtOutput.Columns.Add("LikeCount", typeof(string));
			dtOutput.Columns.Add("Url", typeof(string));
			dtOutput.Columns.Add("CreateTime", typeof(string));
			if (dt != null && dt.Rows.Count > 0)
			{
				foreach (DataRow row in dt.Rows)
				{
					DataRow newRow = dtOutput.NewRow();
					newRow["Platform"] = row["Platform"].ToString();
					newRow["Account"] = row["Account"].ToString();
					newRow["FollowerCount"] = row["FollowerCount"].ToString();
					newRow["PostCount"] = row["PostCount"].ToString();
					newRow["ViewCount"] = row["ViewCount"].ToString();
					newRow["LikeCount"] = row["LikeCount"].ToString();
					newRow["Url"] = row["Url"].ToString();
					newRow["CreateTime"] = row["CreateTimeText"].ToString();
					dtOutput.Rows.Add(newRow);
				}
			}

			string fileName = string.Format("SocialPlatformFans_{0:yyyyMMddHHmmss}.xlsx", DateTime.Now);
			string folderRelevantPath = Constants.RelevantTempPath;
			string folderPath = HttpContext.Current.Server.MapPath(folderRelevantPath);
			if (System.IO.Directory.Exists(folderPath) == false)
			{
				System.IO.Directory.CreateDirectory(folderPath);
			}
			string fileRelevantPath = folderRelevantPath + fileName;
			string filePath = folderPath.TrimEnd('\\') + "\\" + fileName;
			try
			{
				NPOIHelper.ExportDTtoExcel(dtOutput, "", filePath);
			}
			catch { }
			string result = "0";
			if (System.IO.File.Exists(filePath))
			{
				result = System.Web.VirtualPathUtility.ToAbsolute(fileRelevantPath);
			}
			HttpContext.Current.Response.ContentType = "text/plain";
			HttpContext.Current.Response.Write(result);
		}

		public void GetSocialPlatformData()
		{
			string page = HttpContext.Current.Request.QueryString["page"];
			string limit = HttpContext.Current.Request.QueryString["limit"];
			string typeID = HttpContext.Current.Request.QueryString["typeID"];
			string period = HttpContext.Current.Request.QueryString["period"];
			string keyword = HttpContext.Current.Request.QueryString["keyword"];

			DateTime tempFromDate = Constants.Date_Min;
			DateTime tempToDate = Constants.Date_Max;
			if (string.IsNullOrEmpty(period) == false)
			{
				int splitIndex = period.IndexOf(" - ");
				string strFromDate = period.Substring(0, splitIndex);
				string strToDate = period.Substring(splitIndex + 3);
				DateTime.TryParse(strFromDate, out tempFromDate);
				DateTime.TryParse(strToDate, out tempToDate);
				if (tempToDate.Hour == 0 && tempToDate.Minute == 0 && tempToDate.Second == 0)
				{
					tempToDate = tempToDate.AddDays(1);
				}
				else
				{
					tempToDate = tempToDate.AddSeconds(1);
				}
			}
			else
			{
				tempFromDate = DateTime.Today;
				tempToDate = DateTime.Today.AddDays(1);
			}

			int totalCount = 0;

			string sqlCount = string.Format(@"
select count(*) from AccountInfoData with(nolock) 
where [Platform]=isnull(@typeID, [Platform])
and CreateTime>=@fromDate
and CreateTime<@toDate
and (Account like '%'+@keyword+'%')");
			SqlParameter[] parasCount = new SqlParameter[4];
			if (string.IsNullOrEmpty(typeID))
			{
				parasCount[0] = new SqlParameter("@typeID", DBNull.Value);
			}
			else
			{
				parasCount[0] = new SqlParameter("@typeID", typeID);
			}
			parasCount[1] = new SqlParameter("@fromDate", tempFromDate);
			parasCount[2] = new SqlParameter("@toDate", tempToDate);
			parasCount[3] = new SqlParameter("@keyword", keyword);
			DataTable dtCount = BusinessLogicBase.Default.Select(sqlCount, parasCount);
			if (dtCount != null && dtCount.Rows.Count > 0)
			{
				int.TryParse(dtCount.Rows[0][0].ToString(), out totalCount);
			}

			int pageIndex = 0;
			int pageLimit = 0;
			int.TryParse(page, out pageIndex);
			int.TryParse(limit, out pageLimit);
			int pageFrom = (pageIndex - 1) * pageLimit + 1;
			int pageTo = pageFrom + pageLimit - 1;

			string sql = string.Format(@"

/*
declare @typeName nvarchar(50)
declare @fromDate datetime
declare @toDate datetime
declare @pageFrom int
declare @pageTo int

set @typeName=null
set @fromDate='2019-1-20'
set @toDate='2019-1-24'
set @pageFrom=1
set @pageTo=10
*/
SELECT [ID]
      ,[Platform]
      ,[Account]
      ,[Url]
      ,[FollowerCount]
      ,[PostCount]
      ,[ViewCount]
      ,[LikeCount]
      ,[CreateTime]
into #temp
FROM [dbo].[AccountInfoData] with(nolock) 
where [Platform]=isnull(@typeID,[Platform])
and CreateTime>=@fromDate
and CreateTime<@toDate
and (Account like '%'+@keyword+'%')
order by CreateTime,[Platform]

select * from (
	select row_number() over (order by CreateTime,[Platform]) as rownumber,
    *,
    CONVERT(varchar(100), CreateTime, 20) as CreateTimeText
    from #temp
) a
where rownumber>=@pageFrom and rownumber<=@pageTo 
order by rownumber

drop table #temp
");
			SqlParameter[] paras = new SqlParameter[6];
			if (string.IsNullOrEmpty(typeID))
			{
				paras[0] = new SqlParameter("@typeID", DBNull.Value);
			}
			else
			{
				paras[0] = new SqlParameter("@typeID", typeID);
			}
			paras[1] = new SqlParameter("@fromDate", tempFromDate);
			paras[2] = new SqlParameter("@toDate", tempToDate);
			paras[3] = new SqlParameter("@pageFrom", pageFrom);
			paras[4] = new SqlParameter("@pageTo", pageTo);
			paras[5] = new SqlParameter("@keyword", keyword);
			DataTable dt = BusinessLogicBase.Default.Select(sql, paras);

			string jsonResult = JSONHelper.DataTableToJSON(dt);
			string result = "{\"code\": 0, \"msg\": \"\", \"count\": " + totalCount + ",\"data\": " + jsonResult + "}";
			HttpContext.Current.Response.ContentType = "application/json";
			HttpContext.Current.Response.Write(result);
		}

		public void DownloadCGTNNewsData()
		{
			string typeID = HttpContext.Current.Request.QueryString["typeID"];
			string period = HttpContext.Current.Request.QueryString["period"];
			string keyword = HttpContext.Current.Request.QueryString["keyword"];

			int _typeID = 0;
			int.TryParse(typeID, out _typeID);

			string typeName = "CGTN";
			if (_typeID == 1)
			{
				typeName = "CGTN";
			}
			else if (_typeID == 2)
			{
				typeName = "BBC News";
			}
			else if (_typeID == 3)
			{
				typeName = "CNN";
			}

			DateTime tempFromDate = Constants.Date_Min;
			DateTime tempToDate = Constants.Date_Max;
			if (string.IsNullOrEmpty(period) == false)
			{
				int splitIndex = period.IndexOf(" - ");
				string strFromDate = period.Substring(0, splitIndex);
				string strToDate = period.Substring(splitIndex + 3);
				DateTime.TryParse(strFromDate, out tempFromDate);
				DateTime.TryParse(strToDate, out tempToDate);
				if (tempToDate.Hour == 0 && tempToDate.Minute == 0 && tempToDate.Second == 0)
				{
					tempToDate = tempToDate.AddDays(1);
				}
				else
				{
					tempToDate = tempToDate.AddSeconds(1);
				}
			}
			else
			{
				tempFromDate = DateTime.Today;
				tempToDate = DateTime.Today.AddDays(1);
			}

			string sql = string.Format(@"

/*
declare @typeName nvarchar(50)
declare @fromDate datetime
declare @toDate datetime

set @typeName=null
set @fromDate='2019-1-20'
set @toDate='2019-1-24'
*/

select 
[Platform]
,Title
,Source
,[Author]
,[CategoryName]
,[CoverImageUrl]
,[Summary]
,[TextContent]
,[ImageUrls]
,[ReleaseTimeStr]
,[ReleaseTime]
,[UpdateTimeStr]
,[PositionType]
,[ScreenType]
,[Url]
,[CreateTime]
,CONVERT(varchar(100), ReleaseTime, 20) as ReleaseTimeText
,CONVERT(varchar(100), CreateTime, 20) as CreateTimeText
from CGTNNews with(nolock) 
where [Platform]=isnull(@typeName,[Platform])
and CreateTime>=@fromDate
and CreateTime<@toDate
and (Title like '%'+@keyword+'%' or TextContent like '%'+@keyword+'%')
order by CreateTime,[Platform]");
			SqlParameter[] paras = new SqlParameter[4];
			if (_typeID <= 0)
			{
				paras[0] = new SqlParameter("@typeName", DBNull.Value);
			}
			else
			{
				paras[0] = new SqlParameter("@typeName", typeName);
			}
			paras[1] = new SqlParameter("@fromDate", tempFromDate);
			paras[2] = new SqlParameter("@toDate", tempToDate);
			paras[3] = new SqlParameter("@keyword", keyword);

			DataTable dt = BusinessLogicBase.Default.Select(sql, paras);

			DataTable dtOutput = new DataTable();
			dtOutput.TableName = "TopNews";
			dtOutput.Columns.Add("Platform", typeof(string));
			dtOutput.Columns.Add("Title", typeof(string));
			dtOutput.Columns.Add("Source", typeof(string));
			dtOutput.Columns.Add("Author", typeof(string));
			dtOutput.Columns.Add("Channel", typeof(string));
			dtOutput.Columns.Add("TextContent", typeof(string));
			dtOutput.Columns.Add("ReleaseTime", typeof(string));
			dtOutput.Columns.Add("Position", typeof(string));
			dtOutput.Columns.Add("Url", typeof(string));
			dtOutput.Columns.Add("CreateTime", typeof(string));

			if (dt != null && dt.Rows.Count > 0)
			{
				foreach (DataRow row in dt.Rows)
				{
					DataRow newRow = dtOutput.NewRow();
					newRow["Platform"] = row["Platform"].ToString();
					newRow["Title"] = row["Title"].ToString();
					newRow["Source"] = row["Source"].ToString();
					newRow["Author"] = row["Author"].ToString();
					newRow["Channel"] = row["CategoryName"].ToString();
					newRow["TextContent"] = row["TextContent"].ToString();
					newRow["ReleaseTime"] = row["ReleaseTimeText"].ToString();
					newRow["Position"] = row["PositionType"].ToString();
					newRow["Url"] = row["Url"].ToString();
					newRow["CreateTime"] = row["CreateTimeText"].ToString();
					dtOutput.Rows.Add(newRow);
				}
			}
			string fileName = string.Format("TopNews_{0:yyyyMMddHHmmss}.xlsx", DateTime.Now);
			string folderRelevantPath = Constants.RelevantTempPath;
			string folderPath = HttpContext.Current.Server.MapPath(folderRelevantPath);
			if (System.IO.Directory.Exists(folderPath) == false)
			{
				System.IO.Directory.CreateDirectory(folderPath);
			}
			string fileRelevantPath = folderRelevantPath + fileName;
			string filePath = folderPath.TrimEnd('\\') + "\\" + fileName;
			try
			{
				NPOIHelper.ExportDTtoExcel(dtOutput, "", filePath);
			}
			catch { }
			string result = "0";
			if (System.IO.File.Exists(filePath))
			{
				result = System.Web.VirtualPathUtility.ToAbsolute(fileRelevantPath);
			}
			HttpContext.Current.Response.ContentType = "text/plain";
			HttpContext.Current.Response.Write(result);
		}

		public void GetCGTNNewsData()
		{
			string page = HttpContext.Current.Request.QueryString["page"];
			string limit = HttpContext.Current.Request.QueryString["limit"];
			string typeID = HttpContext.Current.Request.QueryString["typeID"];
			string period = HttpContext.Current.Request.QueryString["period"];
			string keyword = HttpContext.Current.Request.QueryString["keyword"];

			int _typeID = 0;
			int.TryParse(typeID, out _typeID);

			string typeName = "CGTN";
			if (_typeID == 1)
			{
				typeName = "CGTN";
			}
			else if (_typeID == 2)
			{
				typeName = "BBC News";
			}
			else if (_typeID == 3)
			{
				typeName = "CNN";
			}

			DateTime tempFromDate = Constants.Date_Min;
			DateTime tempToDate = Constants.Date_Max;
			if (string.IsNullOrEmpty(period) == false)
			{
				int splitIndex = period.IndexOf(" - ");
				string strFromDate = period.Substring(0, splitIndex);
				string strToDate = period.Substring(splitIndex + 3);
				DateTime.TryParse(strFromDate, out tempFromDate);
				DateTime.TryParse(strToDate, out tempToDate);
				if (tempToDate.Hour == 0 && tempToDate.Minute == 0 && tempToDate.Second == 0)
				{
					tempToDate = tempToDate.AddDays(1);
				}
				else
				{
					tempToDate = tempToDate.AddSeconds(1);
				}
			}
			else
			{
				tempFromDate = DateTime.Today;
				tempToDate = DateTime.Today.AddDays(1);
			}

			int totalCount = 0;

			string sqlCount = string.Format(@"
select [Platform], Title into #temp
from CGTNNews with(nolock) 
where [Platform]=isnull(@typeName, [Platform])
and CreateTime>=@fromDate
and CreateTime<@toDate
and (Title like '%'+@keyword+'%' or TextContent like '%'+@keyword+'%')

select Count(*) as DataCount from #temp

drop table #temp
");
			SqlParameter[] parasCount = new SqlParameter[4];
			if (_typeID <= 0)
			{
				parasCount[0] = new SqlParameter("@typeName", DBNull.Value);
			}
			else
			{
				parasCount[0] = new SqlParameter("@typeName", typeName);
			}
			parasCount[1] = new SqlParameter("@fromDate", tempFromDate);
			parasCount[2] = new SqlParameter("@toDate", tempToDate);
			parasCount[3] = new SqlParameter("@keyword", keyword);
			DataTable dtCount = BusinessLogicBase.Default.Select(sqlCount, parasCount);
			if (dtCount != null && dtCount.Rows.Count > 0)
			{
				int.TryParse(dtCount.Rows[0][0].ToString(), out totalCount);
			}

			int pageIndex = 0;
			int pageLimit = 0;
			int.TryParse(page, out pageIndex);
			int.TryParse(limit, out pageLimit);
			int pageFrom = (pageIndex - 1) * pageLimit + 1;
			int pageTo = pageFrom + pageLimit - 1;

			string sql = string.Format(@"

/*
declare @typeName nvarchar(50)
declare @fromDate datetime
declare @toDate datetime
declare @pageFrom int
declare @pageTo int

set @typeName=null
set @fromDate='2019-1-20'
set @toDate='2019-1-24'
set @pageFrom=1
set @pageTo=10
*/

select 
[Platform]
,Title
,Source
,[Author]
,[CategoryName]
,[CoverImageUrl]
,[Summary]
,[TextContent]
,[ImageUrls]
,[ReleaseTimeStr]
,[ReleaseTime]
,[UpdateTimeStr]
,[PositionType]
,[ScreenType]
,[Url]
,[CreateTime]
into #temp
from CGTNNews with(nolock) 
where [Platform]=isnull(@typeName,[Platform])
and CreateTime>=@fromDate
and CreateTime<@toDate
and (Title like '%'+@keyword+'%' or TextContent like '%'+@keyword+'%')
order by CreateTime,[Platform]

select * from (
	select row_number() over (order by CreateTime,[Platform]) as rownumber,
    *,
    CONVERT(varchar(100), ReleaseTime, 20) as ReleaseTimeText,
    CONVERT(varchar(100), CreateTime, 20) as CreateTimeText
    from #temp
) a
where rownumber>=@pageFrom and rownumber<=@pageTo 
order by rownumber

drop table #temp
");
			SqlParameter[] paras = new SqlParameter[6];
			if (_typeID <= 0)
			{
				paras[0] = new SqlParameter("@typeName", DBNull.Value);
			}
			else
			{
				paras[0] = new SqlParameter("@typeName", typeName);
			}
			paras[1] = new SqlParameter("@fromDate", tempFromDate);
			paras[2] = new SqlParameter("@toDate", tempToDate);
			paras[3] = new SqlParameter("@pageFrom", pageFrom);
			paras[4] = new SqlParameter("@pageTo", pageTo);
			paras[5] = new SqlParameter("@keyword", keyword);
			DataTable dt = BusinessLogicBase.Default.Select(sql, paras);

			string jsonResult = JSONHelper.DataTableToJSON(dt);
			string result = "{\"code\": 0, \"msg\": \"\", \"count\": " + totalCount + ",\"data\": " + jsonResult + "}";
			HttpContext.Current.Response.ContentType = "application/json";
			HttpContext.Current.Response.Write(result);
		}


		private void DeleteAccountInfo()
		{
			int id = HttpContext.Current.Request.Params["id"].ToInt(0);
			AccountInfoDO infoDO = new AccountInfoDO();
			infoDO.ID = id;
			BusinessLogicBase.Default.Delete(infoDO);
			string result = "1";
			HttpContext.Current.Response.ContentType = "application/json";
			HttpContext.Current.Response.Write(result);
		}

		private void SaveAccountInfo()
		{
			int id = HttpContext.Current.Request.Params["id"].ToInt(0);
			string platform = HttpContext.Current.Request.Params["platform"];
			string accountID = HttpContext.Current.Request.Params["accountID"];
			string accountName = HttpContext.Current.Request.Params["accountName"];
			string url = HttpContext.Current.Request.Params["accountUrl"];

			AccountInfoDO infoDO = new AccountInfoDO();
			infoDO.AccountID = accountID;
			infoDO.AccountName = accountName;
			infoDO.Platform = platform;
			infoDO.Url = url;
			if (id > 0)
			{
				infoDO.ID = id;
				BusinessLogicBase.Default.Update(infoDO);
			}
			else
			{
				BusinessLogicBase.Default.Insert(infoDO);
			}
			string result = "1";
			HttpContext.Current.Response.ContentType = "application/json";
			HttpContext.Current.Response.Write(result);
		}

		private void GetSocialPlatformAccount()
		{
			string page = HttpContext.Current.Request.QueryString["page"];
			string limit = HttpContext.Current.Request.QueryString["limit"];
			string typeID = HttpContext.Current.Request.QueryString["typeID"];
			string keyword = HttpContext.Current.Request.QueryString["keyword"];

			int totalCount = 0;

			string sqlCount = string.Format(@"
select count(*) from AccountInfo with(nolock) 
where [Platform]=isnull(@typeID, [Platform])
and (AccountName like '%'+@keyword+'%')");
			SqlParameter[] parasCount = new SqlParameter[2];
			if (string.IsNullOrEmpty(typeID))
			{
				parasCount[0] = new SqlParameter("@typeID", DBNull.Value);
			}
			else
			{
				parasCount[0] = new SqlParameter("@typeID", typeID);
			}
			parasCount[1] = new SqlParameter("@keyword", keyword);
			DataTable dtCount = BusinessLogicBase.Default.Select(sqlCount, parasCount);
			if (dtCount != null && dtCount.Rows.Count > 0)
			{
				int.TryParse(dtCount.Rows[0][0].ToString(), out totalCount);
			}

			int pageIndex = 0;
			int pageLimit = 0;
			int.TryParse(page, out pageIndex);
			int.TryParse(limit, out pageLimit);
			int pageFrom = (pageIndex - 1) * pageLimit + 1;
			int pageTo = pageFrom + pageLimit - 1;

			string sql = string.Format(@"

/*
declare @typeName nvarchar(50)
declare @pageFrom int
declare @pageTo int

set @typeName=null
set @pageFrom=1
set @pageTo=10
*/
SELECT [ID]
      ,[Platform]
      ,[AccountID]
      ,[AccountName]
      ,[Url]
into #temp
FROM [dbo].[AccountInfo] with(nolock) 
where [Platform]=isnull(@typeID,[Platform])
and (AccountName like '%'+@keyword+'%')
order by [Platform],AccountName

select * from (
	select row_number() over (order by [Platform],AccountName) as rownumber,*
    from #temp
) a
where rownumber>=@pageFrom and rownumber<=@pageTo 
order by rownumber

drop table #temp
");
			SqlParameter[] paras = new SqlParameter[4];
			if (string.IsNullOrEmpty(typeID))
			{
				paras[0] = new SqlParameter("@typeID", DBNull.Value);
			}
			else
			{
				paras[0] = new SqlParameter("@typeID", typeID);
			}
			paras[1] = new SqlParameter("@pageFrom", pageFrom);
			paras[2] = new SqlParameter("@pageTo", pageTo);
			paras[3] = new SqlParameter("@keyword", keyword);
			DataTable dt = BusinessLogicBase.Default.Select(sql, paras);

			string jsonResult = JSONHelper.DataTableToJSON(dt);
			string result = "{\"code\": 0, \"msg\": \"\", \"count\": " + totalCount + ",\"data\": " + jsonResult + "}";
			HttpContext.Current.Response.ContentType = "application/json";
			HttpContext.Current.Response.Write(result);
		}

		public void DownloadSocialPlatformAccount()
		{
			string typeID = HttpContext.Current.Request.QueryString["typeID"];
			string keyword = HttpContext.Current.Request.QueryString["keyword"];


			string sql = string.Format(@"
SELECT [ID]
      ,[Platform]
      ,[AccountID]
      ,[AccountName]
      ,[Url]
FROM [dbo].[AccountInfo] with(nolock) 
where [Platform]=isnull(@typeID,[Platform])
and (AccountName like '%'+@keyword+'%')
order by [Platform],AccountName");
			SqlParameter[] paras = new SqlParameter[2];
			if (string.IsNullOrEmpty(typeID))
			{
				paras[0] = new SqlParameter("@typeID", DBNull.Value);
			}
			else
			{
				paras[0] = new SqlParameter("@typeID", typeID);
			}
			paras[1] = new SqlParameter("@keyword", keyword);
			DataTable dt = BusinessLogicBase.Default.Select(sql, paras);

			DataTable dtOutput = new DataTable();
			dtOutput.TableName = "SocialPlatformFans";
			dtOutput.Columns.Add("Platform", typeof(string));
			dtOutput.Columns.Add("AccountID", typeof(string));
			dtOutput.Columns.Add("AccountName", typeof(string));
			dtOutput.Columns.Add("Url", typeof(string));
			if (dt != null && dt.Rows.Count > 0)
			{
				foreach (DataRow row in dt.Rows)
				{
					DataRow newRow = dtOutput.NewRow();
					newRow["Platform"] = row["Platform"].ToString();
					newRow["AccountID"] = row["AccountID"].ToString();
					newRow["AccountName"] = row["AccountName"].ToString();
					newRow["Url"] = row["Url"].ToString();
					dtOutput.Rows.Add(newRow);
				}
			}

			string fileName = string.Format("SocialPlatformAccounts_{0:yyyyMMddHHmmss}.xlsx", DateTime.Now);
			string folderRelevantPath = Constants.RelevantTempPath;
			string folderPath = HttpContext.Current.Server.MapPath(folderRelevantPath);
			if (System.IO.Directory.Exists(folderPath) == false)
			{
				System.IO.Directory.CreateDirectory(folderPath);
			}
			string fileRelevantPath = folderRelevantPath + fileName;
			string filePath = folderPath.TrimEnd('\\') + "\\" + fileName;
			try
			{
				NPOIHelper.ExportDTtoExcel(dtOutput, "", filePath);
			}
			catch { }
			string result = "0";
			if (System.IO.File.Exists(filePath))
			{
				result = System.Web.VirtualPathUtility.ToAbsolute(fileRelevantPath);
			}
			HttpContext.Current.Response.ContentType = "text/plain";
			HttpContext.Current.Response.Write(result);
		}

        private void GetAllAccountData()
        {
            DataTable dt = BusinessLogicBase.Default.Select("select * from AccountInfo order by Platform");
            string result = JSONHelper.DataTableToJSON(dt);
            HttpContext.Current.Response.ContentType = "text/plain";
            HttpContext.Current.Response.Write(result);
        }
    }
}