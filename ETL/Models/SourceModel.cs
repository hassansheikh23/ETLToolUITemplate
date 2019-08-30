using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ETL.Models
{
    [Serializable]
    public class SourceModel
    {
        public SourceModel()
        {
            ConnectionName = TableName = SourceName = ConnectedTo = SourceOutputFlags = toConnector = "";
            InputModel = new List<InputModel>();
            top = -1;
            left = -1;
            
        }

        public string ConnectionName { get; set; }
        public string TableName { get; set; }
        public List<InputModel> InputModel { get; set; }
        public string SourceName { get; set; }
        public string ConnectedTo { get; set; }
        public string toConnector { get; set; }
        public string SourceOutputFlags { get; set; }
        public int top { get; set; }
        public int left { get; set; }

    }
}
