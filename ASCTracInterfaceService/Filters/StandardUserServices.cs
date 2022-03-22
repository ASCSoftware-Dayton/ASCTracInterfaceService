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

        private bool InitAuthenticate()
        {
            if( !fInit)
            {
                try
                {
                    var currDT = string.Empty;
                    try
                    {
                        myDBUtils.BuildConnectString("AliasASCTrac");
                    }
                    catch( Exception EX1)
                    {
                        ascLibrary.ascUtils.ascWriteLog("ASCTracInterface", "Exception at BuildConnectString: " + EX1.ToString(), false);
                        myDBUtils.myConnString = "packet size=4096;user id=app_user;Password='WeH73w';data source=asc-cin-app01;persist security info=False;initial catalog=ASCTRAC904Dev";
                    }
                    myDBUtils.ReadFieldFromDB("SELECT GETDATE()", "", ref currDT);
                    fInit = true;
                }
                catch( Exception ex)
                {
                    ascLibrary.ascUtils.ascWriteLog("ASCTracInterface", "Exception during Init Authenticate" + ex.ToString(), false);
                }
            }
            return (fInit);
        }

        public int Authenticate(string userName, string password)
        {
            int retval = 0;
            string aerrmsg = string.Empty;
            if( InitAuthenticate())
            {
                string tmp = string.Empty;
                if( myDBUtils.ReadFieldFromDB("SELECT START_DATE, END_DATE FROM ASCREST_AUTH WHERE TOKEN_VALUE='" + password + "'", "", ref tmp))
                {
                    retval = 1;
                }              
                else
                {

                }
                /*

                string amsg = ascLibrary.dbConst.cmdSIGN_ON;
                amsg += ascLibrary.dbConst.HHDELIM + userName;
                amsg += ascLibrary.dbConst.HHDELIM + "WEB";
                amsg += ascLibrary.dbConst.HHDELIM + password;

                string rtnmsg = iParse.myParseNet.ParseMessage(amsg);

                if (rtnmsg.StartsWith(ascLibrary.dbConst.stOK))
                {
                    retval = 1;
                }
                */
            }
            return (retval);
        }
    }
}