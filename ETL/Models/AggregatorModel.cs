using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web.Mvc;

namespace ETL.Models
{
    [Serializable]
    public class AggregatorModel
    {
        public AggregatorModel()
        {
            SourceModel = new SourceModel();
            InputModel = new List<InputModel>();
            ToSource = FromSource = AggregatorName = toConnector = "";
            top = -1;
            left = -1;
        }
        #region Properties

        #endregion
        public string AggregatorName { get; set; }
        public SourceModel SourceModel { get; set; }
        public List<InputModel> InputModel { get; set; }
        public string ToSource { get; set; }
        public string toConnector { get; set; }
        public string  FromSource { get; set; }

        public int top { get; set; }
        public int left { get; set; }

        public DataModel _dataModel { get; set; }

        //Methods
        #region Aggregator Model
        public string AggregatorSettings(string containerId)
        {
            var agrModel = _dataModel.AggregatorDictionary[containerId];

            return JsonConvert.SerializeObject(agrModel);
        }

        [System.Web.Mvc.HttpPost]
        public string AggregatorSaveChanges([FromBody] SourceModel model)
        {
            //var x = fc["SourceOutputFlags"].ToString();
            if (model == null) throw new ArgumentNullException(nameof(model));
            List<string> agrColumnIds = model.SourceOutputFlags.Split(',').ToList<string>();
            //reset 'outputFlag' for specified aggregator in 'AggregatorDictionary'
            int counter = 0;
            //model.SourceName holds current selected 'aggregator' name
            foreach (var inputModel in _dataModel.AggregatorDictionary[model.SourceName].InputModel)
            {
                _dataModel.AggregatorDictionary[model.SourceName].InputModel[counter].OutputFlag = false;
                counter++;
            }

            foreach (var id in agrColumnIds)
            {
                if (!id.Equals(""))
                {
                    foreach (var inputModel in _dataModel.AggregatorDictionary[model.SourceName].InputModel)
                    {
                        if (inputModel.ColumnId.Equals(id))
                        {
                            var index = id.Substring(id.IndexOf('-') + 1);
                            inputModel.OutputFlag = true;
                            var x = _dataModel.AggregatorDictionary[model.SourceName].InputModel[Convert.ToInt32(index)] =
                                inputModel;
                            break;
                        }
                    }
                }

            }

            //update input Model here by concatenating true check in List<InputModel> obj
            _dataModel.AggregatorDictionary[model.SourceName].AggregatorName = model.SourceName;

            return JsonConvert.SerializeObject(null);

        }
        #endregion Aggregator Model
    }
}
