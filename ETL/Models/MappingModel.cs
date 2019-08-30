using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ETL.Models
{
    public class MappingModel
    {
        public int MappingId { get; set; }
        public int ProjectId { get; set; }
        public string MappingName { get; set; }
        public DateTime StartDate { get; set; }
    }
}
