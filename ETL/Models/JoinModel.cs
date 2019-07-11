using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ETL.Models
{
    public class JoinModel
    {
        public JoinModel()
        {
            SourceModel1 = new SourceModel();
            SourceModel2 = new SourceModel();
            SourceModel1SelectedColumn = "";
            SourceModel2SelectedColumn = "";
            JoinName = JoinType = ToSource = "";
        }
        public SourceModel SourceModel1 { get; set; }
        public SourceModel SourceModel2 { get; set; }
        public string JoinType { get; set; }
        public string JoinName { get; set; }
        public string SourceModel1SelectedColumn { get; set; }
        public string SourceModel2SelectedColumn { get; set; }
        public string ToSource { get; set; }
    }
}
