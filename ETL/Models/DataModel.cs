using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ETL.Models
{
    public class DataModel
    {
        public DataModel()
        {
            SourceDictionary = new Dictionary<string, SourceModel>();
            AggregatorDictionary = new Dictionary<string, AggregatorModel>();
            JoinDictionary = new Dictionary<string, JoinModel>();
            TargetDictionary = new Dictionary<string, TargetModel>();
            DbConnection = new Dbconnection();
            ConnectionNames = new List<string>();
            TableList = new Dictionary<string, List<string>>();
            ConnectionString = new Dictionary<string, string>();

            ////InputModel = new List<InputModel>();
            ////ExcelDataModelList = new List<ConnectionSettings>();
            ConnectionSettingModel = new ConnectionSettings();
            SourceModel = new SourceModel();
            SourceOutputFlags = "";


        }
        
        public Dbconnection DbConnection { get; set; }
        public Dictionary<string, SourceModel> SourceDictionary { get; set; }
        public Dictionary<string, AggregatorModel>  AggregatorDictionary { get; set; }
        public Dictionary<string, JoinModel> JoinDictionary { get; set; }
        public Dictionary<string, TargetModel> TargetDictionary { get; set; }
        public List<string> ConnectionNames { get; set; }
        public Dictionary<string, List<string>> TableList { get; set; }

        ////public List<InputModel> InputModel { get; set; }
        ////public List<ConnectionSettings> ExcelDataModelList { get; set; }
        public ConnectionSettings ConnectionSettingModel { get; set; } //temporary Usage
        public SourceModel SourceModel { get; set; } //temporary Usage
        public string SourceOutputFlags { get; set; } //temporary Usage
        public Dictionary<string, string> ConnectionString { get; set; }



    }
}
