using ETL.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ETL
{
    [Serializable]
    public class DataModelObject
    {
        public static DataModel _dataModel { get; set; }
        
        public static void SetDataModel(DataModel dm)
        {
            _dataModel = dm;
        } 

        public static DataModel GetDataModel()
        {
            return _dataModel;
        }
    }
}
