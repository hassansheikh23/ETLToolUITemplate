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
using System.Runtime.Serialization.Formatters.Binary;
using System.Threading.Tasks;


namespace ETL.Controllers
{
    public class HomeController : Controller
    {
        private static DataModel _dataModel = new DataModel();
        private static SolutionDataModel _SolutionModel = new SolutionDataModel();
        private static int viewCounter = 0;
        

        public HomeController()
        {
            
        }

        
        #region  Source Model
        public string GetConnections(string containerId)
        {
            //var src = SessionObjects.DataModel.SourceDictionary[containerId];
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

            DataModelObject.SetDataModel(_dataModel);
            DataModelObject.GetDataModel();
            //ISession session = new ISession

            return JsonConvert.SerializeObject(null);
        }

        
        [HttpPost]
        public string ExecuteJob()
        {
            //-----validate Model
            //-----validate Model

            //if (Validate()!="null")
            //{
            //    return JsonConvert.SerializeObject(Validate());
            //}

            //-----Model Execution
            var dataModel = _dataModel;
            var queryLog = "";
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
                                tableName = targetModel.JoinModel.SourceModel2.TableName;
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
                                sqlQuery = " CREATE VIEW [dbo]." + joinerQuery[1] + joinerQuery[2] + _SolutionModel.ToJulianDate(DateTime.Now) + " AS SELECT " + joinerQuery[0].ToString() + " FROM " + joinerQuery[1].ToString() + " " + joinerQuery[6].ToString() +
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
                                tableName = targetModel.JoinModel.SourceModel1.TableName;
                                int tblLength = (tableName.Length >= 3) ? 3 : tableName.Length;
                                var tblColSel = "";
                                foreach (var item in targetModel.JoinModel.SourceModel1.InputModel)
                                {
                                    if (item.OutputFlag)
                                    {
                                        tblColSel += tableName.Substring(0, tblLength)+"0" + "." + item.ColumnName.ToUpper() + ",";
                                    }
                                }
                                joinerQuery[0] = tblColSel ;
                                //joinerQuery[0] = tableName.ElementAt(0) + ".*";
                                joinerQuery[1] = tableName;
                                joinerQuery[3] = targetModel.JoinModel.JoinType;
                                joinerQuery[4] = targetModel.JoinModel.SourceModel1SelectedColumn;
                                joinerQuery[6] = tableName.Substring(0, tblLength).ToUpper()+"0";
                            }
                        } // (dbName != "" && tableName != "")
                    }
                }
                else if (source.ConnectedTo.Contains("aggregator"))
                {
                    SourceAggregatorQuery(source, dataModel);
                    //object[] queryParam = new object[4];
                    //var aggregatorModel = dataModel.AggregatorDictionary[source.ConnectedTo];
                    //if (aggregatorModel.AggregatorName.Equals(source.ConnectedTo) && aggregatorModel != null)
                    //{
                    //    var targetModel = new TargetModel
                    //    {
                    //        ConnectedFrom = aggregatorModel.AggregatorName
                    //    };
                    //    foreach (var item in dataModel.TargetDictionary.Values)
                    //    {
                    //        if (item.ConnectedFrom.Equals(aggregatorModel.AggregatorName))
                    //        {
                    //            targetModel = item;
                    //            break;
                    //        }
                    //    }
                    //    string dbName = source.ConnectionName;
                    //    dbName = dbName.Substring(dbName.IndexOf('-'));
                    //    string tableName = source.TableName;
                    //    int tblLength = (tableName.Length >= 3) ? 3 : tableName.Length;
                    //    queryParam[0] = tableName.Substring(0, tblLength);
                    //    queryParam[1] = queryParam[2] = "";
                    //    if (dbName != "" && tableName != "")
                    //    {

                    //        foreach (var item in aggregatorModel.InputModel)
                    //        {
                    //            if (item.OutputFlag)
                    //            {
                    //                queryParam[2] += queryParam[0] + "." + item.ColumnName + ",";
                    //            }
                    //        }
                    //        queryParam[2] = queryParam[2].ToString().Substring(0, queryParam[2].ToString().Length - 1);

                    //        sqlQuery = "Create View " + tableName + _SolutionModel.ToJulianDate(DateTime.Now)  + " AS SELECT " + queryParam[2] + " FROM [dbo]." + tableName + " " + queryParam[0] + " GROUP BY " + queryParam[2];
                    //        string connetionString = _dataModel.ConnectionString[source.ConnectionName];
                    //        //@"Data Source=desktop-ig62959\PCUSER;Initial Catalog=ETLTest;Integrated Security=True";
                    //        SqlConnection connection = new SqlConnection(connetionString);
                    //        connection.Open();
                    //        if (connection != null && connection.State == ConnectionState.Open)
                    //        {
                    //            //SqlCommand sqlCommand = new SqlCommand("DROP VIEW IF EXISTS [StudentRegistration3]", connection);
                    //            //int ret = sqlCommand.ExecuteNonQuery();
                    //            SqlCommand sqlCommand = new SqlCommand(sqlQuery, connection);
                    //            sqlCommand.ExecuteNonQuery();
                    //        }
                    //    }
                    //}
                    queryLog = SourceAggregatorQuery(source, dataModel);
                }
                else if (source.ConnectedTo.Contains("filter"))
                {
                    queryLog = SourceFilterQuery(source, dataModel);
                }
                else if (source.ConnectedTo.Contains("expression"))
                {
                    queryLog = SourceExpressionQuery(source, dataModel);
                }
                //Expression
            }
            //-----Model Execution
            if(queryLog == "")
            {
                queryLog = "mapping is executed";
            }
            return JsonConvert.SerializeObject(queryLog);
        }
        #endregion Source Model
        #region Execute
        public TargetModel GetTargetModel(string filterName, DataModel dataModel)
        {
            var targetModel = dataModel.TargetDictionary[filterName];

            return targetModel;
        }

        public string getTablePrefix(string tableName)
        {
            int tblLength = (tableName.Length >= 3) ? 3 : tableName.Length;
            string res = tableName.Substring(0, tblLength);
            return res;
        }

        public string ConvertFilterConditionToScript(SelectedFilterModel selectedFilterModel)
        {
            string strFilterquery = "";

            return strFilterquery;
        } 
        public string SourceFilterQuery(SourceModel source, DataModel dataModel)
        {
            string queryLog = "";
            object[] queryParam = new object[4]; //0 -> Table prefix
            var filterModel = dataModel.FilterDictionary[source.ConnectedTo];
            if (filterModel.FilterName.Equals(source.ConnectedTo) && filterModel != null)
            {
                var targetModel = new TargetModel
                {
                    ConnectedFrom = filterModel.FilterName
                };
                foreach (var item in dataModel.TargetDictionary.Values)
                {
                    if (item.ConnectedFrom.Equals(filterModel.FilterName))
                    {
                        targetModel = item;
                        break;
                    }
                }
                if (targetModel != null)
                {
                    string dbName = source.ConnectionName.Substring(source.ConnectionName.IndexOf('-'));
                    string tableName = source.TableName;
                    queryParam[0] = getTablePrefix(tableName);
                    queryParam[1] = queryParam[2] = "";
                    var columnList = new List<string>();
                    foreach (var item in source.InputModel)
                    {
                        columnList.Add(item.ColumnName);
                    }
                    queryParam[3] = columnList;
                    int selectedFilterModelCount = filterModel.SelectedFilterModel.Count;
                    int index = 0;
                    foreach (var item in filterModel.SelectedFilterModel)
                    {
                        string strFilterquery = "";
                        if (!item.ColumnName.Equals(""))
                        {
                            if (item.FilterOperator.Equals("Equal"))
                            {
                                strFilterquery = queryParam[0] + "." + item.ColumnName + " = '" + item.FilterValue + "'";
                            }
                            else if (item.FilterOperator.Equals("IN"))
                            {
                                strFilterquery = queryParam[0] + "." + item.ColumnName + " IN (" + arrangeStr(item.FilterValue) + ")";
                            }
                            else if (item.FilterOperator.Equals("NOTIN"))
                            {
                                strFilterquery = queryParam[0] + "." + item.ColumnName + " NOT IN (" + arrangeStr(item.FilterValue) + ")";
                            }
                            else if (item.FilterOperator.Equals("LIKE"))
                            {
                                strFilterquery = queryParam[0] + "." + item.ColumnName + " LIKE ('%" + item.FilterValue + "%')";
                            }
                            //check current filter criteria is the last one 
                            if ((index != selectedFilterModelCount - 1))
                            {
                                strFilterquery += " " + item.FilterCondition + " ";
                            }
                            index++;
                            queryParam[2] += strFilterquery;
                        }
                    }
                    string createTableQuery = "";
                    string strTable = tableName + _SolutionModel.ToJulianDate(DateTime.Now);
                    createTableQuery += "CREATE TABLE " + dbName.Substring(1) + ".[dbo]." + strTable + "(";
                    createTableQuery += tableName + "_Id int IDENTITY(1,1) NOT NULL,";
                    string temp = "";
                    foreach (var item in columnList)
                    {
                        temp += item + " varchar(100) NULL" + " ,";
                    }
                    temp = temp.Substring(0, temp.Length - 1);
                    createTableQuery += temp + ")";
                    //queryParam[2] = queryParam[2].ToString().Substring(0, queryParam[2].ToString().Length - 1);
                    var s = String.Join(",", columnList);
                    //sqlQuery = "Create View " + tableName + _SolutionModel.ToJulianDate(DateTime.Now) + " AS SELECT * FROM " + dbName.Substring(1) + ".[dbo]." + tableName + " " + queryParam[0] + " WHERE " + queryParam[2];
                    string str = "INSERT INTO " + dbName.Substring(1) + ".[dbo]." + strTable + "(" + s + ") ";
                    string sqlQuery = str + " SELECT * FROM " + dbName.Substring(1) + ".[dbo]." + tableName + " " + queryParam[0] + " WHERE " + queryParam[2];
                    string connetionString = _SolutionModel.getConnectionString();
                    //@"Data Source=desktop-ig62959\PCUSER;Initial Catalog=ETLTest;Integrated Security=True";
                    SqlConnection connection = new SqlConnection(connetionString);
                    connection.Open();
                    if (connection != null && connection.State == ConnectionState.Open)
                    {
                        SqlCommand sqlCommand = new SqlCommand(createTableQuery, connection);
                        try
                        {
                            int ret = sqlCommand.ExecuteNonQuery();
                            sqlCommand = new SqlCommand(sqlQuery, connection);
                            var res = sqlCommand.ExecuteNonQuery();
                        }
                        catch (Exception e)
                        {
                            queryLog += e.InnerException;
                        }
                    }
                }// target Check

                
            }
            return queryLog;
        }
        public String SourceExpressionQuery(SourceModel source, DataModel dataModel)
        {
            string queryLog = "";
            object[] queryParam = new object[4];
            var expressionModel = dataModel.ExpressionDictionary[source.ConnectedTo];
            if (expressionModel.ExpressionName.Equals(source.ConnectedTo) && expressionModel != null)
            {
                var targetModel = new TargetModel
                {
                    ConnectedFrom = expressionModel.ExpressionName
                };
                foreach (var item in dataModel.TargetDictionary.Values)
                {
                    if (item.ConnectedFrom.Equals(expressionModel.ExpressionName))
                    {
                        targetModel = item;
                        break;
                    }
                }
                if (targetModel != null)
                {
                    string dbName = source.ConnectionName.Substring(source.ConnectionName.IndexOf('-'));
                    string tableName = source.TableName;
                    queryParam[0] = getTablePrefix(tableName);
                    queryParam[1] = queryParam[2] = "";
                    var columnList = new List<string>();
                    //foreach (var item in source.InputModel)
                    //{
                    //    columnList.Add(item.ColumnName);
                    //}
                    queryParam[3] = columnList;
                    foreach (var item in expressionModel.InputModel)
                    {
                        if (item.toDataType != null && item.OutputFlag == true)
                        {
                            if (item.toDataType.Equals("Integar"))
                            {
                                queryParam[2] += "CAST(" + queryParam[0] + "." + item.ColumnName + " AS INT)" + "AS " + item.ColumnName + ",";
                            }
                            else if (item.toDataType.Equals("Float"))
                            {
                                queryParam[2] += "CAST(" + queryParam[0] + "." + item.ColumnName + " AS Float) " + "AS " + item.ColumnName + ",";
                            }
                            else if (item.toDataType.Equals("Date"))
                            {
                                queryParam[2] += "CAST(" + queryParam[0] + "." + item.ColumnName + " AS Date) " + "AS " + item.ColumnName + ",";
                            }
                            else
                            {
                                queryParam[2] += "CAST(" + queryParam[0] + "." + item.ColumnName + " AS VARCHAR) " + "AS " + item.ColumnName + ",";
                            }
                            columnList.Add(item.ColumnName);
                        }
                    }
                    queryParam[2] = queryParam[2].ToString().Substring(0, queryParam[2].ToString().Length - 1);

                    string createTableQuery = "";
                    string strTable = tableName + _SolutionModel.ToJulianDate(DateTime.Now);
                    createTableQuery += "CREATE TABLE " + dbName.Substring(1) + ".[dbo]." + strTable + "(";
                    createTableQuery += tableName + "_Id int IDENTITY(1,1) NOT NULL,";
                    string temp = "";
                    foreach (var item in columnList)
                    {
                        temp += item + " varchar(100) NULL" + " ,";
                    }
                    temp = temp.Substring(0, temp.Length - 1);
                    createTableQuery += temp + ")";
                    var columnListSelect = String.Join(",", columnList);
                    string str = "INSERT INTO " + dbName.Substring(1) + ".[dbo]." + strTable + "(" + columnListSelect + ") ";
                    string sqlQuery = str + "SELECT " + queryParam[2] + " FROM " + dbName.Substring(1) + ".[dbo]." + tableName + " " + queryParam[0];
                    //sqlQuery = "SELECT " + queryParam[2] + " FROM " + dbName.Substring(1) + ".[dbo]." + tableName + " " + queryParam[0];
                    string connetionString = _SolutionModel.getConnectionString();
                    SqlConnection connection = new SqlConnection(connetionString);
                    connection.Open();
                    if (connection != null && connection.State == ConnectionState.Open)
                    {
                        SqlCommand sqlCommand = new SqlCommand(createTableQuery, connection);
                        try
                        {
                            int ret = sqlCommand.ExecuteNonQuery();
                            sqlCommand = new SqlCommand(sqlQuery, connection);
                            var res = sqlCommand.ExecuteNonQuery();
                        }
                        catch (Exception e)
                        {
                            queryLog += e.InnerException;
                        }
                    }
                }
            }
            return queryLog;
        }

        public string SourceAggregatorQuery(SourceModel source, DataModel dataModel)
        {
            string queryLog = "";
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
                if (targetModel != null)
                {
                    string dbName = source.ConnectionName.Substring(source.ConnectionName.IndexOf('-'));
                    string tableName = source.TableName;
                    queryParam[0] = getTablePrefix(tableName);
                    queryParam[1] = queryParam[2] = "";
                    var columnList = new List<string>();
                    //foreach (var item in source.InputModel)
                    //{
                    //    columnList.Add(item.ColumnName);
                    //}
                    queryParam[3] = columnList;

                    foreach (var item in aggregatorModel.InputModel)
                    {
                        if (item.OutputFlag)
                        {
                            queryParam[2] += queryParam[0] + "." + item.ColumnName + ",";
                            columnList.Add(item.ColumnName);
                        }
                    }
                    queryParam[2] = queryParam[2].ToString().Substring(0, queryParam[2].ToString().Length - 1);

                    string createTableQuery = "";
                    string strTable = tableName + _SolutionModel.ToJulianDate(DateTime.Now);
                    createTableQuery += "CREATE TABLE " + dbName.Substring(1) + ".[dbo]." + strTable + "(";
                    createTableQuery += tableName + "_Id int IDENTITY(1,1) NOT NULL,";
                    string temp = "";
                    foreach (var item in columnList)
                    {
                        temp += item + " varchar(100) NULL" + " ,";
                    }
                    temp = temp.Substring(0, temp.Length - 1);
                    createTableQuery += temp + ")";
                    var columnListSelect = String.Join(",", columnList);
                    string str = "INSERT INTO " + dbName.Substring(1) + ".[dbo]." + strTable + "(" + columnListSelect + ") ";
                    string sqlQuery = str + " SELECT " + queryParam[2] + " FROM " + dbName.Substring(1) + ".[dbo]." + tableName + " " + queryParam[0] + " GROUP BY " + queryParam[2];
                    //sqlQuery = "SELECT " + queryParam[2] + " FROM [dbo]." + tableName + " " + queryParam[0] + " GROUP BY " + queryParam[2];
                    string connetionString = _SolutionModel.getConnectionString();
                    SqlConnection connection = new SqlConnection(connetionString);
                    connection.Open();
                    if (connection != null && connection.State == ConnectionState.Open)
                    {
                        SqlCommand sqlCommand = new SqlCommand(createTableQuery, connection);
                        try
                        {
                            //sqlQuery = "Create View " + tableName + _SolutionModel.ToJulianDate(DateTime.Now) + " AS SELECT " + queryParam[2] + " FROM [dbo]." + tableName + " " + queryParam[0] + " GROUP BY " + queryParam[2];
                            int ret = sqlCommand.ExecuteNonQuery();
                            sqlCommand = new SqlCommand(sqlQuery, connection);
                            var res = sqlCommand.ExecuteNonQuery();
                        }
                        catch (Exception e)
                        {
                            queryLog += e.InnerException;
                        }
                    }
                }

            }
            return queryLog;
        }

        #endregion


        public string arrangeStr(string str)
        {
            string[] split = str.Split(',');
            var res = "";
            foreach (string item in split)
            {
                res += "'" + item + "',";
            }
            res = res.Substring(0, res.Length - 1);
            return res;
        }

        #region Expression Model
        public string ExpressionSettings(string containerId)
        {
            var expModel = _dataModel.ExpressionDictionary[containerId];

            return JsonConvert.SerializeObject(expModel);
        }

        [HttpPost]
        public string ExpressionSaveChanges([FromBody] ExpressionInputModel[] model)
        {
            //var x = fc["SourceOutputFlags"].ToString();
            if (model == null) throw new ArgumentNullException(nameof(model));
            var _inputModel = _dataModel.ExpressionDictionary[model[0].ExpressionName].InputModel;
            foreach (var m in model)
            {
                foreach (var inp in _inputModel)
                {
                    if (inp.ColumnName == m.columnName)
                    {
                        if (m.DataType != "--Select DataType--")
                            inp.toDataType = m.DataType;
                        else
                        {
                            inp.toDataType = "";
                        }
                    }
                }
            }


            return JsonConvert.SerializeObject(null);

        }
        #endregion Expression Model

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

        #region Filter Model

        public string FilterSettings(string containerId)
        {

            var filterModel = _dataModel.FilterDictionary[containerId];

            return JsonConvert.SerializeObject(filterModel);
        }

        [HttpPost]
        public string FilterSaveChanges([FromBody] FilterModel model)
        {

            var filterModel = _dataModel.FilterDictionary[model.FilterName];
            filterModel.SelectedFilterModel = model.SelectedFilterModel;

            _dataModel.FilterDictionary[model.FilterName] = filterModel;

            return JsonConvert.SerializeObject(null);
        }
        #endregion
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
            var x = HttpContext.Items[""];
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
            else if (containerType.Equals("filter"))
            {
                if (_dataModel.FilterDictionary.Count > 0)
                {
                    if (_dataModel.FilterDictionary.ContainsKey(containerId))
                    {

                    }
                    else
                    {
                        _dataModel.FilterDictionary.Add(containerId, new FilterModel { FilterName = containerId });
                    }
                }
                else
                {
                    _dataModel.FilterDictionary.Add(containerId, new FilterModel { FilterName = containerId });
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
            else if (containerType.Equals("expression"))
            {
                if (_dataModel.ExpressionDictionary.Count > 0)
                {
                    if (_dataModel.ExpressionDictionary.ContainsKey(containerId))
                    {

                    }
                    else
                    {
                        _dataModel.ExpressionDictionary.Add(containerId, new ExpressionModel { ExpressionName = containerId });
                    }
                }
                else
                {
                    _dataModel.ExpressionDictionary.Add(containerId, new ExpressionModel { ExpressionName = containerId });
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

        public string UpdateContainerLinks(string fromContainer, string toContainer, string toConnector)
        {

            string fromOperator = fromContainer.Substring(0, fromContainer.IndexOf('_'));
            string toOperator = toContainer.Substring(0, toContainer.IndexOf('_'));
            if (fromOperator.Equals("source"))
            {
                if (toOperator.Equals("aggregator"))
                {
                    //update source link
                    _dataModel.SourceDictionary[fromContainer].ConnectedTo = toContainer;
                    _dataModel.SourceDictionary[fromContainer].toConnector = toConnector;
                    SourceModel sourceModel = _dataModel.SourceDictionary[fromContainer];
                    var aggregatorModel = _dataModel.AggregatorDictionary[toContainer];
                    aggregatorModel.AggregatorName = toContainer;
                    aggregatorModel.SourceModel = sourceModel;
                    aggregatorModel.FromSource = fromContainer;
                    //aggregatorModel.ToSource = toContainer;
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
                else if (toOperator.Equals("filter"))
                {
                    //update source link
                    _dataModel.SourceDictionary[fromContainer].ConnectedTo = toContainer;
                    _dataModel.SourceDictionary[fromContainer].toConnector = toConnector;
                    SourceModel sourceModel = _dataModel.SourceDictionary[fromContainer];
                    var filterModel = _dataModel.FilterDictionary[toContainer];
                    filterModel.FilterName = toContainer;
                    filterModel.SourceModel = sourceModel;
                    filterModel.FromSource = fromContainer;
                    if (filterModel.InputModel.Count > 0)
                    {
                        filterModel.InputModel.Clear();
                    }
                    foreach (var inputModel in filterModel.SourceModel.InputModel)
                    {
                        if (inputModel.OutputFlag)
                        {
                            filterModel.InputModel.Add(inputModel.Clone());
                        }
                        else
                        {
                            var input = inputModel;
                            input.OutputFlag = false;

                            filterModel.InputModel.Add(input.Clone());
                        }
                    }
                    _dataModel.FilterDictionary[toContainer] = filterModel;
                }
                else if (toOperator.Equals("expression"))
                {
                    //update source link
                    _dataModel.SourceDictionary[fromContainer].ConnectedTo = toContainer;
                    _dataModel.SourceDictionary[fromContainer].toConnector = toConnector;
                    SourceModel sourceModel = _dataModel.SourceDictionary[fromContainer];
                    var expressionModel = _dataModel.ExpressionDictionary[toContainer];
                    expressionModel.ExpressionName = toContainer;
                    expressionModel.SourceModel = sourceModel;
                    expressionModel.FromSource = fromContainer;
                    //expressionModel.ToSource = toContainer;
                    if (expressionModel.InputModel.Count > 0)
                    {
                        expressionModel.InputModel.Clear();
                    }
                    foreach (var inputModel in expressionModel.SourceModel.InputModel)
                    {
                        if (inputModel.OutputFlag)
                        {
                            expressionModel.InputModel.Add(inputModel);
                        }
                        else
                        {
                            var input = inputModel;
                            input.OutputFlag = false;

                            expressionModel.InputModel.Add(input);
                        }
                    }
                    _dataModel.ExpressionDictionary[toContainer] = expressionModel;
                }
                else if (toOperator.Equals("joiner"))
                {
                    //update source link
                    _dataModel.SourceDictionary[fromContainer].ConnectedTo = toContainer;
                    _dataModel.SourceDictionary[fromContainer].toConnector = toConnector;
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
                else if(toOperator.Equals("target"))
                {
                    var sourceModel = _dataModel.SourceDictionary[fromContainer];
                    sourceModel.ConnectedTo = toContainer;
                    _dataModel.SourceDictionary[fromContainer].toConnector = toConnector;
                    _dataModel.SourceDictionary[fromContainer] = sourceModel;

                    _dataModel.TargetDictionary[toContainer].SourceModel = sourceModel;
                    _dataModel.TargetDictionary[toContainer].ConnectedFrom = fromContainer;
                    var targetModel = _dataModel.TargetDictionary[toContainer];
                    if (targetModel.InputModel.Count > 0)
                    {
                        targetModel.InputModel.Clear();
                    }
                    //source1
                    foreach (var inputModel in targetModel.SourceModel.InputModel)
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
            else if (fromOperator.Equals("aggregator"))
            {
                if (toOperator.Equals("target"))
                {
                    var agrModel = _dataModel.AggregatorDictionary[fromContainer];
                    //agrModel.FromSource = fromContainer;
                    agrModel.ToSource = toContainer;
                    agrModel.toConnector = toConnector;
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
                    joinerModel.toConnector = toConnector;
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
            else if (fromOperator.Equals("filter"))
            {
                if (toOperator.Equals("target"))
                {
                    var filterModel = _dataModel.FilterDictionary[fromContainer];
                    //agrModel.FromSource = fromContainer;
                    filterModel.ToSource = toContainer;
                    filterModel.ToConnector = toConnector;
                    _dataModel.FilterDictionary[fromContainer] = filterModel;

                    _dataModel.TargetDictionary[toContainer].FilterModel = filterModel;
                    _dataModel.TargetDictionary[toContainer].ConnectedFrom = fromContainer;
                    var targetModel = _dataModel.TargetDictionary[toContainer];
                    if (targetModel.InputModel.Count > 0)
                    {
                        targetModel.InputModel.Clear();
                    }
                    //source1
                    foreach (var selFilterModel in targetModel.FilterModel.SelectedFilterModel)
                    {
                        //if (!selFilterModel.ColumnId.Equals(""))
                        //{
                        //    targetModel.InputModel.Add(targetModel.);
                        //}
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
            else if (fromOperator.Equals("expression"))
            {
                if (toOperator.Equals("target"))
                {
                    var expModel = _dataModel.ExpressionDictionary[fromContainer];
                    //agrModel.FromSource = fromContainer;
                    expModel.ToSource = toContainer;
                    expModel.toConnector = toConnector;
                    _dataModel.ExpressionDictionary[fromContainer] = expModel;

                    //_dataModel.TargetDictionary[toContainer].ExpressionMod = filterModel;
                    _dataModel.TargetDictionary[toContainer].ConnectedFrom = fromContainer;
                    var targetModel = _dataModel.TargetDictionary[toContainer];
                    var inputModel = new List<InputModel>();
                    foreach(var input in expModel.InputModel)
                    {
                        inputModel.Add(input.Clone());
                    }
                    if (targetModel.InputModel.Count > 0)
                    {
                        targetModel.InputModel.Clear();
                    }
                    targetModel.InputModel = inputModel;
                    //source1
                    
                    _dataModel.TargetDictionary[toContainer] = targetModel;
                }
            }

                return JsonConvert.SerializeObject(null);
        }

        public string DeleteContainerLinks(string fromContainer, string toContainer, string toConnector)
        {

            string fromOperator = fromContainer.Substring(0, fromContainer.IndexOf('_'));
            string toOperator = toContainer.Substring(0, toContainer.IndexOf('_'));
            if (fromOperator.Equals("source"))
            {
                _dataModel.SourceDictionary[fromContainer].ConnectedTo = "";
                _dataModel.SourceDictionary[fromContainer].toConnector = "";
                if (toOperator.Equals("aggregator"))
                {
                    //delete source link
                    var oldArgModel = _dataModel.AggregatorDictionary[toContainer];
                    var newAgrModel = new AggregatorModel();
                    newAgrModel.left = oldArgModel.left;
                    newAgrModel.top = oldArgModel.top;
                    newAgrModel.AggregatorName = oldArgModel.AggregatorName;
                    newAgrModel.toConnector = oldArgModel.toConnector;
                    newAgrModel.ToSource = oldArgModel.ToSource;
                    _dataModel.AggregatorDictionary[toContainer] = newAgrModel;
                }
                else if (toOperator.Equals("filter"))
                {
                    //update source link
                    var oldFilterModel = _dataModel.FilterDictionary[toContainer];
                    var newFilterModel = new FilterModel();
                    newFilterModel.left = oldFilterModel.left;
                    newFilterModel.top = oldFilterModel.top;
                    newFilterModel.FilterName = oldFilterModel.FilterName;
                    newFilterModel.ToConnector = oldFilterModel.ToConnector;
                    newFilterModel.ToSource = oldFilterModel.ToSource;
                    _dataModel.FilterDictionary[toContainer] = newFilterModel;
                }
                else if (toOperator.Equals("expression"))
                {
                    //update source link
                    var oldExpModel = _dataModel.ExpressionDictionary[toContainer];
                    var newExpModel = new ExpressionModel();
                    newExpModel.left = oldExpModel.left;
                    newExpModel.top = oldExpModel.top;
                    newExpModel.ExpressionName = oldExpModel.ExpressionName;
                    newExpModel.toConnector = oldExpModel.toConnector;
                    newExpModel.ToSource = oldExpModel.ToSource;
                    _dataModel.ExpressionDictionary[toContainer] = newExpModel;
                }
                else if (toOperator.Equals("joiner"))
                {
                    //update source link
                    var oldJoinModel = _dataModel.JoinDictionary[toContainer];
                    var newJoinModel = new JoinModel();
                    //inspect join model
                    if (toConnector == "Input_1")
                    {
                        //fill source 1
                        newJoinModel.JoinName = oldJoinModel.JoinName;
                        newJoinModel.SourceModel2 = oldJoinModel.SourceModel2;
                        newJoinModel.SourceModel2SelectedColumn = oldJoinModel.SourceModel2SelectedColumn;
                        newJoinModel.ToSource = oldJoinModel.ToSource;
                        newJoinModel.left = newJoinModel.left;
                        newJoinModel.top = newJoinModel.top;
                        newJoinModel.toConnector = newJoinModel.toConnector;
                        _dataModel.JoinDictionary[toContainer] = newJoinModel;

                    }
                    else if (toConnector == "Input_2")
                    {
                        //fill source 2
                        newJoinModel.JoinName = oldJoinModel.JoinName;
                        newJoinModel.SourceModel1 = oldJoinModel.SourceModel2;
                        newJoinModel.SourceModel1SelectedColumn = oldJoinModel.SourceModel2SelectedColumn;
                        newJoinModel.ToSource = oldJoinModel.ToSource;
                        newJoinModel.left = newJoinModel.left;
                        newJoinModel.top = newJoinModel.top;
                        newJoinModel.toConnector = newJoinModel.toConnector;
                        _dataModel.JoinDictionary[toContainer] = newJoinModel;
                    }
                }
                else if (toOperator.Equals("target"))
                {

                    var oldTrgModel = _dataModel.TargetDictionary[toContainer];
                    var newTrgModel = new TargetModel();
                    newTrgModel.left = oldTrgModel.left;
                    newTrgModel.top = oldTrgModel.top;
                    newTrgModel.TargetName = oldTrgModel.TargetName;
                    _dataModel.TargetDictionary[toContainer] = newTrgModel;
                }

            }
            else if (fromOperator.Equals("aggregator"))
            {
                var agrModel = _dataModel.AggregatorDictionary[fromContainer];
                //agrModel.FromSource = fromContainer;
                agrModel.ToSource = "";
                agrModel.toConnector = "";
                _dataModel.AggregatorDictionary[fromContainer] = agrModel;
                if (toOperator.Equals("target"))
                {
                    var oldTrgModel = _dataModel.TargetDictionary[toContainer];
                    var newTrgModel = new TargetModel();
                    newTrgModel.left = oldTrgModel.left;
                    newTrgModel.top = oldTrgModel.top;
                    newTrgModel.TargetName = oldTrgModel.TargetName;
                    _dataModel.TargetDictionary[toContainer] = newTrgModel;
                }
            }
            else if (fromOperator.Equals("joiner"))
            {
                var joinerModel = _dataModel.JoinDictionary[fromContainer];
                joinerModel.ToSource = "";
                joinerModel.toConnector = "";
                _dataModel.JoinDictionary[fromContainer] = joinerModel;
                if (toOperator.Equals("target"))
                {
                    var oldTrgModel = _dataModel.TargetDictionary[toContainer];
                    var newTrgModel = new TargetModel();
                    newTrgModel.left = oldTrgModel.left;
                    newTrgModel.top = oldTrgModel.top;
                    newTrgModel.TargetName = oldTrgModel.TargetName;
                    _dataModel.TargetDictionary[toContainer] = newTrgModel;
                }
            }
            else if (fromOperator.Equals("filter"))
            {
                var filterModel = _dataModel.FilterDictionary[fromContainer];
                //agrModel.FromSource = fromContainer;
                filterModel.ToSource = "";
                filterModel.ToConnector = "";
                _dataModel.FilterDictionary[fromContainer] = filterModel;
                if (toOperator.Equals("target"))
                {
                    var oldTrgModel = _dataModel.TargetDictionary[toContainer];
                    var newTrgModel = new TargetModel();
                    newTrgModel.left = oldTrgModel.left;
                    newTrgModel.top = oldTrgModel.top;
                    newTrgModel.TargetName = oldTrgModel.TargetName;
                    _dataModel.TargetDictionary[toContainer] = newTrgModel;
                }
            }
            else if (fromOperator.Equals("expression"))
            {
                var ExpressionModel = _dataModel.ExpressionDictionary[fromContainer];
                //agrModel.FromSource = fromContainer;
                ExpressionModel.ToSource = "";
                ExpressionModel.toConnector = "";
                _dataModel.ExpressionDictionary[fromContainer] = ExpressionModel;
                if (toOperator.Equals("target"))
                {
                    var oldTrgModel = _dataModel.TargetDictionary[toContainer];
                    var newTrgModel = new TargetModel();
                    newTrgModel.left = oldTrgModel.left;
                    newTrgModel.top = oldTrgModel.top;
                    newTrgModel.TargetName = oldTrgModel.TargetName;
                    _dataModel.TargetDictionary[toContainer] = newTrgModel;
                }
            }

            return JsonConvert.SerializeObject(null);
        }

        public string DeleteNode(string operatorId)
        {
            string node = operatorId.Substring(0, operatorId.IndexOf('_'));
            if (node.Equals("source"))
            {
                var source = new SourceModel();
                source = _dataModel.SourceDictionary[operatorId];
                if (source.ConnectedTo != "")
                {
                    string linkedNode = source.ConnectedTo.Substring(0, source.ConnectedTo.IndexOf('_'));

                    if (linkedNode.Equals("aggregator"))
                    {
                        //delete source link
                        var oldArgModel = _dataModel.AggregatorDictionary[source.ConnectedTo];
                        var newAgrModel = new AggregatorModel();
                        newAgrModel.left = oldArgModel.left;
                        newAgrModel.top = oldArgModel.top;
                        newAgrModel.AggregatorName = oldArgModel.AggregatorName;
                        newAgrModel.toConnector = oldArgModel.toConnector;
                        newAgrModel.ToSource = oldArgModel.ToSource;
                        _dataModel.AggregatorDictionary[source.ConnectedTo] = newAgrModel;
                    }
                    else if (linkedNode.Equals("filter"))
                    {
                        //update source link
                        var oldFilterModel = _dataModel.FilterDictionary[source.ConnectedTo];
                        var newFilterModel = new FilterModel();
                        newFilterModel.left = oldFilterModel.left;
                        newFilterModel.top = oldFilterModel.top;
                        newFilterModel.FilterName = oldFilterModel.FilterName;
                        newFilterModel.ToConnector = oldFilterModel.ToConnector;
                        newFilterModel.ToSource = oldFilterModel.ToSource;
                        _dataModel.FilterDictionary[source.ConnectedTo] = newFilterModel;
                    }
                    else if (linkedNode.Equals("expression"))
                    {
                        //update source link
                        var oldExpressionModel = _dataModel.ExpressionDictionary[source.ConnectedTo];
                        var newExpressionModel = new ExpressionModel();
                        newExpressionModel.left = oldExpressionModel.left;
                        newExpressionModel.top = oldExpressionModel.top;
                        newExpressionModel.ExpressionName = oldExpressionModel.ExpressionName;
                        newExpressionModel.toConnector = oldExpressionModel.toConnector;
                        newExpressionModel.ToSource = oldExpressionModel.ToSource;
                        _dataModel.ExpressionDictionary[source.ConnectedTo] = newExpressionModel;
                    }
                    else if (linkedNode.Equals("joiner"))
                    {
                        //update source link
                        var oldJoinModel = _dataModel.JoinDictionary[source.ConnectedTo];
                        var newJoinModel = new JoinModel();
                        //inspect join model
                        if (source.toConnector == "Input_1")
                        {
                            //fill source 1
                            newJoinModel.JoinName = oldJoinModel.JoinName;
                            newJoinModel.SourceModel2 = oldJoinModel.SourceModel2;
                            newJoinModel.SourceModel2SelectedColumn = oldJoinModel.SourceModel2SelectedColumn;
                            newJoinModel.ToSource = oldJoinModel.ToSource;
                            newJoinModel.left = newJoinModel.left;
                            newJoinModel.top = newJoinModel.top;
                            newJoinModel.toConnector = newJoinModel.toConnector;
                            _dataModel.JoinDictionary[source.ConnectedTo] = newJoinModel;

                        }
                        else if (source.toConnector == "Input_2")
                        {
                            //fill source 2
                            newJoinModel.JoinName = oldJoinModel.JoinName;
                            newJoinModel.SourceModel1 = oldJoinModel.SourceModel2;
                            newJoinModel.SourceModel1SelectedColumn = oldJoinModel.SourceModel2SelectedColumn;
                            newJoinModel.ToSource = oldJoinModel.ToSource;
                            newJoinModel.left = newJoinModel.left;
                            newJoinModel.top = newJoinModel.top;
                            newJoinModel.toConnector = newJoinModel.toConnector;
                            _dataModel.JoinDictionary[source.ConnectedTo] = newJoinModel;
                        }

                    }
                    else if (linkedNode.Equals("target"))
                    {

                        var oldTrgModel = _dataModel.TargetDictionary[source.ConnectedTo];
                        var newTrgModel = new TargetModel();
                        newTrgModel.left = oldTrgModel.left;
                        newTrgModel.top = oldTrgModel.top;
                        newTrgModel.TargetName = oldTrgModel.TargetName;
                        _dataModel.TargetDictionary[source.ConnectedTo] = newTrgModel;
                    }

                    
                }
                _dataModel.SourceDictionary.Remove(operatorId);
            }
            else if (node.Equals("aggregator"))
            {
                var aggregator = new AggregatorModel();
                aggregator = _dataModel.AggregatorDictionary[operatorId];
                if (aggregator.ToSource != "")
                {
                    string linkedNode = aggregator.ToSource.Substring(0, aggregator.ToSource.IndexOf('_'));
                    if (linkedNode.Equals("target"))
                    {
                        var oldTrgModel = _dataModel.TargetDictionary[aggregator.ToSource];
                        var newTrgModel = new TargetModel();
                        newTrgModel.left = oldTrgModel.left;
                        newTrgModel.top = oldTrgModel.top;
                        newTrgModel.TargetName = oldTrgModel.TargetName;
                        _dataModel.TargetDictionary[aggregator.ToSource] = newTrgModel;
                    }
                }
                _dataModel.AggregatorDictionary.Remove(operatorId);
            }
            else if (node.Equals("joiner"))
            {
                var joiner = new JoinModel();
                joiner = _dataModel.JoinDictionary[operatorId];
                if (joiner.ToSource != "")
                {
                    string linkedNode = joiner.ToSource.Substring(0, joiner.ToSource.IndexOf('_'));
                    if (linkedNode.Equals("target"))
                    {
                        var oldTrgModel = _dataModel.TargetDictionary[joiner.ToSource];
                        var newTrgModel = new TargetModel();
                        newTrgModel.left = oldTrgModel.left;
                        newTrgModel.top = oldTrgModel.top;
                        newTrgModel.TargetName = oldTrgModel.TargetName;
                        _dataModel.TargetDictionary[joiner.ToSource] = newTrgModel;
                    }
                }
                _dataModel.JoinDictionary.Remove(operatorId);
            }
            else if (node.Equals("filter"))
            {
                var Filter = new FilterModel();
                Filter = _dataModel.FilterDictionary[operatorId];
                if (Filter.ToSource != "")
                {
                    string linkedNode = Filter.ToSource.Substring(0, Filter.ToSource.IndexOf('_'));
                    if (linkedNode.Equals("target"))
                    {
                        var oldTrgModel = _dataModel.TargetDictionary[Filter.ToSource];
                        var newTrgModel = new TargetModel();
                        newTrgModel.left = oldTrgModel.left;
                        newTrgModel.top = oldTrgModel.top;
                        newTrgModel.TargetName = oldTrgModel.TargetName;
                        _dataModel.TargetDictionary[Filter.ToSource] = newTrgModel;
                    }
                }
                _dataModel.FilterDictionary.Remove(operatorId);
            }
            else if (node.Equals("expression"))
            {
                var Expression = new ExpressionModel();
                Expression = _dataModel.ExpressionDictionary[operatorId];
                if (Expression.ToSource != "")
                {
                    string linkedNode = Expression.ToSource.Substring(0, Expression.ToSource.IndexOf('_'));
                    if (linkedNode.Equals("target"))
                    {
                        var oldTrgModel = _dataModel.TargetDictionary[Expression.ToSource];
                        var newTrgModel = new TargetModel();
                        newTrgModel.left = oldTrgModel.left;
                        newTrgModel.top = oldTrgModel.top;
                        newTrgModel.TargetName = oldTrgModel.TargetName;
                        _dataModel.TargetDictionary[Expression.ToSource] = newTrgModel;
                    }
                }
                _dataModel.ExpressionDictionary.Remove(operatorId);
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
            Task<string> task1 = SqlServerConnectionAsync(model);

            Task.WaitAll(new Task[] { task1 }); //synchronously wait

            return task1.Result;
        }

        public async Task<string> SqlServerConnectionAsync(SqlServerConnectionModel model)
        {
            DataModel dataModel = new DataModel();
            string connetionString = "";
            string dbName = "";
            if (model.ServerName != "" && model.DbName != "" && model.UserName != "" && model.Password != "")
            {

                connetionString = "Server=" + @model.ServerName + ";Initial Catalog=" + model.DbName +
                                    ";Persist Security Info=False;User ID = " + @model.UserName + ";Password=" + model.Password +
                                    ";MultipleActiveResultSets=False;Encrypt=True;TrustServerCertificate = True; Connection Timeout = 30;";
                //";Persist Security Info=False;User ID = " + model.UserName + ";Password=" + model.Password +
                //";MultipleActiveResultSets=False;Encrypt=True;TrustServerCertificate = True; Connection Timeout = 30;";

                //connetionString = @"Data Source=desktop-ig62959\PCUSER;Initial Catalog=ETLTest;Integrated Security=True";
                /*"Server=" + model.ServerName + ";Initial Catalog=" + model.DbName +
                                "Integrated Security=True";*/
                dbName = model.DbName;
                /*
                Server=tcp:etldemo1.database.windows.net,1433;Initial Catalog=etl;Persist Security Info=False;
                User ID={your_username};Password={your_password};MultipleActiveResultSets=False;Encrypt=True;
                TrustServerCertificate=False;Connection Timeout=30;        
                */
            }
            else
            {
                connetionString = _SolutionModel.getConnectionString();
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
                    sqlCommand.CommandTimeout=300;
                    SqlDataReader reader = await sqlCommand.ExecuteReaderAsync().ConfigureAwait(false);

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
            catch (Exception e)
            {
                string ret = "Failure";
                return JsonConvert.SerializeObject(ret);
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
        [HttpGet]
        public ActionResult JsTreeDemo()
        {
            return View();
        }
        public ActionResult Nodes()
         {
            JsTreeModel j = new JsTreeModel();
            var nodes = j.list(_SolutionModel);
            _SolutionModel.selected_project = -1;
            _SolutionModel.selected_mapping = -1;

            //SessionObjects.InitSession();

            return Json(nodes);
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

        #region Validate
        [HttpPost]
        public string Validate()
        {
            string retMsg = null;
            if(_dataModel.AggregatorDictionary.Count == 0 && _dataModel.TargetDictionary.Count == 0 && _dataModel.JoinDictionary.Count == 0
                && _dataModel.SourceDictionary.Count == 0)
            {
                retMsg = "No workflow found";
                return JsonConvert.SerializeObject(retMsg);
            }
            else
            {

                if(_dataModel.SourceDictionary.Count == 0)
                {
                    retMsg = retMsg + "No Sources";
                }
                if (_dataModel.TargetDictionary.Count == 0)
                {
                    if(retMsg != null)
                    retMsg = retMsg + ",Targets";
                    else
                        retMsg = "No Targets";
                }

                if(retMsg != null)
                {
                    retMsg = retMsg + " found. Invalid Workflow";
                    return JsonConvert.SerializeObject(retMsg);
                }
            }

            if(validate_sources() != null)
            {
                retMsg = "Source object is not "+validate_sources();
                return JsonConvert.SerializeObject(retMsg);
            }

            if(_dataModel.AggregatorDictionary.Count > 0)
            {
                if (validate_aggregator() != null)
                {
                    retMsg = "Aggregator is not " + validate_aggregator();
                    return JsonConvert.SerializeObject(retMsg);
                }

            }

            if (_dataModel.FilterDictionary.Count > 0)
            {
                if (validate_filter() != null)
                {
                    retMsg = "Filter is not " + validate_aggregator();
                    return JsonConvert.SerializeObject(retMsg);
                }

            }

            if (_dataModel.ExpressionDictionary.Count > 0)
            {
                if (validate_expression() != null)
                {
                    retMsg = "Expression is not " + validate_expression();
                    return JsonConvert.SerializeObject(retMsg);
                }

            }

            if (_dataModel.JoinDictionary.Count > 0)
            {
                if (validate_joiner() != null)
                {
                    retMsg = "Joiner is not " + validate_joiner();
                    return JsonConvert.SerializeObject(retMsg);
                }

            }
            return JsonConvert.SerializeObject(retMsg);
        }


        public string validate_sources()
        {
            foreach (var source in _dataModel.SourceDictionary)
            {
                SourceModel s = new SourceModel();
                s = source.Value;
                if(s.ConnectedTo == "")
                {
                    return "connected to any node.";
                }
                if(s.ConnectionName == "")
                {
                    return "connected to any database.";
                }
                if(s.TableName == "")
                {
                    return "connected to any table";
                }
            }

            return null;
        }


        public string validate_aggregator()
        {
            foreach (var aggregator in _dataModel.AggregatorDictionary)
            {
                AggregatorModel a = new AggregatorModel();
                a = aggregator.Value;
                if (a.FromSource == "")
                {
                    return "connected to any source.";
                }
                if (a.ToSource == "")
                {
                    return "connected to any target.";
                }
                if (a.SourceModel == null)
                {
                    return "connected to any source.";
                }
            }

            return null;
        }


        public string validate_filter()
        {
            foreach (var filter in _dataModel.FilterDictionary)
            {
                FilterModel a = new FilterModel();
                a = filter.Value;
                if (a.FromSource == "")
                {
                    return "connected to any source.";
                }
                if (a.ToSource == "")
                {
                    return "connected to any target.";
                }
                if (a.SourceModel == null)
                {
                    return "connected to any source.";
                }
            }

            return null;
        }


        public string validate_expression()
        {
            foreach (var expression in _dataModel.ExpressionDictionary)
            {
                ExpressionModel e = new ExpressionModel();
                e = expression.Value;
                if (e.FromSource == "")
                {
                    return "connected to any source.";
                }
                if (e.ToSource == "")
                {
                    return "connected to any target.";
                }
                if (e.SourceModel == null)
                {
                    return "connected to any source.";
                }
            }

            return null;
        }


        public string validate_joiner()
        {
            foreach (var joiner in _dataModel.JoinDictionary)
            {
                JoinModel j = new JoinModel();
                j = joiner.Value;
                if (j.SourceModel1.SourceName == "")
                {
                    return "connected to 1st source.";
                }
                if (j.SourceModel2.SourceName == "")
                {
                    return "connected to 2nd source.";
                }
                if (j.ToSource == "")
                {
                    return "connected to any target.";
                }
                if (j.SourceModel1SelectedColumn == "")
                {
                    return "having 1st column selected for join condition.";
                }
                if (j.SourceModel2SelectedColumn == "")
                {
                    return "having 2nd column selected for join condition.";
                }
            }

            return null;
        }
        #endregion Validate


        #region save
        [HttpPost]
        public string Save([FromBody] OperatorModel[] model)
        {
            string retMsg = null;
            if (_dataModel.AggregatorDictionary.Count == 0 && _dataModel.TargetDictionary.Count == 0 && _dataModel.JoinDictionary.Count == 0
                && _dataModel.SourceDictionary.Count == 0 && _dataModel.FilterDictionary.Count == 0 && _dataModel.ExpressionDictionary.Count == 0)
            {
                retMsg = "No mapping found to save";
                return JsonConvert.SerializeObject(retMsg);
            }
            int i = 0;

            while(i < model.Length)
            {
                if(_dataModel.SourceDictionary.ContainsKey(model[i].OperatorId))
                {

                    _dataModel.SourceDictionary[model[i].OperatorId].top = model[i].top;
                    _dataModel.SourceDictionary[model[i].OperatorId].left = model[i].left;
                }
                i++;
            }

            i = 0;

            while (i < model.Length)
            {
                if (_dataModel.AggregatorDictionary.ContainsKey(model[i].OperatorId))
                {

                    _dataModel.AggregatorDictionary[model[i].OperatorId].top = model[i].top;
                    _dataModel.AggregatorDictionary[model[i].OperatorId].left = model[i].left;
                }
                i++;
            }

            i = 0;

            while (i < model.Length)
            {
                if (_dataModel.JoinDictionary.ContainsKey(model[i].OperatorId))
                {

                    _dataModel.JoinDictionary[model[i].OperatorId].top = model[i].top;
                    _dataModel.JoinDictionary[model[i].OperatorId].left = model[i].left;
                }
                i++;
            }
            i = 0;
            while (i < model.Length)
            {
                if (_dataModel.FilterDictionary.ContainsKey(model[i].OperatorId))
                {

                    _dataModel.FilterDictionary[model[i].OperatorId].top = model[i].top;
                    _dataModel.FilterDictionary[model[i].OperatorId].left = model[i].left;
                }
                i++;
            }
            i = 0;

            while (i < model.Length)
            {
                if (_dataModel.ExpressionDictionary.ContainsKey(model[i].OperatorId))
                {

                    _dataModel.ExpressionDictionary[model[i].OperatorId].top = model[i].top;
                    _dataModel.ExpressionDictionary[model[i].OperatorId].left = model[i].left;
                }
                i++;
            }
            i = 0;

            while (i < model.Length)
            {
                if (_dataModel.TargetDictionary.ContainsKey(model[i].OperatorId))
                {

                    _dataModel.TargetDictionary[model[i].OperatorId].top = model[i].top;
                    _dataModel.TargetDictionary[model[i].OperatorId].left = model[i].left;
                }
                i++;
            }

            if (_SolutionModel.selected_mapping == -1)
            {

                retMsg = "Please select any mapping from Explorer to Save";
                return JsonConvert.SerializeObject(retMsg);
            }


            object mapping = _dataModel;

            MemoryStream memStream = new MemoryStream();
            BinaryFormatter b = new BinaryFormatter();
            b.Serialize(memStream, mapping);
            //StreamWriter sw = new StreamWriter(memStream);
            //sw.Write(mapping);
            string connetionString = _SolutionModel.getConnectionString();


            SqlConnection connection = new SqlConnection(connetionString);
            try
            {
                connection.Open();
                if (connection != null && connection.State == ConnectionState.Open)
                {
                    SqlCommand sqlCmd = new SqlCommand("update dbo.project_mappings set end_date = @enddate where Id = @Id and Project_Id=@project", connection);

                    sqlCmd.Parameters.Add("@id", SqlDbType.Int);
                    sqlCmd.Parameters["@id"].Value = _SolutionModel.selected_mapping;
                    sqlCmd.Parameters.Add("@project", SqlDbType.Int);
                    sqlCmd.Parameters["@project"].Value = _SolutionModel.selected_project;

                    sqlCmd.Parameters.Add("@enddate", SqlDbType.Date);
                    sqlCmd.Parameters["@enddate"].Value = DateTime.Now;

                    sqlCmd.ExecuteNonQuery();


                    sqlCmd = new SqlCommand("INSERT INTO project_mappings(Project_Id,mapping_name,mapping_object,start_date,len) VALUES (@Project,@mapping_name,@mapping_object,@start_date,@len)", connection);

                    sqlCmd.Parameters.Add("@Project", SqlDbType.Int);
                    sqlCmd.Parameters["@Project"].Value = _SolutionModel.selected_project;

                    string mapping_name = null ;
                    DataModel dm = new DataModel();
                    foreach(var p in _SolutionModel.projects )
                    {
                        if (p.Key == _SolutionModel.selected_project)
                        {

                            foreach(var m in p.Value.mappings)
                            {

                                if(m.Key == _SolutionModel.selected_mapping)
                                {

                                    mapping_name= m.Value.name;
                                    break;
                                }
                            }
                            break;
                        }

                    }


                    sqlCmd.Parameters.Add("@mapping_name", SqlDbType.VarChar);
                    sqlCmd.Parameters["@mapping_name"].Value = mapping_name;

                    var dateAndTime = DateTime.Now;
                    sqlCmd.Parameters.Add("@start_date", SqlDbType.Date);
                    sqlCmd.Parameters["@start_date"].Value = DateTime.Now;

                    sqlCmd.Parameters.Add("@mapping_object", SqlDbType.VarBinary, Int32.MaxValue);

                    sqlCmd.Parameters["@mapping_object"].Value = memStream.GetBuffer();

                    sqlCmd.Parameters.Add("@len", SqlDbType.Int, Int32.MaxValue);

                    sqlCmd.Parameters["@len"].Value = memStream.Length;

                    sqlCmd.ExecuteNonQuery();
                }
            }
            catch (Exception e)
            {
                retMsg = e.Message;
                return JsonConvert.SerializeObject(retMsg);
            }
            finally
            {
                // Always call Close when done reading.
                //reader.Close();
                connection.Close();
            }


            if(retMsg == null)
            {
                retMsg = "mapping saved";
            }
            return JsonConvert.SerializeObject(retMsg);


        }
        #endregion save

        #region Create Project
        [HttpPost]
        public string CreateProject([FromBody] ProjectModel model)
        {
            string retMsg = null;
            JsTreeModel j;
            JstreeOutputmodel jout = new JstreeOutputmodel();
            if (model.name.Trim() == "")
            {
                TransactionLog(0, 0, "Project", "Project name cannot be empty");
                j = new JsTreeModel();
                jout.nodes = j.list(_SolutionModel);
                jout.message = "Project name cannot be empty";

                return JsonConvert.SerializeObject(jout);
            }

            string connetionString = _SolutionModel.getConnectionString();
            SqlConnection connection = new SqlConnection(connetionString);
            SqlConnection connection1 = new SqlConnection(connetionString);
            try
            {
                connection.Open();
                if (connection != null && connection.State == ConnectionState.Open)
                {

                    string tableColumnsQuery = @"Select Name from ETL.dbo.Projects where Name = @projectName;";
                    //select mapping_object,len from dbo.project_mappings where Project_Id = @project and Id=@id and end_date is null;";
                    SqlCommand sqlCommand = new SqlCommand(tableColumnsQuery, connection);
                    sqlCommand.Parameters.Add("@projectName", SqlDbType.VarChar);
                    sqlCommand.Parameters["@projectName"].Value = model.name.Trim();
                    SqlDataReader reader;
                    reader = sqlCommand.ExecuteReader();
                    if (reader.HasRows)
                    {
                        TransactionLog(0, 0, "Project", model.name + " is already exists");
                        j = new JsTreeModel();
                        jout.nodes = j.list(_SolutionModel);
                        jout.message = model.name + " is already exists";

                        return JsonConvert.SerializeObject(jout);

                    }

                    try
                    {
                        connection1.Open();
                        tableColumnsQuery = @"Insert into ETL.dbo.Projects(Name, Start_Date) Values(@projectName, @StartDate);";
                        //select mapping_object,len from dbo.project_mappings where Project_Id = @project and Id=@id and end_date is null;";
                        sqlCommand = new SqlCommand(tableColumnsQuery, connection1);
                        sqlCommand.Parameters.Add("@projectName", SqlDbType.VarChar);
                        sqlCommand.Parameters["@projectName"].Value = model.name;
                        sqlCommand.Parameters.Add("@StartDate", SqlDbType.Date);
                        sqlCommand.Parameters["@StartDate"].Value = DateTime.Now;
                        //sqlCommand.Parameters.Add("@EndDate", SqlDbType.Date);
                        //sqlCommand.Parameters["@EndDate"].Value = null;

                        int ret = sqlCommand.ExecuteNonQuery();
                    }
                    catch (Exception e)
                    {
                        TransactionLog(0, 0, "Project", e.Message);
                        j = new JsTreeModel();
                        jout.nodes = j.list(_SolutionModel);
                        jout.message = e.Message;

                        return JsonConvert.SerializeObject(jout);
                    }
                    finally
                    {
                        // Always call Close when done reading.
                        //reader.Close();
                        connection1.Close();
                    }
                    TransactionLog(0, 0, "Project Created", model.name + " is created");
                    j = new JsTreeModel();
                    jout.nodes = j.list(_SolutionModel);
                    jout.message = "Project Created.";

                    return JsonConvert.SerializeObject(jout);
                }
            }
            catch (Exception e)
            {
                TransactionLog(0, 0, "Project", e.Message);
                j = new JsTreeModel();
                jout.nodes = j.list(_SolutionModel);
                jout.message = e.Message;
            }
            finally
            {
                // Always call Close when done reading.
                //reader.Close();
                connection.Close();
            }
            return JsonConvert.SerializeObject(retMsg);
        }

        public void TransactionLog(int projectId, int mappingId, string transType, string transMessage)
        {
            string connetionString = _SolutionModel.getConnectionString();
            SqlConnection connection = new SqlConnection(connetionString);
            try
            {
                connection.Open();
                if (connection != null && connection.State == ConnectionState.Open)
                {
                    string tableColumnsQuery = @"Insert into ETL.dbo.TransactionLog
                        (ProjectId, MappingId, TransType,TransMessage, TimeStamp) 
                        Values(@ProjectId, @MappingId, @TransType, @TransMessage, @TransMessage, @TimeStamp);";
                    //select mapping_object,len from dbo.project_mappings where Project_Id = @project and Id=@id and end_date is null;";
                    SqlCommand sqlCommand = new SqlCommand(tableColumnsQuery, connection);
                    sqlCommand.Parameters.Add("@projectId", SqlDbType.Int);
                    sqlCommand.Parameters["@projectId"].Value = projectId;//ProjectId
                    sqlCommand.Parameters.Add("@MappingId", SqlDbType.Int);
                    sqlCommand.Parameters["@MappingId"].Value = mappingId;
                    sqlCommand.Parameters.Add("@TransType", SqlDbType.VarChar);
                    sqlCommand.Parameters["@TransType"].Value = transType;
                    sqlCommand.Parameters.Add("@TransMessage", SqlDbType.VarChar);
                    sqlCommand.Parameters["@TransMessage"].Value = transMessage;
                    sqlCommand.Parameters.Add("@TimeStamp", SqlDbType.Date);
                    sqlCommand.Parameters["@TimeStamp"].Value = DateTime.Now;
                    //sqlCommand.Parameters.Add("@EndDate", SqlDbType.Date);
                    //sqlCommand.Parameters["@EndDate"].Value = null;

                    int ret = sqlCommand.ExecuteNonQuery();
                }
            }
            catch (Exception e)
            {

            }
        }
        [HttpPost]
        public string CreateMapping([FromBody] MappingModel model)
        {
            string retMsg = null;
            JsTreeModel j;
            JstreeOutputmodel jout = new JstreeOutputmodel();
            if (model.MappingName.Trim() == "")
            {
                TransactionLog(0, 0, "Project", "Mapping name cannot be empty");
                j = new JsTreeModel();
                jout.nodes = j.list(_SolutionModel);
                jout.message = "Mapping name cannot be empty";

                return JsonConvert.SerializeObject(jout);
            }
            string connetionString = _SolutionModel.getConnectionString();
            SqlConnection connection = new SqlConnection(connetionString);
            SqlConnection connection1 = new SqlConnection(connetionString);
            try
            {
                connection.Open();
                connection1.Open();
                if (connection != null && connection.State == ConnectionState.Open)
                {
                    string tableColumnsQuery = @"Select Mapping_Name from ETL.dbo.Project_Mappings where Project_Id = @projectId and Mapping_Name= @mappingName;";
                    //select mapping_object,len from dbo.project_mappings where Project_Id = @project and Id=@id and end_date is null;";
                    SqlCommand sqlCommand = new SqlCommand(tableColumnsQuery, connection);
                    sqlCommand.Parameters.Add("@projectId", SqlDbType.Int);
                    sqlCommand.Parameters["@projectId"].Value = model.ProjectId;//ProjectId
                    sqlCommand.Parameters.Add("@mappingName", SqlDbType.VarChar);
                    sqlCommand.Parameters["@mappingName"].Value = model.MappingName;
                    SqlDataReader reader;
                    reader = sqlCommand.ExecuteReader();
                    if (reader.HasRows)
                    {
                        TransactionLog(0, 0, "Project", model.MappingName + " is already exists");
                        j = new JsTreeModel();
                        jout.nodes = j.list(_SolutionModel);
                        jout.message = model.MappingName + " is already exists";

                        return JsonConvert.SerializeObject(jout);

                    }

                    try
                    {
                        tableColumnsQuery = @"Insert into ETL.dbo.Project_Mappings
                        (Project_Id, Mapping_Name, Start_Date,Len) Values(@projectId, @mappingName, @StartDate, 0);";
                        //select mapping_object,len from dbo.project_mappings where Project_Id = @project and Id=@id and end_date is null;";
                        sqlCommand = new SqlCommand(tableColumnsQuery, connection1);
                        sqlCommand.Parameters.Add("@projectId", SqlDbType.Int);
                        sqlCommand.Parameters["@projectId"].Value = model.ProjectId;//ProjectId
                        sqlCommand.Parameters.Add("@mappingName", SqlDbType.VarChar);
                        sqlCommand.Parameters["@mappingName"].Value = model.MappingName;
                        sqlCommand.Parameters.Add("@StartDate", SqlDbType.Date);
                        sqlCommand.Parameters["@StartDate"].Value = DateTime.Now;
                        //sqlCommand.Parameters.Add("@EndDate", SqlDbType.Date);
                        //sqlCommand.Parameters["@EndDate"].Value = null;

                        int ret = sqlCommand.ExecuteNonQuery();
                    }
                    catch (Exception e)
                    {
                        TransactionLog(0, 0, "Project", e.Message);
                        j = new JsTreeModel();
                        jout.nodes = j.list(_SolutionModel);
                        jout.message = e.Message;

                        return JsonConvert.SerializeObject(jout);
                    }
                    finally
                    {
                        connection1.Close();

                    }
                    TransactionLog(0, 0, "Project", "mapping created.");
                    j = new JsTreeModel();
                    jout.nodes = j.list(_SolutionModel);
                    jout.message = "mapping created.";

                    return JsonConvert.SerializeObject(jout);
                }
            }
            catch (Exception e)
            {
                TransactionLog(0, 0, "Project", e.Message);
                j = new JsTreeModel();
                jout.nodes = j.list(_SolutionModel);
                jout.message = e.Message;

                return JsonConvert.SerializeObject(jout);
            }
            finally
            {
                connection.Close();
            }
            return JsonConvert.SerializeObject(retMsg);
        }

        public string GetProjectMappingDetail()
        {
            object obj = new
            {
                Project = _SolutionModel.selected_project,
                Mapping = _SolutionModel.selected_mapping
            };

            return JsonConvert.SerializeObject(obj);
        }
        #endregion

        #region load
        [HttpPost]
        public string Load([FromBody] JsTreeModel model)
        {
            string retMsg = null;
            _dataModel = new DataModel();
            if (model.id == null || model.parent == "#")
            {
                return JsonConvert.SerializeObject(retMsg);
            }
            _SolutionModel.selected_mapping = Convert.ToInt32(model.id.Substring(0, model.id.Length - "_c".Length));
            _SolutionModel.selected_project = Convert.ToInt32(model.parent);
            _dataModel.Id = _SolutionModel.selected_mapping;
            _dataModel.name = model.text;



            MemoryStream memStream = new MemoryStream();
            BinaryFormatter b = new BinaryFormatter();
         //   b.Serialize(memStream, mapping);
            //StreamWriter sw = new StreamWriter(memStream);
            //sw.Write(mapping);
            string connetionString = _SolutionModel.getConnectionString();


            SqlConnection connection = new SqlConnection(connetionString);
            try
            {
                connection.Open();
                if (connection != null && connection.State == ConnectionState.Open)
                {
                    string tableColumnsQuery = @"select mapping_object,len from dbo.project_mappings where Project_Id = @project and Id=@id and end_date is null;";
                    SqlCommand sqlCommand = new SqlCommand(tableColumnsQuery, connection);

                    sqlCommand.Parameters.Add("@id", SqlDbType.Int);
                    sqlCommand.Parameters["@id"].Value = _SolutionModel.selected_mapping;
                    sqlCommand.Parameters.Add("@project", SqlDbType.Int);
                    sqlCommand.Parameters["@project"].Value= _SolutionModel.selected_project;


                    SqlDataReader reader = sqlCommand.ExecuteReader();
                    try
                    {
                        while (reader.Read())
                        {
                            byte[] cc = new byte[(int)reader["len"]];
                            reader.GetBytes(0, 0, cc, 0, (int)reader["len"]);
                            MemoryStream mss = new MemoryStream(cc);
                            _dataModel = (DataModel)b.Deserialize(mss);
                        }
                    }
                    catch (Exception e)
                    {
                        retMsg = e.Message;
                        return JsonConvert.SerializeObject(retMsg);
                    }
                    finally
                    {
                        // Always call Close when done reading.
                        reader.Close();
                        connection.Close();
                    }
                }
            }
            catch (Exception e)
            {
                retMsg = e.Message;
                return JsonConvert.SerializeObject(retMsg);
            }
            finally
            {
                // Always call Close when done reading.
                //reader.Close();
                connection.Close();
            }

            return JsonConvert.SerializeObject(_dataModel);
           


        }
        #endregion load
    }
}
