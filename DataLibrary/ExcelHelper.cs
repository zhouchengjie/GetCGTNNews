using System;
using System.Collections.Generic;
using System.Data;
using NPOI.SS.UserModel;
using NPOI.XSSF.UserModel;
using System.IO;
using System.Linq;
using System.Reflection;

namespace DataLibrary
{
    public class ExcelHelper
    {
        /// <summary>
        /// keyword|medianame|channelname|programname
        /// </summary>
        /// <param name="file"></param>
        /// <returns></returns>
        public static List<string> LoadExcel(string file, int loadRow)
        {
            List<string> list = new List<string>();
            IWorkbook workbook = new XSSFWorkbook(file);
            ISheet sheet = workbook.GetSheetAt(0);
            IRow cellNum = sheet.GetRow(0);
            string value = null;
            int num = cellNum.LastCellNum;

            for (int i = 0; i < sheet.LastRowNum + 1; i++)
            {
                IRow row = sheet.GetRow(i);
                if (row.GetCell(loadRow - 1) != null)
                {
                    value = row.GetCell(loadRow - 1).ToString().Trim();
                    if (!string.IsNullOrWhiteSpace(value))
                    {
                        list.Add(value);
                    }
                }

            }
            workbook.Close();

            return list;
        }



        public static void ExortExcel<T>(List<T> accountList, string tableName)
        {
            IWorkbook workbook = new XSSFWorkbook();
            ISheet sheet = workbook.CreateSheet("sheet");
            IRow Title = null;
            IRow rows = null;

            if (accountList.Count > 0)
            {

                Type entityType = accountList[0].GetType();
                System.Reflection.PropertyInfo[] entityProperties = entityType.GetProperties();

                for (int i = 0; i <= accountList.Count; i++)
                {
                    if (i == 0)
                    {
                        Title = sheet.CreateRow(0);
                        for (int k = 1; k < entityProperties.Length + 1; k++)
                        {
                            Title.CreateCell(0).SetCellValue("序号");
                            Title.CreateCell(k).SetCellValue(entityProperties[k - 1].Name);
                        }

                        continue;
                    }
                    else
                    {
                        rows = sheet.CreateRow(i);
                        object entity = accountList[i - 1];
                        for (int j = 1; j <= entityProperties.Length; j++)
                        {
                            object[] entityValues = new object[entityProperties.Length];
                            entityValues[j - 1] = entityProperties[j - 1].GetValue(entity,null);
                            rows.CreateCell(0).SetCellValue(i);
                            if (entityValues[j - 1] != null)
                            {
                                string text = entityValues[j - 1].ToString();
                                if (text.Length > 32675)
                                {
                                    text = text.Substring(0, 32675);
                                }
                                rows.CreateCell(j).SetCellValue(text);
                            }
                            else
                            {
                                rows.CreateCell(j).SetCellValue("");
                            }

                        }
                    }
                }
                string fileName = Directory.GetCurrentDirectory() + "\\" + DateTime.Now.ToString("yyyyMMddHHmmss") + tableName + ".xlsx";

                using (FileStream ms = new FileStream(fileName, FileMode.OpenOrCreate))
                {
                    workbook.Write(ms);
                    ms.Close();
                }
            }



        }

    }
}
