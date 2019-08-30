using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ETL.Models
{
    [Serializable]
    public class ExpressionInputModel
    {
        public ExpressionInputModel()
        {
            ExpressionName = columnName = DataType= "";

        }
        public string ExpressionName { get; set; }
        public string columnName { get; set; }
        public string DataType { get; set; }
    }
        
}
