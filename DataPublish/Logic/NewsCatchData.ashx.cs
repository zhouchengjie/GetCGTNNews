using DataLibrary;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Web;

namespace DataPublish.Logic
{
	/// <summary>
	/// NewsCatchData 的摘要说明
	/// </summary>
	public class NewsCatchData : IHttpHandler
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
					case 18:
						//获取新闻客户端数据
						GetNewsData();
						break;
					case 19:
						//下载新闻客户端数据
						GetNewsDataDownload();
						break;
					default:
						break;
				}
			}
			catch
			{

			}
		}


		public void GetNewsDataDownload()
		{
			string typeID = HttpContext.Current.Request.QueryString["typeID"];
			string period = HttpContext.Current.Request.QueryString["period"];
			string keyword = HttpContext.Current.Request.QueryString["keyword"];

			int _typeID = 0;
			int.TryParse(typeID, out _typeID);
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
				tempFromDate = DateTime.Today.AddDays(-1);
				tempToDate = DateTime.Today;
			}

			string tableName = ConfigurationManager.AppSettings["TableName"].ToString();

			string sql = string.Format(@"
/*
declare @dateFrom datetime  
declare @dateTo datetime
declare @typeID int
set @dateFrom='2018-10-24'
set @dateTo='2018-10-24'
*/
select 
IDstr,
TypeID,
case when TypeID=1 then '央视新闻'
when TypeID=2 then '人民日报'
when TypeID=3 then '新华社'
when TypeID=5 then '央广新闻'
when TypeID=6 then '环球资讯+'
when TypeID=7 then 'ChinaNews'
when TypeID=4 then '凤凰新闻' end as 'APP', 
Title, 
Source,
ViewCount,
CommentCount,
LikeCount,
ReleaseDate,
CreateTime,
Url, 
Content,
HasWords,
HasImage,
HasVideo,
VideoLength
into #temp
from {0}
where TypeID=isnull(@typeID, TypeID)
and ReleaseDate>=DATEADD(day,-1,@dateFrom)
and ReleaseDate<DATEADD(day,1,@dateTo)
and (Title like '%'+@keyword+'%' or Content like '%'+@keyword+'%')
order by TypeID,ReleaseDate desc

select * into #result from #temp order by APP,ReleaseDate desc,ViewCount desc

select TypeID,APP,
Title,
MAX(ViewCount) as 'ViewCount',
MAX(CommentCount) as 'CommentCount',
MAX(LikeCount) as 'LikeCount',
MIN(ReleaseDate) as 'ReleaseDate',
MAX(CreateTime) as 'CreateTime'
into #result2
from #result
group by TypeID,APP,Title

select r2.*,
r1.Source,r1.Content,r1.Url,
r1.HasWords,
r1.HasImage,
r1.HasVideo,
r1.VideoLength,
CONVERT(varchar(100), r2.ReleaseDate, 20) as ReleaseDateText,
CONVERT(varchar(100), r2.CreateTime, 20) as CreateTimeText,
case when CHARINDEX('习近平',r1.content)>0 or CHARINDEX('总书记',r1.content)>0
or CHARINDEX('习近平',r2.Title)>0 or CHARINDEX('总书记',r2.Title)>0 then 1
else 0 end as '是否包含习近平',

case when 

CHARINDEX('肺炎',r2.Title)>0 or CHARINDEX('肺炎',r1.content)>0
or CHARINDEX('新冠',r2.Title)>0 or CHARINDEX('新冠',r1.content)>0 
or CHARINDEX('武汉',r2.Title)>0 or CHARINDEX('武汉',r1.content)>0 
or CHARINDEX('疫情',r2.Title)>0 or CHARINDEX('疫情',r1.content)>0 
or CHARINDEX('红十字会',r2.Title)>0 or CHARINDEX('红十字会',r1.content)>0 
or CHARINDEX('火神山',r2.Title)>0 or CHARINDEX('火神山',r1.content)>0 
or CHARINDEX('雷神山',r2.Title)>0 or CHARINDEX('雷神山',r1.content)>0 
or CHARINDEX('口罩',r2.Title)>0 or CHARINDEX('口罩',r1.content)>0 
or CHARINDEX('物资',r2.Title)>0 or CHARINDEX('物资',r1.content)>0 
or CHARINDEX('确诊',r2.Title)>0 or CHARINDEX('确诊',r1.content)>0 
or CHARINDEX('抗疫',r2.Title)>0 or CHARINDEX('抗疫',r1.content)>0 
or CHARINDEX('双黄连',r2.Title)>0 or CHARINDEX('双黄连',r1.content)>0 
or CHARINDEX('医疗',r2.Title)>0 or CHARINDEX('医疗',r1.content)>0 
or CHARINDEX('湖北',r2.Title)>0 or CHARINDEX('湖北',r1.content)>0 
or CHARINDEX('复工',r2.Title)>0 or CHARINDEX('复工',r1.content)>0 
or CHARINDEX('海鲜市场',r2.Title)>0 or CHARINDEX('海鲜市场',r1.content)>0 
or CHARINDEX('医疗',r2.Title)>0 or CHARINDEX('医疗',r1.content)>0 
or CHARINDEX('隔离',r2.Title)>0 or CHARINDEX('隔离',r1.content)>0 
or CHARINDEX('战疫',r2.Title)>0 or CHARINDEX('战疫',r1.content)>0 
then 1
else 0 end as '是否包含疫情'
from #result2 r2
inner join (select ROW_NUMBER() over (partition by APP,Title order by ViewCount desc) as NID ,* from #result) r1 
on r1.APP=r2.APP
and r1.Title=r2.Title
where r1.NID=1
and r2.ReleaseDate>=@dateFrom
and r2.ReleaseDate<@dateTo
order by ReleaseDate desc,TypeID

drop table #result2
drop table #result
drop table #temp", tableName);

			SqlParameter[] paras = new SqlParameter[4];
			if (_typeID <= 0)
			{
				paras[0] = new SqlParameter("@typeID", DBNull.Value);
			}
			else
			{
				paras[0] = new SqlParameter("@typeID", _typeID);
			}
			paras[1] = new SqlParameter("@dateFrom", tempFromDate);
			paras[2] = new SqlParameter("@dateTo", tempToDate);
			paras[3] = new SqlParameter("@keyword", keyword);
			DataTable dt = BusinessLogicBase.Default.Select(sql, paras);

			DataTable dtOutput = new DataTable();
			dtOutput.TableName = "客户端数据";
			dtOutput.Columns.Add("APP平台", typeof(string));
			dtOutput.Columns.Add("标题", typeof(string));
			dtOutput.Columns.Add("来源", typeof(string));
			dtOutput.Columns.Add("发布时间", typeof(string));
			dtOutput.Columns.Add("采集时间", typeof(string));
			dtOutput.Columns.Add("阅读量", typeof(string));
			dtOutput.Columns.Add("评论量", typeof(string));
			dtOutput.Columns.Add("点赞量", typeof(string));
			dtOutput.Columns.Add("链接", typeof(string));
			dtOutput.Columns.Add("正文", typeof(string));
			dtOutput.Columns.Add("正文长度", typeof(string));
			dtOutput.Columns.Add("是否包含习近平", typeof(string));
			dtOutput.Columns.Add("是否包含疫情", typeof(string));
			dtOutput.Columns.Add("是否包含文字", typeof(string));
			dtOutput.Columns.Add("是否包含图片", typeof(string));
			dtOutput.Columns.Add("是否包含视频", typeof(string));
			dtOutput.Columns.Add("视频长度(秒)", typeof(string));
			if (dt != null && dt.Rows.Count > 0)
			{
				foreach (DataRow row in dt.Rows)
				{
					DataRow newRow = dtOutput.NewRow();
					newRow["APP平台"] = row["APP"].ToString();
					newRow["标题"] = row["Title"].ToString();
					newRow["来源"] = row["Source"].ToString();
					newRow["发布时间"] = row["ReleaseDateText"].ToString();
					newRow["采集时间"] = row["CreateTimeText"].ToString();
					newRow["阅读量"] = row["ViewCount"].ToString();
					newRow["评论量"] = row["CommentCount"].ToString();
					newRow["点赞量"] = row["LikeCount"].ToString();
					newRow["链接"] = row["Url"].ToString();
					string content = row["Content"].ToString();
					int contentLength = content.Length;
					//Excel单元格限制文本长度必须在32K以内
					if (contentLength > 32700)
					{
						content = content.Substring(0, 32700);
					}
					newRow["正文"] = content;
					newRow["正文长度"] = contentLength;
					newRow["是否包含习近平"] = row["是否包含习近平"].ToString();
					newRow["是否包含疫情"] = row["是否包含疫情"].ToString();
					newRow["是否包含文字"] = row["HasWords"].ToString();
					newRow["是否包含图片"] = row["HasImage"].ToString();
					newRow["是否包含视频"] = row["HasVideo"].ToString();
					newRow["视频长度(秒)"] = row["VideoLength"].ToString();
					dtOutput.Rows.Add(newRow);
				}
			}
			//DataSet ds = new DataSet();
			//ds.Tables.Add(dtOutput);

			string fileName = string.Format("客户端数据_{0:yyyyMMddHHmmss}.xlsx", DateTime.Now);
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
				//ParseExcel.CreateExcel(filePath, ds);
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

		public void GetNewsData()
		{
			string page = HttpContext.Current.Request.QueryString["page"];
			string limit = HttpContext.Current.Request.QueryString["limit"];
			string typeID = HttpContext.Current.Request.QueryString["typeID"];
			string period = HttpContext.Current.Request.QueryString["period"];
			string keyword = HttpContext.Current.Request.QueryString["keyword"];

			string tableName = ConfigurationManager.AppSettings["TableName"].ToString();

			int _typeID = 0;
			int.TryParse(typeID, out _typeID);

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
				tempFromDate = DateTime.Today.AddDays(-1);
				tempToDate = DateTime.Today;
			}

			int totalCount = 0;

			string sqlCount = string.Format(@"
select TypeID, Title into #temp
from {0} with(nolock) 
where TypeID=isnull(@typeID, TypeID)
and ReleaseDate>=@fromDate
and ReleaseDate<@toDate
and (Title like '%'+@keyword+'%' or Content like '%'+@keyword+'%')
group by TypeID,Title

select Count(*) as DataCount from #temp

drop table #temp
", tableName);
			SqlParameter[] parasCount = new SqlParameter[4];
			if (_typeID <= 0)
			{
				parasCount[0] = new SqlParameter("@typeID", DBNull.Value);
			}
			else
			{
				parasCount[0] = new SqlParameter("@typeID", _typeID);
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

select 
TypeID,
IDstr,
case when TypeID=1 then '央视新闻'
when TypeID=2 then '人民日报'
when TypeID=3 then '新华社'
when TypeID=5 then '央广新闻'
when TypeID=6 then '环球资讯+'
when TypeID=7 then 'ChinaNews'
when TypeID=4 then '凤凰新闻' end as 'APP', 
Title, 
Source,
ViewCount,
CommentCount,
LikeCount,
ReleaseDate,
CreateTime,
Url, 
Content
into #temp
from {0} with(nolock) 
where TypeID=isnull(@typeID, TypeID)
and ReleaseDate>=@fromDate
and ReleaseDate<@toDate
and (Title like '%'+@keyword+'%' or Content like '%'+@keyword+'%')
order by ReleaseDate desc,TypeID

select TypeID,APP,
Title,
MAX(ViewCount) as 'ViewCount',
MAX(CommentCount) as 'CommentCount',
MAX(LikeCount) as 'LikeCount',
MIN(ReleaseDate) as 'ReleaseDate',
MAX(CreateTime) as 'CreateTime'
into #result
from #temp
group by TypeID,APP,Title

select * from (
	select row_number() over (order by r2.ReleaseDate desc,r2.TypeID,r2.APP) as rownumber,
    r2.*,
    CONVERT(varchar(100), r2.ReleaseDate, 20) as ReleaseDateText,
    CONVERT(varchar(100), r2.CreateTime, 20) as CreateTimeText,
    r1.Source,r1.Content,r1.Url
    from #result r2
    inner join (select ROW_NUMBER() over (partition by APP,Title order by ViewCount desc) as NID ,* from #temp) r1 
    on r1.APP=r2.APP
    and r1.Title=r2.Title
    where r1.NID=1
) a
where rownumber>=@pageFrom and rownumber<=@pageTo 
order by rownumber

drop table #result
drop table #temp", tableName);
			SqlParameter[] paras = new SqlParameter[6];
			if (_typeID <= 0)
			{
				paras[0] = new SqlParameter("@typeID", DBNull.Value);
			}
			else
			{
				paras[0] = new SqlParameter("@typeID", _typeID);
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

		public bool IsReusable
		{
			get
			{
				return false;
			}
		}
	}
}
	