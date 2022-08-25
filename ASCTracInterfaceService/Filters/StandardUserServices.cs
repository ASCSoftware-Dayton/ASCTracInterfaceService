using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace ASCTracInterfaceService.Filters
{
    public class StandardUserServices : IUserServices
    {
        private ascLibrary.ascDBUtils myDBUtils = new ascLibrary.ascDBUtils();
        private bool fInit = false;
        private bool fUsingAuthentication = true;

        private bool InitAuthenticate()
        {
            if (!fInit)
            {
                try
                {
                    var currDT = string.Empty;
                    try
                    {
                        myDBUtils.BuildConnectString("AliasASCTrac");
                        string tmp = string.Empty;
                        if (myDBUtils.ReadFieldFromDB("select CFGDATA from CFGSETTINGS WHERE CFGFIELD = 'GWInterfaceUseAuthentication'", "", ref tmp))
                            fUsingAuthentication = tmp.Equals("T");
                    }
                    catch (Exception EX1)
                    {
                        ascLibrary.ascUtils.ascWriteLog("ASCTracInterface", "Exception at BuildConnectString: " + EX1.ToString(), false);
                        myDBUtils.myConnString = "packet size=4096;user id=app_user;Password='WeH73w';data source=asc-cin-app01;persist security info=False;initial catalog=ASCTRAC904Dev";
                    }
                    myDBUtils.ReadFieldFromDB("SELECT GETDATE()", "", ref currDT);
                    fInit = true;
                }
                catch (Exception ex)
                {
                    ascLibrary.ascUtils.ascWriteLog("ASCTracInterface", "Exception during Init Authenticate" + ex.ToString(), false);
                }
            }
            return (fInit);
        }

        public int Authenticate(string userName, string password)
        {
            int retval = 1;
            string aerrmsg = string.Empty;
            if (InitAuthenticate())
            {
                if (fUsingAuthentication)
                {
                    string tmp = string.Empty;
                    if (myDBUtils.ReadFieldFromDB("SELECT START_DATE, END_DATE, GetDate() FROM ASCREST_AUTH WHERE TOKEN_VALUE='" + password + "'", "", ref tmp))
                    {
                        DateTime startDT = ascLibrary.ascUtils.ascStrToDate(ascLibrary.ascStrUtils.GetNextWord(ref tmp), DateTime.MinValue);
                        DateTime endDT = ascLibrary.ascUtils.ascStrToDate(ascLibrary.ascStrUtils.GetNextWord(ref tmp), DateTime.MinValue);
                        DateTime currDT = ascLibrary.ascUtils.ascStrToDate(ascLibrary.ascStrUtils.GetNextWord(ref tmp), DateTime.MinValue);
                        if ((currDT < startDT) || (currDT > endDT))
                            retval = 0;
                        else
                            retval = 1;
                    }
                    else
                    {
                        retval = 0;
                    }
                }
            }

            return (retval);
        }
    }
}