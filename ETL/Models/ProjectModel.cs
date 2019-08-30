using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ETL.Models
{
    [Serializable]
    public class ProjectModel
    {
        public int id { get; set; }
        public string name { get; set; }
        public Dictionary<int,DataModel> mappings { get; set; }

       

        public ProjectModel()
        {
            id = -1;
            name = null;
            mappings = new Dictionary<int,DataModel>();
        }

    }
}
