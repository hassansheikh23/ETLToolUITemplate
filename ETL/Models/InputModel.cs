using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ETL.Models
{
    [Serializable]
    public class InputModel
    {
        public string ColumnId { get; set; }
        public string ColumnName { get; set; }
        public bool InputFlag { get; set; }
        public bool OutputFlag { get; set; }
        public bool GroupByFlag { get; set; }
        public bool CountFlag { get; set; }
        public bool SumFlag { get; set; }
        public bool MaxFlag { get; set; }
        public bool MinFlag { get; set; }
        public bool AvgFlag { get; set; }
        public string toDataType { get; set; }

        public InputModel Clone()
        {
            return new InputModel
            {
                ColumnId = ColumnId,
                ColumnName = ColumnName,
                InputFlag = InputFlag,
                OutputFlag = OutputFlag,
                GroupByFlag = GroupByFlag,
                toDataType = toDataType
            };
        }

    }
}
