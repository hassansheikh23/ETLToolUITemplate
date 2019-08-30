using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ETL.Models
{
    [Serializable]
    public class SqlServerConnectionModel
    {
        public SqlServerConnectionModel()
        {
            ServerName = DbName = UserName = Password = "";
        }
        public string ServerName { get; set; }
        public string DbName { get; set; }
        public string UserName { get; set; }
        public string Password { get; set; }
        //sqlServerServerName,sqlServerDatabaseName,sqlServerUserName, sqlServerPassword
    }
}
