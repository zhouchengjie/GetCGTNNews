using System;
using System.Collections.Generic;

namespace DataLibrary
{
    public class Constants
    {
        public static readonly DateTime Date_Min = new DateTime(1900, 1, 2);
        public static readonly DateTime Date_Max = new DateTime(2900, 1, 2);
        public static readonly DateTime Date_Null = new DateTime(1900, 1, 1);
        public static readonly Int32 AllValue = -999;
        public static readonly string RelevantDocumentPath = "~/Upload/Documents/";
        public static readonly string RelevantTempPath = "~/Upload/Temp/";
        public static readonly string RelevantAudiencesDocumentPath = "~/Upload/Documents/Audiences/";
        public static readonly string RelevantScreenshotPath = "~/Upload/Screenshot/";


        public class RowStyle
        {
            public const string RowUniqueID = "Row_Key";
            public const string RowStyleContent = "Row_Style";
        }
    }
}