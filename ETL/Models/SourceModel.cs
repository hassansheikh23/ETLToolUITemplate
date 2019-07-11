using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ETL.Models
{
    public class SourceModel
    {
        public SourceModel()
        {
            ConnectionName = TableName = SourceName = ConnectedTo = SourceOutputFlags = "";
            InputModel = new List<InputModel>();
        }

        public string ConnectionName { get; set; }
        public string TableName { get; set; }
        public List<InputModel> InputModel { get; set; }
        public string SourceName { get; set; }
        public string ConnectedTo { get; set; }
        public string SourceOutputFlags { get; set; }
        
    }
}
