using ETL.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore.Internal;
using Newtonsoft.Json;
using OfficeOpenXml;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Diagnostics;
using System.IO;
using System.Linq;

namespace ETL.Controllers
{
    public class HomeController : Controller
    {
        private static DataModel _dataModel = new DataModel();
        private static int viewCounter = 0;

        public HomeController()
        {

        }
        /*
        public class Category
        {
            public int CategoryId { get; set; }
            public string CategoryName { get; set; }
        }

        public class Product
        {
            public int ProductId { get; set; }
            public string ProductName { get; set; }
            public int CategoryId { get; set; }
        }
        List<Category> lstCat = new List<Category>()
        {
            new Category() { CategoryId = 1, CategoryName = "Source" },
            new Category() { CategoryId = 2, CategoryName = "Joiner" },
            new Category() { CategoryId = 3, CategoryName = "Aggregator" }
        };

        List<Product> lstProd = new List<Product>()
        {
            new Product() { ProductId = 1, ProductName = "Source1", CategoryId = 1 },
            new Product() { ProductId = 2, ProductName = "Source2", CategoryId = 1 },
            new Product() { ProductId = 3, ProductName = "Source3", CategoryId = 1 },
            new Product() { ProductId = 4, ProductName = "Joiner1", CategoryId = 2 },
            new Product() { ProductId = 5, ProductName = "Joiner2", CategoryId = 2 },
            new Product() { ProductId = 6, ProductName = "Joiner3", CategoryId = 2 },
            new Product() { ProductId = 7, ProductName = "Aggregator1", CategoryId = 3 },
            new Product() { ProductId = 8, ProductName = "Aggregator2", CategoryId = 3 },
            new Product() { ProductId = 9, ProductName = "Aggregator3", CategoryId = 3 }
        };
        */

        #region  Source Model
        public string GetConnections(string containerId)
        {
            var src = _dataModel.SourceDictionary[containerId];
            object obj = new
            {
                SourceModel = src,
                Connection = _dataModel.ConnectionNames
            };

            return JsonConvert.SerializeObject(obj);
        }

        public string GetTable(string connName)
        {
            //connName = @"SQL Server (StudentDb)";
            if (_dataModel.TableList.ContainsKey(connName))
            {
                var tableList = _dataModel.TableList[connName];
                //get table list link with 'connName' 

                return JsonConvert.SerializeObject(tableList);
            }

            return JsonConvert.SerializeObject(null);
        }

        [HttpGet]
        public string GetTableHeader(string connName, string tableName, string containerId)
        {
            var dm = new DataModel();
            if (connName.ToLower().Contains("SqlServer".ToLower()))
            {
                dm = SqlServerDataSource(connName, tableName);
            }
            else if (connName.ToLower().Contains("Excel".ToLower()))
            {
                dm = ExcelDataSource(connName, tableName);
            }

            //set Table Header in SourceModel
            //--Now
            if (_dataModel.SourceDictionary.Any())
            {
                if (_dataModel.SourceDictionary.ContainsKey(containerId))
                {
                    var x = _dataModel.SourceDictionary[containerId];
                    if (x.InputModel.Count > 0 && x.ConnectionName.Equals(connName) && x.TableName.Equals(tableName))
                    {

                        //_dataModel.SourceDictionary[containerId].InputModel;
                    }
                    else
                    {
                        _dataModel.SourceDictionary[containerId].InputModel = dm.SourceModel.InputModel;
                    }
                    return JsonConvert.SerializeObject(_dataModel.SourceDictionary[containerId].InputModel);
                }
            }
            else
            {
                //Link source and Input Model List
                string sourceName = containerId;
                _dataModel.SourceDictionary.Add(sourceName, dm.SourceModel);
                return JsonConvert.SerializeObject(_dataModel.SourceDictionary[containerId]);
            }

            return JsonConvert.SerializeObject(dm.SourceModel.InputModel);
        }

        [HttpPost]
        public string SourceSaveChanges([FromBody] SourceModel model)
        {
            //var x = fc["SourceOutputFlags"].ToString();
            if (model == null) throw new ArgumentNullException(nameof(model));
            List<string> sourceColumnIds = model.SourceOutputFlags.Split(',').ToList<string>();
            //reset 'outputFlag' for specified source in 'SourceDictionary'G
            int counter = 0;
            foreach (var inputModel in _dataModel.SourceDictionary[model.SourceName].InputModel)
            {
                _dataModel.SourceDictionary[model.SourceName].InputModel[counter].OutputFlag = false;
                counter++;
            }
            foreach (var id in sourceColumnIds)
            {
                foreach (var inputModel in _dataModel.SourceDictionary[model.SourceName].InputModel)
                {
                    if (inputModel.ColumnId.Equals(id))
                    {
                        var index = id.Substring(id.IndexOf('-') + 1);
                        inputModel.OutputFlag = true;
                        var x = _dataModel.SourceDictionary[model.SourceName].InputModel[Convert.ToInt32(index)] = inputModel;
                        break;
                    }
                }
            }

            //update input Model here by concatenating true check in List<InputModel> obj
            _dataModel.SourceDictionary[model.SourceName].SourceName = model.SourceName;
            _dataModel.SourceDictionary[model.SourceName].ConnectionName = model.ConnectionName;
            _dataModel.SourceDictionary[model.SourceName].TableName = model.TableName;
            _dataModel.SourceDictionary[model.SourceName].SourceName = model.SourceName;



            return JsonConvert.SerializeObject(null);
        }
        [HttpPost]
        public string ExecuteJob()
        {
            //-----validate Model
            //-----validate Model

            //-----Model Execution
            var dataModel = _dataModel;
            object[] joinerQuery = new object[8];
            //"TableSelection",//table selection
            //"TableName1",//Table Name 1
            //"TableName2",//Table Name 2
            //"JoinType",//Join Type
            //"Tbl1SelCol",//Table 1 Selected Column
            //"TblSelCol",//able 2 selected Column
            var sourceDictionary = dataModel.SourceDictionary;
            string sqlQuery = "";
            foreach (var source in sourceDictionary.Values)
            {
                var x = source.ConnectedTo.Contains("joiner");
                if (source.ConnectedTo.Contains("joiner"))
                {
                    object[] arr = new object[8];

                    var joinerModel = dataModel.JoinDictionary[source.ConnectedTo];
                    if (joinerModel.JoinName.Equals(source.ConnectedTo) && joinerModel != null)
                    {
                        string src1SelCol = joinerModel.SourceModel1SelectedColumn;
                        string src2SelCOl = joinerModel.SourceModel2SelectedColumn;
                        
                        string joinType = joinerModel.JoinType;
                        var targetModel = new TargetModel
                        {
                            ConnectedFrom = joinerModel.JoinName
                        };
                        foreach (var item in dataModel.TargetDictionary.Values)
                        {
                            if (item.ConnectedFrom.Equals(joinerModel.JoinName))
                            {
                                targetModel = item;
                                break;
                            }
                        }
                        
                        //generate Sql Server Query
                        string dbName = source.ConnectionName;
                        dbName = dbName.Substring(dbName.IndexOf('-'));
                        string tableName = source.TableName;
                        if (dbName != "" && tableName != "")
                        {
                            if (sqlQuery != "")
                            {
                                var tblColSel = "";
                                int tblLength = (tableName.Length >= 3) ? 3 : tableName.Length;
                                foreach (var item in targetModel.JoinModel.SourceModel2.InputModel)
                                {
                                    var duplicateColName = joinerQuery[0].ToString();
                                    var duplicateCol = duplicateColName.Contains(item.ColumnName.ToUpper());

                                    if (item.OutputFlag && !duplicateCol)
                                    {
                                        tblColSel += tableName.Substring(0, tblLength) + "." +  item.ColumnName.ToUpper() + ",";
                                    }
                                }
                                tblColSel = tblColSel.Substring(0, tblColSel.Length - 1);
                                joinerQuery[0] = joinerQuery[0] + tblColSel;
                                //joinerQuery[0] += "," + tableName.ElementAt(0) + ".*";
                                joinerQuery[2] = tableName;
                                joinerQuery[3] = targetModel.JoinModel.JoinType;
                                joinerQuery[5] = targetModel.JoinModel.SourceModel2SelectedColumn;
                                joinerQuery[7] = tableName.Substring(0, tblLength).ToUpper();
                                //string queryHeader = "IF OBJECT_ID('StudentRegistration" + viewCounter + "', 'V') IS NOT NULL DROP VIEW StudentRegistration3 GO use[ETLTest] GO";
                                sqlQuery = " CREATE VIEW [dbo]." + joinerQuery[1] + joinerQuery[2] +   viewCounter++ + " AS SELECT " + joinerQuery[0].ToString() + " FROM " + joinerQuery[1].ToString() + " " + joinerQuery[6].ToString() +
                                            " " + joinerQuery[3] + " " +
                                            joinerQuery[2].ToString() + " " + joinerQuery[7].ToString() +
                                            " ON " + joinerQuery[6].ToString() + "." + joinerQuery[4].ToString() +
                                            " = " + joinerQuery[7].ToString() + "." + joinerQuery[5].ToString();
                                string connetionString = _dataModel.ConnectionString[source.ConnectionName];
                                    //@"Data Source=desktop-ig62959\PCUSER;Initial Catalog=ETLTest;Integrated Security=True";
                                SqlConnection connection = new SqlConnection(connetionString);
                                connection.Open();
                                if (connection != null && connection.State == ConnectionState.Open)
                                {
                                    //SqlCommand sqlCommand = new SqlCommand("DROP VIEW IF EXISTS [StudentRegistration3]", connection);
                                    //int ret = sqlCommand.ExecuteNonQuery();
                                    SqlCommand sqlCommand = new SqlCommand(sqlQuery, connection);
                                    sqlCommand.ExecuteNonQuery();
                                }

                            }
                            else
                            {
                                sqlQuery = "Joiner";
                                int tblLength = (tableName.Length >= 3) ? 3 : tableName.Length;
                                var tblColSel = "";
                                foreach (var item in targetModel.JoinModel.SourceModel1.InputModel)
                                {
                                    if (item.OutputFlag)
                                    {
                                        tblColSel += tableName.Substring(0, tblLength) + "." + item.ColumnName.ToUpper() + ",";
                                    }
                                }
                                joinerQuery[0] = tblColSel ;
                                //joinerQuery[0] = tableName.ElementAt(0) + ".*";
                                joinerQuery[1] = tableName;
                                joinerQuery[3] = targetModel.JoinModel.JoinType;
                                joinerQuery[4] = targetModel.JoinModel.SourceModel1SelectedColumn;
                                joinerQuery[6] = tableName.Substring(0, tblLength).ToUpper();
                            }
                        } // (dbName != "" && tableName != "")
                    }
                } else if (source.ConnectedTo.Contains("aggregator"))
                {
                    object[] queryParam = new object[4];
                    var aggregatorModel = dataModel.AggregatorDictionary[source.ConnectedTo];
                    if (aggregatorModel.AggregatorName.Equals(source.ConnectedTo) && aggregatorModel != null)
                    {
                        var targetModel = new TargetModel
                        {
                            ConnectedFrom = aggregatorModel.AggregatorName
                        };
                        foreach (var item in dataModel.TargetDictionary.Values)
                        {
                            if (item.ConnectedFrom.Equals(aggregatorModel.AggregatorName))
                            {
                                targetModel = item;
                                break;
                            }
                        }
                        string dbName = source.ConnectionName;
                        dbName = dbName.Substring(dbName.IndexOf('-'));
                        string tableName = source.TableName;
                        int tblLength = (tableName.Length >= 3) ? 3 : tableName.Length;
                        queryParam[0] = tableName.Substring(0, tblLength);
                        queryParam[1] = queryParam[2] = "";
                        if (dbName != "" && tableName != "")
                        {
                            
                            foreach (var item in aggregatorModel.InputModel)
                            {
                                if (item.OutputFlag)
                                {
                                    queryParam[2] += queryParam[0] + "." + item.ColumnName + ",";
                                }
                            }
                            queryParam[2] = queryParam[2].ToString().Substring(0, queryParam[2].ToString().Length - 1);
                            //foreach (var item in source.InputModel)
                            //{

                            //    if (item.OutputFlag)
                            //    {
                            //        var duplicateColName = joinerQuery[2].ToString();
                            //        var duplicateCol = duplicateColName.Contains(item.ColumnName.ToUpper());
                            //        if (duplicateCol)
                            //        {

                            //        }
                            //        else
                            //        {
                            //            queryParam[1] += queryParam[0] + "." + item.ColumnName;
                            //        }

                            //    }
                            //}
                            sqlQuery = "Create View "+tableName+viewCounter+" AS SELECT " + queryParam[2] + " FROM [dbo]." + tableName + " " + queryParam[0] + " GROUP BY " + queryParam[2];
                            string connetionString = _dataModel.ConnectionString[source.ConnectionName];
                            //@"Data Source=desktop-ig62959\PCUSER;Initial Catalog=ETLTest;Integrated Security=True";
                            SqlConnection connection = new SqlConnection(connetionString);
                            connection.Open();
                            if (connection != null && connection.State == ConnectionState.Open)
                            {
                                //SqlCommand sqlCommand = new SqlCommand("DROP VIEW IF EXISTS [StudentRegistration3]", connection);
                                //int ret = sqlCommand.ExecuteNonQuery();
                                SqlCommand sqlCommand = new SqlCommand(sqlQuery, connection);
                                sqlCommand.ExecuteNonQuery();
                            }
                        }
                        
                        

                    }
                }


            }
            //-----Model Execution
            return JsonConvert.SerializeObject(null);
        }
        #endregion Source Model

        #region Aggregator Model
        public string AggregatorSettings(string containerId)
        {
            var agrModel = _dataModel.AggregatorDictionary[containerId];

            return JsonConvert.SerializeObject(agrModel);
        }

        [HttpPost]
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

        #region Joiner Model

        public string JoinerSettings(string containerId)
        {
            var joinModel = _dataModel.JoinDictionary[containerId];

            return JsonConvert.SerializeObject(joinModel);
        }

        [HttpPost]
        public string JoinerSaveChanges([FromBody] JoinModel model)
        {
            //var x = fc["SourceOutputFlags"].ToString();
            if (model == null) throw new ArgumentNullException(nameof(model));
            var joinModel = _dataModel.JoinDictionary[model.JoinName];
            _dataModel.JoinDictionary[model.JoinName].JoinName = model.JoinName;
            _dataModel.JoinDictionary[model.JoinName].JoinType = model.JoinType;
            _dataModel.JoinDictionary[model.JoinName].SourceModel1SelectedColumn = model.SourceModel1SelectedColumn;
            _dataModel.JoinDictionary[model.JoinName].SourceModel2SelectedColumn = model.SourceModel2SelectedColumn;

            return JsonConvert.SerializeObject(null);

        }
        #endregion Joiner Model

        #region Target Model
        public string TargetSettings(string containerId)
        {
            var targetModel = _dataModel.TargetDictionary[containerId];

            return JsonConvert.SerializeObject(targetModel);
        }
        #endregion Target Model

        [HttpPost]
        public IActionResult NewModel()
        {
            _dataModel = new DataModel();


            return RedirectToAction("Index");
        }

        public IActionResult Index()
        {
            var model = new DataModel();
            return View(model);
        }

        [HttpPost]
        public string Index(DataModel model)
        {
            if (!ModelState.IsValid)
            {
                return JsonConvert.SerializeObject(null);
            }

            var dataModel = ReadDataModel();
            if (!_dataModel.ConnectionNames.Contains(dataModel.ConnectionSettingModel.ConnectionName))
            {
                _dataModel.ConnectionNames.Add(dataModel.ConnectionSettingModel.ConnectionName);
                _dataModel.TableList.Add(dataModel.ConnectionSettingModel.ConnectionName, dataModel.ConnectionSettingModel.TableNames);
                //.TableNames = dataModel.ConnectionSettings.TableNames;
                //_dataModel.InputModel = dataModel.InputModel;
            }
            return JsonConvert.SerializeObject(null);
        }

        #region Common Data

        public string UpdateContainerVal(string container, string contId)
        {
            string containerType = container;
            string containerId = contId;
            if (containerType.Equals("source"))
            {
                if (_dataModel.SourceDictionary.Count > 0)
                {
                    if (_dataModel.SourceDictionary.ContainsKey(containerId))
                    {
                        //Already Exist
                    }
                    else
                    {
                        _dataModel.SourceDictionary.Add(containerId, new SourceModel { SourceName = containerId });
                    }
                }
                else
                {
                    _dataModel.SourceDictionary.Add(containerId, new SourceModel { SourceName = containerId });
                }
                //--Now
                //var x = _dataModel.SourceDictionary[_dataModel.SourceDictionary.Keys.ElementAt(0)].ElementAt(0).SourceName = containerId;
                //_dataModel.SourceModel.ElementAt(0).Value.SourceName = containerId;
            }
            else if (containerType.Equals("aggregator"))
            {
                if (_dataModel.AggregatorDictionary.Count > 0)
                {
                    if (_dataModel.AggregatorDictionary.ContainsKey(containerId))
                    {

                    }
                    else
                    {
                        _dataModel.AggregatorDictionary.Add(containerId, new AggregatorModel { AggregatorName = containerId });
                    }
                }
                else
                {
                    _dataModel.AggregatorDictionary.Add(containerId, new AggregatorModel { AggregatorName = containerId });
                }
            }
            else if (containerType.Equals("joiner"))
            {
                if (_dataModel.JoinDictionary.Count > 0)
                {
                    if (_dataModel.JoinDictionary.ContainsKey(containerId))
                    {
                        //Already Exist
                    }
                    else
                    {
                        _dataModel.JoinDictionary.Add(containerId, new JoinModel { JoinName = containerId });
                    }
                }
                else
                {
                    _dataModel.JoinDictionary.Add(containerId, new JoinModel { JoinName = containerId });
                }
            }
            else if (containerType.Equals("target"))
            {
                TargetModel targetModel = new TargetModel { TargetName = containerId };
                if (_dataModel.TargetDictionary.Count > 0)
                {
                    if (_dataModel.TargetDictionary.ContainsKey(containerId))
                    {
                        //Already Exist
                    }
                    else
                    {
                        _dataModel.TargetDictionary.Add(containerId, targetModel);
                    }
                }
                else
                {
                    _dataModel.TargetDictionary.Add(containerId, targetModel);
                }
            }

            return JsonConvert.SerializeObject(null);
        }

        public string UpdateContainerLinks(string fromContainer, string toContainer)
        {

            string fromOperator = fromContainer.Substring(0, fromContainer.IndexOf('_'));
            string toOperator = toContainer.Substring(0, toContainer.IndexOf('_'));
            if (fromOperator.Equals("source"))
            {
                if (toOperator.Equals("aggregator"))
                {
                    //update source link
                    _dataModel.SourceDictionary[fromContainer].ConnectedTo = toContainer;
                    SourceModel sourceModel = _dataModel.SourceDictionary[fromContainer];
                    var aggregatorModel = _dataModel.AggregatorDictionary[toContainer];
                    aggregatorModel.AggregatorName = toContainer;
                    aggregatorModel.SourceModel = sourceModel;
                    aggregatorModel.FromSource = fromContainer;
                    aggregatorModel.ToSource = toContainer;
                    if (aggregatorModel.InputModel.Count > 0)
                    {
                        aggregatorModel.InputModel.Clear();
                    }
                    foreach (var inputModel in aggregatorModel.SourceModel.InputModel)
                    {
                        if (inputModel.OutputFlag)
                        {
                            aggregatorModel.InputModel.Add(inputModel.Clone());
                        }
                        else
                        {
                            var input = inputModel;
                            input.OutputFlag = false;

                            aggregatorModel.InputModel.Add(input.Clone());
                        }
                    }
                    _dataModel.AggregatorDictionary[toContainer] = aggregatorModel;
                }
                else if (toOperator.Equals("joiner"))
                {
                    //update source link
                    _dataModel.SourceDictionary[fromContainer].ConnectedTo = toContainer;

                    var sourceModel = _dataModel.SourceDictionary[fromContainer];
                    var joinModel = _dataModel.JoinDictionary[toContainer];
                    //inspect join model
                    if (joinModel.SourceModel1.SourceName.Equals(""))
                    {
                        //fill source 1
                        joinModel.JoinName = toContainer;
                        joinModel.SourceModel1 = sourceModel;
                        _dataModel.JoinDictionary[toContainer] = joinModel;

                    }
                    else if (joinModel.SourceModel2.SourceName.Equals(""))
                    {
                        //fill source 2
                        joinModel.SourceModel2 = sourceModel;
                        _dataModel.JoinDictionary[toContainer].SourceModel2 = joinModel.SourceModel2;
                    }
                }

            }
            else if (fromOperator.Equals("aggregator"))
            {
                if (toOperator.Equals("target"))
                {
                    var agrModel = _dataModel.AggregatorDictionary[fromContainer];
                    agrModel.FromSource = fromContainer;
                    agrModel.ToSource = toContainer;
                    _dataModel.AggregatorDictionary[fromContainer] = agrModel;

                    _dataModel.TargetDictionary[toContainer].AggregatorModel = agrModel;
                    _dataModel.TargetDictionary[toContainer].ConnectedFrom = fromContainer;
                    var targetModel = _dataModel.TargetDictionary[toContainer];
                    if (targetModel.InputModel.Count > 0)
                    {
                        targetModel.InputModel.Clear();
                    }
                    //source1
                    foreach (var inputModel in targetModel.AggregatorModel.InputModel)
                    {
                        if (inputModel.OutputFlag)
                        {
                            targetModel.InputModel.Add(inputModel);
                        }
                        //else
                        //{
                        //    var input = inputModel;
                        //    input.OutputFlag = false;
                        //    targetModel.InputModel.Add(inputModel);
                        //}
                    }
                    _dataModel.TargetDictionary[toContainer] = targetModel;
                }
            }
            else if (fromOperator.Equals("joiner"))
            {
                if (toOperator.Equals("target"))
                {
                    var joinerModel = _dataModel.JoinDictionary[fromContainer];
                    joinerModel.ToSource = toContainer;
                    _dataModel.JoinDictionary[fromContainer] = joinerModel;

                    _dataModel.TargetDictionary[toContainer].JoinModel = joinerModel;
                    _dataModel.TargetDictionary[toContainer].ConnectedFrom = fromContainer;
                    var targetModel = _dataModel.TargetDictionary[toContainer];
                    if (targetModel.InputModel.Count > 0)
                    {
                        targetModel.InputModel.Clear();
                    }
                    //source1
                    foreach (var inputModel in targetModel.JoinModel.SourceModel1.InputModel)
                    {
                        if (inputModel.OutputFlag)
                        {
                            targetModel.InputModel.Add(inputModel);
                        }
                        //else
                        //{
                        //    var input = inputModel;
                        //    input.OutputFlag = false;
                        //    targetModel.InputModel.Add(inputModel);
                        //}
                    }
                    //source2
                    foreach (var inputModel in targetModel.JoinModel.SourceModel2.InputModel)
                    {
                        if (inputModel.OutputFlag)
                        {
                            targetModel.InputModel.Add(inputModel);
                        }
                        //else
                        //{
                        //    var input = inputModel;
                        //    input.OutputFlag = false;
                        //    targetModel.InputModel.Add(inputModel);
                        //}
                    }
                    _dataModel.TargetDictionary[toContainer] = targetModel;
                }
            }


            return JsonConvert.SerializeObject(null);
        }
        #endregion

        #region DataSources

        [HttpPost]
        public IActionResult ExcelConnection(DataModel model)
        {
            var retModel = ExcelSheetNames(""); //set connectionName and Table Names in ConnectionSettings
            if (!_dataModel.ConnectionNames.Contains(retModel.ConnectionSettingModel.ConnectionName))
            {
                _dataModel.ConnectionNames.Add(retModel.ConnectionSettingModel.ConnectionName);
                _dataModel.TableList.Add(retModel.ConnectionSettingModel.ConnectionName, retModel.ConnectionSettingModel.TableNames);
                _dataModel.DbConnection = retModel.DbConnection;
            }
            else
            {
                Console.WriteLine(@"Connection Already Available");
            }
            return RedirectToAction("Index", _dataModel);
        }


        /// <summary>
        /// Reads sheet/database name and sheet list/ table list
        /// </summary>
        /// <returns></returns>
        private DataModel ReadDataModel()
        {
            var retModel = ExcelSheetNames("");
            return retModel;
        }

        private IList<String> GetSelectedColumn(DataModel model)
        {
            //var selectColumns = model.AggregatorModel.ColumnsName;
            var select = new List<String>();
            //var groupByColumn = model.SourceModel.OutputFlag;
            int counter = 0;
            //foreach (var outFlag in model.SourceModel.OutputFlag)
            //{
            //    if (outFlag)
            //    {
            //        select.Add(selectColumns.ElementAt(counter));
            //    }

            //    counter++;
            //}
            return select;
        }

        /// <summary>
        /// Return filename and sheets name list
        /// </summary>
        /// <param name="fileName"></param>
        /// <returns></returns>
        private DataModel ExcelSheetNames(string fileName)
        {
            DataModel dataModel = new DataModel();
            string basePath = @"D:\1.xlsx";
            FileInfo existingFile = new FileInfo(basePath);
            using (ExcelPackage package = new ExcelPackage(existingFile))
            {
                //get the first worksheet in the workbook
                foreach (var worksheet in package.Workbook.Worksheets)
                {
                    dataModel.ConnectionSettingModel.TableNames.Add(worksheet.Name);
                }
                dataModel.DbConnection.msg = "File processed";
                dataModel.ConnectionSettingModel.ConnectionName = @"Excel-" + basePath;
                //dataModel.ConnectionSettings.ConnectionName = @"SQL Server (StudentDb)";

            }

            return dataModel;
        }

        private DataModel SqlServerDataSource(string dbName, string tableName)
        {
            var dataModel = new DataModel();
            string completeDbName = dbName;
            int loc = dbName.IndexOf('-') + 1;
            int length = dbName.Length - loc;
            dbName = dbName.Substring(loc);
            string connetionString = _dataModel.ConnectionString[completeDbName];
            //@"Data Source=desktop-ig62959\PCUSER;Initial Catalog=ETLTest;Integrated Security=True";
            SqlConnection connection = new SqlConnection(connetionString);
            connection.Open();
            if (connection != null && connection.State == ConnectionState.Open)
            {

                string tableColumnsQuery =
                    "SELECT COLUMN_NAME FROM " + dbName + ".INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = '" + tableName + "'";
                //"SELECT COLUMN_NAME FROM " + dbName + ".INFORMATION_SCHEMA.COLUMNS WHERE TABLENAME = '" + tableName + "'" ;
                SqlCommand sqlCommand = new SqlCommand(tableColumnsQuery, connection);
                SqlDataReader reader = sqlCommand.ExecuteReader();
                try
                {
                    int counter = 0;
                    while (reader.Read())
                    {
                        if (reader["COLUMN_NAME"] != null)
                        {

                            var inputModel = new InputModel()
                            {
                                ColumnId = "ETLCol-" + counter,
                                ColumnName = reader["COLUMN_NAME"].ToString(),
                                GroupByFlag = true,
                                InputFlag = true,
                                OutputFlag = true
                            };
                            counter++;
                            dataModel.SourceModel.InputModel.Add(inputModel);
                        }
                    }
                }
                catch (Exception e)
                {
                    dataModel.DbConnection.msg = "Error in loading " + dbName;
                }
                finally
                {
                    // Always call Close when done reading.
                    reader.Close();
                    connection.Close();
                }
            }
            return dataModel;
        }

        /// <summary>
        /// Return selected sheet header column list
        /// </summary>
        /// <param name="fileName"></param>
        /// <param name="sheetName"></param>
        /// <returns></returns>
        private DataModel ExcelDataSource(string fileName, string sheetName)
        {
            var dataModel = new DataModel();

            string basePath = @"D:\1.xlsx";//;fileName;
            int loc = basePath.IndexOf('-') + 1;
            int length = basePath.Length - loc;
            basePath = basePath.Substring(loc);
            FileInfo existingFile = new FileInfo(basePath);
            using (ExcelPackage package = new ExcelPackage(existingFile))
            {
                Random rand = new Random();
                //get the first worksheet in the workbook
                bool result = false;
                try
                {
                    foreach (var worksheet in package.Workbook.Worksheets)
                    {
                        if (worksheet.Name.Equals(sheetName))
                        {
                            result = true;
                            int row = 0, counter = 0;
                            var start = worksheet.Dimension.Start;
                            var end = worksheet.Dimension.End;
                            //for (int row = start.Row; row <= end.Row; row++)
                            //{ // Row by row...
                            for (int col = start.Column; col <= end.Column; col++)
                            {
                                // ... Cell by cell...
                                object cellValue = worksheet.Cells[start.Row, col].Text; // This got me the actual value I needed.
                                var inputModel = new InputModel()
                                {
                                    ColumnId = "Excel-" + counter,
                                    ColumnName = cellValue.ToString(),
                                    GroupByFlag = true,
                                    InputFlag = true,
                                    OutputFlag = true
                                };
                                counter++;
                                dataModel.SourceModel.InputModel.Add(inputModel);

                            }
                            dataModel.DbConnection.msg = "File processed";
                            //}
                            //dataModel.DbConnection.filename = basePath;
                            break;
                        }
                    }

                    if (!result)
                    {
                        dataModel.DbConnection.msg = "Error in loading " + sheetName;
                    }
                }
                catch (Exception e)
                {
                    dataModel.DbConnection.msg = "Error in loading " + sheetName;

                    Console.WriteLine(e);
                    throw;
                }

            }

            return dataModel;
        }
        #region SQL Server
        [HttpPost]
        public string SqlServerConnection([FromBody] SqlServerConnectionModel model)
        {
            DataModel dataModel = new DataModel();
            string connetionString = "";
            string dbName = "";
            if (model.ServerName != "" && model.DbName != "" && model.UserName != "" && model.Password != "")
            {
                connetionString = "Server=" + model.ServerName + ";Initial Catalog=" + model.DbName +
                                    ";Persist Security Info=False;User ID = " + model.UserName + ";Password=" + model.Password +
                                    ";MultipleActiveResultSets=False;Encrypt=True;TrustServerCertificate = False; Connection Timeout = 30;";
                dbName = model.DbName;
             /*
             Server=tcp:etldemo1.database.windows.net,1433;Initial Catalog=etl;Persist Security Info=False;
             User ID={your_username};Password={your_password};MultipleActiveResultSets=False;Encrypt=True;
             TrustServerCertificate=False;Connection Timeout=30;        
             */
            }
            else
            {
                connetionString = @"Data Source=desktop-ig62959\PCUSER;Initial Catalog=ETLTest;Integrated Security=True";
            }
            //set it to dynamic
            
            SqlConnection connection = new SqlConnection(connetionString);
            try
            {
                connection.Open();
                if (connection != null && connection.State == ConnectionState.Open)
                {

                    string dbTableQuery = "SELECT * FROM " + dbName + ".INFORMATION_SCHEMA.TABLES WHERE TABLE_TYPE = 'BASE TABLE'";
                    SqlCommand sqlCommand = new SqlCommand(dbTableQuery, connection);
                    SqlDataReader reader = sqlCommand.ExecuteReader();

                    //dataModel.TableList.Add(dbName, new List<string>());
                    try
                    {
                        while (reader.Read())
                        {
                            /*reader["TABLE_CATALOG"],
                            reader["TABLE_SCHEMA"],
                            reader["TABLE_TYPE"]*/
                            //reader["TABLE_NAME"]
                            if (reader["TABLE_NAME"] != null)
                            {
                                dataModel.ConnectionSettingModel.TableNames.Add(reader["TABLE_NAME"].ToString());
                            }
                        }
                    }
                    catch (Exception e)
                    {

                    }
                    finally
                    {
                        // Always call Close when done reading.
                        dataModel.ConnectionSettingModel.ConnectionName = "";
                        reader.Close();
                    }
                }
                dataModel.DbConnection.msg = "File processed";
                dataModel.ConnectionSettingModel.ConnectionName = @"SqlServer-" + dbName;
                dataModel.ConnectionString.Add(@"SqlServer-" + dbName, connetionString);
                var retModel = dataModel; //set connectionName and Table Names in ConnectionSettings
                if (!_dataModel.ConnectionNames.Contains(retModel.ConnectionSettingModel.ConnectionName))
                {
                    _dataModel.ConnectionNames.Add(retModel.ConnectionSettingModel.ConnectionName);
                    _dataModel.TableList.Add(retModel.ConnectionSettingModel.ConnectionName, retModel.ConnectionSettingModel.TableNames);
                    _dataModel.ConnectionString.Add(@"SqlServer-" + dbName, connetionString);
                    _dataModel.DbConnection = retModel.DbConnection; //File Processing Status
                }
                else
                {
                    Console.WriteLine(@"Connection Already Available");
                }
            }
            catch(Exception e)
            {
                string ret = "Failure";
                return JsonConvert.SerializeObject("Failure");
            }
            finally
            {
               
            }
            
            return JsonConvert.SerializeObject("Success");
        }
        #endregion

        //private static SourceModel RetrieveExcelData(DataModel dataModel)
        //{
        //    var retModel = new DataModel();
        //    Random rand = new Random();
        //    //IHostingEnvironment _hostingEnvironment = null;

        //    //New instance of ExcelEngine is created 
        //    //Equivalent to launching Microsoft Excel with no workbooks open
        //    //Instantiate the spreadsheet creation engine
        //    using (ExcelEngine excelEngine = new ExcelEngine())
        //    {
        //        //Instantiate the Excel application object
        //        IApplication application = excelEngine.Excel;

        //        //Assigns default application version
        //        application.DefaultVersion = ExcelVersion.Excel2016;

        //        //A existing workbook is opened.              
        //        string basePath = @"E:\1.xlsx";
        //        using (FileStream sampleFile = new FileStream(basePath, FileMode.Open))
        //        {
        //            try
        //            {
        //                IWorkbook workbook = application.Workbooks.Open(sampleFile);

        //                //Access first worksheet from the workbook.
        //                IWorksheet worksheet = workbook.Worksheets[0];
        //                DataTable dt = worksheet.ExportDataTable(worksheet.UsedRange, ExcelExportDataTableOptions.ColumnNames);
        //                if (dt.Rows.Count > 0)
        //                {
        //                    var headerColumn = new List<string>();
        //                    var headerDt = dt.Rows[0];
        //                    //dataModel.SourceModel.OutputFlag.Clear();
        //                    foreach (DataColumn dc in dt.Columns)
        //                    {
        //                        int res = rand.Next(0, 4) % 2;
        //                        //dataModel.SourceModel.OutputFlag.Add((res) == 0);
        //                        if (res == 0)
        //                        {
        //                            headerColumn.Add(headerDt[dc.ColumnName].ToString());
        //                        }
        //                    }
        //                    foreach (DataRow dataRow in dt.Rows.Cast<DataRow>().Skip(1))
        //                    {
        //                        int counter = 0;
        //                        var list = new List<string>();
        //                        foreach (DataColumn dc in dt.Columns)
        //                        {
        //                            //var x = dataRow[dc.ColumnName].ToString();
        //                            //if (dataModel.SourceModel.OutputFlag.ElementAt(counter) == true)
        //                            {
        //                                list.Add(dataRow[dc.ColumnName].ToString());
        //                            }
        //                            counter++;
        //                        }
        //                        //retModel.SourceModel.TableData.Add(list);

        //                    }

        //                    //retModel.SourceModel.ColumnsName = headerColumn;
        //                }

        //                retModel.DbConnection.msg = "file processed";
        //                workbook.Close();

        //                //No exception will be thrown if there are unsaved workbooks
        //                excelEngine.ThrowNotSavedOnDestroy = true;

        //                //Dispose the instance of ExcelEngine
        //                excelEngine.Dispose();
        //            }
        //            catch (Exception e)
        //            {
        //                retModel.DbConnection.msg = "file not processed";
        //            }
        //        }
        //    }

        //    return retModel;
        //}
        #endregion DataSources

        public IActionResult About()
        {
            ViewData["Message"] = "Your application description page.";

            return View();
        }

        public IActionResult Contact()
        {
            ViewData["Message"] = "Your contact page.";

            return View();
        }

        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
