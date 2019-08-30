using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ETL.Models
{
    [Serializable]
    public class ExpressionModel
    {
        public ExpressionModel()
        {
            SourceModel = new SourceModel();
            InputModel = new List<InputModel>();
            ToSource = FromSource = ExpressionName = toConnector = "";
            top = -1;
            left = -1;

        }

        public string ExpressionName { get; set; }
        public SourceModel SourceModel { get; set; }
        public List<InputModel> InputModel { get; set; }
        public string ToSource { get; set; }
        public string toConnector { get; set; }
        public string FromSource { get; set; }

        public int top { get; set; }
        public int left { get; set; }



    }
}
