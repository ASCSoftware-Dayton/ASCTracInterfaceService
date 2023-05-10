using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;
using Windows.ApplicationModel;
using Windows.Management.Core;
using Windows.Storage;

namespace ASCTracInterfaceUtility.Data
{
    public class DBConnection
    {
        private Windows.Storage.ApplicationDataContainer localSettings;
        private Dictionary<string, Model.ModelDBConnection> DBConnectionList = new Dictionary<string, Model.ModelDBConnection>();
        private List<string> aDBConnectionList = new List<string>();
        public List<string> myDBConnectionList { get { return (aDBConnectionList); } }
        public DBConnection()
        {
            var pkgName = Package.Current.Id.FamilyName;
            //localSettings = ApplicationDataManager.CreateForPackageFamily(pkgName).LocalSettings;
            localSettings = Windows.Storage.ApplicationData.Current.LocalSettings;
            ReadConnections();
        }

        public void SetDBAlias(string alias, string DBName)
        {
            localSettings.Values[alias] = DBName;
        }
        public string GetDBAliasConnectionName(string alias)
        {
            string DBName = string.Empty;
            if (localSettings.Values[alias] != null)
                DBName = localSettings.Values[alias].ToString();
            return (DBName);
        }
        public Model.ModelDBConnection GetDBAlias(string alias)
        {
            Model.ModelDBConnection dbConn = null;

            string DBName = string.Empty;
            if (localSettings.Values[alias] != null)
                DBName = localSettings.Values[alias].ToString();
            dbConn = GetDBConnection(DBName);

            return (dbConn);
        }

        public Model.ModelDBConnection GetDBConnection(string DBName)
        {
            Model.ModelDBConnection dbConn = null;
            if (!String.IsNullOrEmpty(DBName))
            {
                Windows.Storage.ApplicationDataCompositeValue composite =
    (Windows.Storage.ApplicationDataCompositeValue)localSettings.Values[DBName];

                dbConn = new Model.ModelDBConnection(DBName);
                dbConn.fDatabase = composite["Database"].ToString();
                dbConn.fServer = composite["Server"].ToString();
                dbConn.fTLSVersion = composite["TLSVersion"].ToString();
                dbConn.fUserID = composite["UserID"].ToString();
                dbConn.fPassword = composite["Password"].ToString();
            }
            return dbConn;
        }

        public void ReadConnections()
        {
            DBConnectionList = new Dictionary<string, Model.ModelDBConnection>();
            string DBNameList = string.Empty;
            if (localSettings.Values["DBConnectionNameList"] != null)
                DBNameList = localSettings.Values["DBConnectionNameList"].ToString();
            string[] dbNames = DBNameList.Split('|');
            foreach (string dbName in dbNames)
            {
                if (!String.IsNullOrEmpty(dbName))
                {
                    if (localSettings.Values[dbName] != null)
                    {
                        Windows.Storage.ApplicationDataCompositeValue composite = (Windows.Storage.ApplicationDataCompositeValue)localSettings.Values[dbName];
                        try
                        {
                            Model.ModelDBConnection dbConn = new Model.ModelDBConnection(dbName);
                            dbConn.fDatabase = composite["Database"].ToString();
                            dbConn.fServer = composite["Server"].ToString();
                            dbConn.fTLSVersion = composite["TLSVersion"].ToString();
                            dbConn.fUserID = composite["UserID"].ToString();
                            dbConn.fPassword = composite["Password"].ToString();

                            if (!DBConnectionList.ContainsKey(dbName))
                            {
                                DBConnectionList.Add(dbName, dbConn);
                                aDBConnectionList.Add(dbName);
                            }
                        }
                        catch { }
                    }
                }
            }
        }

        public void SaveConnection(Model.ModelDBConnection rec)
        {
            string DBNameList = string.Empty;
            Windows.Storage.ApplicationDataCompositeValue composite = new ApplicationDataCompositeValue();

            composite["Database"] = rec.fDatabase;
            composite["Server"] = rec.fServer;
            composite["TLSVersion"] = rec.fTLSVersion;
            composite["UserID"] = rec.fUserID;
            composite["Password"] = rec.fPassword;

            localSettings.Values[rec.ConnectionName] = composite;

            AddDBNameToList(rec.ConnectionName);
            ReadConnections();
        }

        public void DeleteConnection(Model.ModelDBConnection rec)
        {
            if (!String.IsNullOrEmpty(rec.ConnectionName))
                RemoveDBNameFromList(rec.ConnectionName);
        }        

        public void SaveConnections()
        {
            string DBNameList = string.Empty;
            foreach (var rec in DBConnectionList.Values)
            {
                if (!String.IsNullOrEmpty(DBNameList))
                    DBNameList += "|";
                DBNameList += rec.ConnectionName;
                Windows.Storage.ApplicationDataCompositeValue composite = new ApplicationDataCompositeValue();

                composite["Database"] = rec.fDatabase;
                composite["Server"] = rec.fServer;
                composite["TLSVersion"] = rec.fTLSVersion;
                composite["UserID"] = rec.fUserID;
                composite["Password"] = rec.fPassword;

                localSettings.Values[rec.ConnectionName] = composite;
            }
            localSettings.Values["DBConnectionNameList"] = DBNameList;
        }

        private void AddDBNameToList(string aDBName)
        {
            string DBNameList = string.Empty;
            if (localSettings.Values["DBConnectionNameList"] != null)
                DBNameList = localSettings.Values["DBConnectionNameList"].ToString();
            string[] dbNames = DBNameList.Split('|');
            if (!dbNames.Contains(aDBName))
            {
                if (!String.IsNullOrEmpty(DBNameList))
                    DBNameList += "|";
                DBNameList += aDBName;
                localSettings.Values["DBConnectionNameList"] = DBNameList;
            }
        }
        private void RemoveDBNameFromList(string aDBName)
        {
            string DBNameList = string.Empty;
            if (localSettings.Values["DBConnectionNameList"] != null)
                DBNameList = localSettings.Values["DBConnectionNameList"].ToString();
            string[] dbNames = DBNameList.Split('|');
            DBNameList = string.Empty;
            foreach (string dbName in dbNames)
            {
                if( !dbName.Equals( aDBName))
                {
                    if (!String.IsNullOrEmpty(DBNameList))
                        DBNameList += "|";
                    DBNameList += aDBName;
                }
            }
        }

        public void SaveDBAlias(string alias, Model.ModelDBConnection rec)
        {
            Windows.Storage.ApplicationDataCompositeValue composite = new ApplicationDataCompositeValue();

            composite["Database"] = rec.fDatabase;
            composite["Server"] = rec.fServer;
            composite["TLSVersion"] = rec.fTLSVersion;
            composite["UserID"] = rec.fUserID;
            composite["Password"] = rec.fPassword;

            localSettings.Values[rec.ConnectionName] = composite;


            AddDBNameToList(rec.ConnectionName);
            localSettings.Values[alias] = rec.ConnectionName;
        }
    }
}