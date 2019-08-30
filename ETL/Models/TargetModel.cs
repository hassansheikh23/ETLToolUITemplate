using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ETL.Models
{
    [Serializable]
    public class TargetModel
    {
        public TargetModel()
        {
            JoinModel = new JoinModel();
            AggregatorModel = new AggregatorModel();
            FilterModel = new FilterModel();
            SourceModel = new SourceModel();
            InputModel = new List<InputModel>();
            TargetName = ConnectedFrom = "";
            top = -1;
            left = -1;
        }
        public JoinModel JoinModel { get; set; }
        public AggregatorModel AggregatorModel { get; set; }
        public FilterModel FilterModel { get; set; }
        public SourceModel SourceModel { get; set; }

        public string TargetName { get; set; }
        public string ConnectedFrom { get; set; }
        public List<InputModel> InputModel { get; set; }

        public int top { get; set; }
        public int left { get; set; }

    }
}
