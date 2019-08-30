using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ETL.Models
{
    [Serializable]
    public class JstreeOutputmodel
    {
        public JstreeOutputmodel()
        {
            nodes = new List<JsTreeModel>();
            message = "";
        }
        public string message { get; set; }
        public List<JsTreeModel> nodes { get; set; }
    }
}
