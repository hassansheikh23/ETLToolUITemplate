using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ETL.Models
{
    [Serializable]
    public class SelectedFilterModel
    {
        public SelectedFilterModel()
        {
            ColumnId = ColumnName = FilterValue = FilterCondition = FilterOperator = "";
            SelfCheck = "";
        }
        public string ColumnId { get; set; }
        public string ColumnName { get; set; }
        public string FilterValue { get; set; }
        public string FilterCondition { get; set; }
        public string FilterOperator { get; set; }
        public string SelfCheck { get; set; }
    }
    [Serializable]
    public class FilterModel
    {
        public string FilterName { get; set; }
        public SourceModel SourceModel { get; set; }

        public string ToSource { get; set; }
        public string ToConnector { get; set; }
        public string FromSource { get; set; }
        public List<InputModel> InputModel { get; set; }
        public IList<SelectedFilterModel> SelectedFilterModel { get; set; }
        public int top { get; set; }
        public int left { get; set; }
        public FilterModel()
        {
            FilterName = ToSource = ToConnector = FromSource = "";  
            top = left = -1;
            SourceModel = new SourceModel();
            InputModel = new List<InputModel>();
            SelectedFilterModel = new List<SelectedFilterModel>();
        }
    }
}
