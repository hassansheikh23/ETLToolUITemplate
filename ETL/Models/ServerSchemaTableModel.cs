using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ETL.Models
{
    [Serializable]
    public class ServerSchemaTableModel
    {
        public ServerSchemaTableModel()
        {
            ConnectionName = "";
            TableNames = new List<string>();
        }
        public List<string> TableNames { get; set; }
        public string ConnectionName { get; set; }  

    }
}
