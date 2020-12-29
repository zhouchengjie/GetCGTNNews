using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

public class AccountInfoDO : NewsCatch.DataObjectBase
{
    private int iD;
    public int ID
    {
        get { return iD; }
        set { iD = value; }
    }

    private string platform;
    public string Platform
    {
        get { return platform; }
        set { platform = value; }
    }

    private string accountID;
    public string AccountID
    {
        get { return accountID; }
        set { accountID = value; }
    }

    private string accountName;
    public string AccountName
    {
        get { return accountName; }
        set { accountName = value; }
    }

    private string url;
    public string Url
    {
        get { return url; }
        set { url = value; }
    }

    public AccountInfoDO()
    {
        this.BO_Name = "AccountInfo";
        this.PK_Name = "ID";
    }
}