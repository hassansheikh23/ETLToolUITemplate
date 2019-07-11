using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ETL.Models
{
    public class AggregatorModel
    {
        public AggregatorModel()
        {
            SourceModel = new SourceModel();
            InputModel = new List<InputModel>();
            ToSource = FromSource = AggregatorName = "";
        }

        public string AggregatorName { get; set; }
        public SourceModel SourceModel { get; set; }
        public List<InputModel> InputModel { get; set; }
        public string ToSource { get; set; }
        public string  FromSource { get; set; }
    }
}
