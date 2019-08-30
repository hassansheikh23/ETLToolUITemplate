using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ETL.Models
{
    [Serializable]
    public class OperatorModel
    {
        public string OperatorId { get; set; }
        public int top { get; set; }
        public int left { get; set; }

        public OperatorModel()
        {
            OperatorId = null;
            top = -1;
            left = -1;
        }
           
    }
}
