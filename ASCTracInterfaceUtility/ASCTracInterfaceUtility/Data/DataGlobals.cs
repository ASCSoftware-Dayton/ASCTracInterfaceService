using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace ASCTracInterfaceUtility.Data
{
    public class DataGlobals
    {
        public static string HHDELIM = "|";
        public static DataGlobals myDBGlobals;

        public DBConnection myDBConnectionInfo = new DBConnection();
        public Model.ModelDBConnection myDBConnection;
        public string AliasName;

        public DataGlobals( string alias) 
        {
            AliasName = alias;
            myDBConnection = myDBConnectionInfo.GetDBAlias( alias);
        }

        public bool ReadFieldFromDBWithPipes(string aSQLStr, string aField, ref string aData)
        {
            string mySelectQuery = aSQLStr;
            bool fFoundIt = false;

            try
            {
                aData = "";
                SqlConnection myConnection = new SqlConnection(myDBConnection.myConnString);
                SqlCommand myCommand = new SqlCommand(aSQLStr, myConnection);
                myCommand.CommandTimeout = 30;
                myConnection.Open();
                SqlDataReader myReader = myCommand.ExecuteReader();
                try
                {
                    if (myReader.Read())
                    {
                        fFoundIt = true;
                        if (aField == "")
                        {
                            for (int i = 0; i < myReader.FieldCount; i++)
                            {
                                string fieldData = myReader[i].ToString();
                                if (i > 0)
                                    aData += HHDELIM;
                                aData += fieldData;
                            }
                        }
                        else
                            aData = myReader[aField].ToString();
                    }
                }
                finally
                {
                    myConnection.Close();
                }
            }
            catch (Exception ex)
            {
                //ascLibrary.ascUtils.ascWriteLog("ReadSQL", ex.Message + "\r\n" + aSQLStr + "\r\n" + logConnString, true);
                throw new Exception(ex.Message); // + "\r\n" + aSQLStr);
            }

            return fFoundIt;
        }

        public int RunSqlCommand(string aCommandStr)
        {
            SqlConnection myConnection = new SqlConnection(myDBConnection.myConnString);

            myConnection.Open();
            SqlCommand myCommand = myConnection.CreateCommand();
            myCommand.CommandTimeout = 30;
            SqlTransaction myTransaction = myConnection.BeginTransaction();
            myCommand.Transaction = myTransaction;
            int retval = 0;

            myCommand.CommandText = "SET ANSI_NULLS ON";
            myCommand.ExecuteNonQuery();
            myCommand.CommandText = "SET NOCOUNT OFF";
            myCommand.ExecuteNonQuery();

            myCommand.CommandText = aCommandStr;

            bool fOK = false;
            try
            {
                try
                {
                    retval = myCommand.ExecuteNonQuery();
                    myTransaction.Commit();
                    fOK = true;
                }
                catch (Exception ex)
                {
                    //ascLibrary.ascUtils.ascWriteLog("ReadSQL", ex.Message + "\r\n" + aCommandStr, true);
                    throw new Exception(ex.Message);
                }
            }
            finally
            {
                if (!fOK)
                {
                    try
                    {
                        myTransaction.Rollback();
                    }
                    catch
                    {
                    }
                }
                myConnection.Close();
            }
            return (retval);
        }

    }
}
