using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ETL.Models
{
    public class TargetModel
    {
        public TargetModel()
        {
            JoinModel = new JoinModel();
            AggregatorModel = new AggregatorModel();
            InputModel = new List<InputModel>();
            TargetName = ConnectedFrom = "";
        }
        public JoinModel JoinModel { get; set; }
        public AggregatorModel AggregatorModel { get; set; }
        public string TargetName { get; set; }
        public string ConnectedFrom { get; set; }
        public List<InputModel> InputModel { get; set; }

    }
}
