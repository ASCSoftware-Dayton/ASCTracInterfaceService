using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static System.Collections.Specialized.BitVector32;

namespace ASCTracInterfaceUtility.Model
{
    public class ModelDBConnection : INotifyPropertyChanged
    {
        public ModelDBConnection(string aConnectionName)
        {
            ConnectionName = aConnectionName;
            fTLSVersion = "1.1";
        }


        public event PropertyChangedEventHandler PropertyChanged;

        private void RaisePropertyChanged(string propertyName)
        {
            // take a copy to prevent thread issues
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }


        public string ConnectionName { get; set; }
        public string myConnString
        {
            get
            {
                string retval = "packet size=4096;";
                retval += "Max Pool Size=1024;";
                retval += "user id=" + fUserID + ";";
                if (fPassword != "")
                    retval += "Password='" + fPassword + "';";
                if (fTLSVersion.Equals("1.2"))
                {
                    retval += "Server=" + fServer + ";";
                    retval += "persist security info=False;";
                    retval += "Database=" + fDatabase;
                    // Set Encrypt=true;TrustServerCertificate=true to connection string
                    retval += ";Encrypt=True;TrustServerCertificate=True";
                }
                else
                {
                    retval += "data source=" + fServer + ";";
                    retval += "persist security info=False;";
                    retval += "initial catalog=" + fDatabase;
                }
                return (retval);
            }
        }
        public string logConnString
        {
            get
            {
                string retval = "packet size=4096;";
                retval += "Max Pool Size=1024;";
                //retval += "user id=" + fUserID + ";";
                //if (fPassword != "")
                //    retval += "Password='" + fPassword + "';";
                if (fTLSVersion.Equals("1.2"))
                {
                    retval += "Server=" + fServer + ";";
                    retval += "persist security info=False;";
                    retval += "Database=" + fDatabase;
                    retval += ";Encrypt=True;TrustServerCertificate=True";
                }
                else
                {
                    retval += "data source=" + fServer + ";";
                    retval += "persist security info=False;";
                    retval += "initial catalog=" + fDatabase;
                }
                return (retval);
            }
        }

        private string _database;
        public string fDatabase
        {
            get { return _database; }
            set
            {
                if (_database != value)
                {
                    _database = value;
                    RaisePropertyChanged("fDatabase");
                }
            }
        }
        private string _server;
        public string fServer
        {
            get { return _server; }
            set
            {
                if (_server != value)
                {
                    _server = value;
                    RaisePropertyChanged("fServer");
                }
            }
        }

        private string _fTLSVersion;
        public string fTLSVersion
        {
            get { return _fTLSVersion; }
            set
            {
                if (_fTLSVersion != value)
                {
                    _fTLSVersion = value;
                    RaisePropertyChanged("fTLSVersion");
                }
            }
        }

        private string _fUserID;
        public string fUserID
        {
            get { return _fUserID; }
            set
            {
                if (_fUserID != value)
                {
                    _fUserID = value;
                    RaisePropertyChanged("fUserID");
                }
            }
        }

        private string _fPassword;
        public string fPassword
        {
            get { return _fPassword; }
            set
            {
                if (fPassword != value)
                {
                    _fPassword = value;
                    RaisePropertyChanged("fPassword");
                }
            }
        }
    }
}
