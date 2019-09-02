using ALE.ETLBox;
using ALE.ETLBox.ConnectionManager;
using ALE.ETLBox.ControlFlow;
using ALE.ETLBox.DataFlow;
using System;
using System.Collections.Generic;
using System.Linq;
//using System.Linq.Dynamic;
using System.Threading.Tasks;

namespace ETL.Models
{
    public class ETLpipelineModel
    {
        public DataModel datamodel { get; set; }

        public ETLpipelineModel()
        {
            datamodel = new DataModel();
        }

        public void createPassthrough()
        {
            foreach (var source in datamodel.SourceDictionary)
            {
                SourceModel s = source.Value;
                var fields = new List<Field>();
                DynamicProperty<object> complexProperty = new ComplexProperty<object>();
                foreach (var col in s.InputModel)
                {
                    //if (col.toDataType == "")
                    //{
                    //    var f = new Field(col.ColumnName, typeof(string));
                    //    fields.Add(f);
                    //}

                    var f = new Field(col.ColumnName, typeof(string));
                        fields.Add(f);
                    complexProperty.AddProperty(col.ColumnName, new SimpleProperty<string>(String.Empty));
                    complexProperty.GetValue<string>("Id");
                }

                
                
                ControlFlow.CurrentDbConnection = new SqlConnectionManager(datamodel.ConnectionString[s.ConnectionName]);
                DBSource source1 = new DBSource("dbo." + s.TableName);
                source1.ReadAll();
                TableDefinition st = source1.SourceTableDefinition;
         //       source1.SourceBlock.
                DBDestination<List<string>> dest = new DBDestination<List<string>>("dbo." + s.TableName);
                //RowTransformation<DBSource, List<string>> rowTrans = new RowTransformation< DBSource, List<string>> (
                //        myRow =>
                //        {
                //              return myRow.ColumnNamesEvaluated;
                                
                //        }

                //    );
                //source1.LinkTo(rowTrans);

               // rowTrans.LinkTo(dest);
                source1.Execute();
                dest.Wait();



                //DBDestination dest = new DBDestination("dbo." + s.TableName);
                // source1.LinkTo(rowTrans);

            }
        }
    }
}
