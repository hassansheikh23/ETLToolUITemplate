using ETL.Controllers;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Threading.Tasks;

namespace ETL.Models
{
    public class JsTreeModel
    {
        public string id { get; set; }
        public string parent { get; set; }
        public string text { get; set; }
        public string icon { get; set; }
        public string state { get; set; }
        public bool opened { get; set; }
        public bool disabled { get; set; }
        public bool selected { get; set; }
        public string li_attr { get; set; }
        public string a_attr { get; set; }


        public List<JsTreeModel> list(SolutionDataModel _SolutionModel)
        {

            var nodes = new List<JsTreeModel>();
            _SolutionModel.projects = new Dictionary<int, ProjectModel>();

            
            string connetionString = _SolutionModel.getConnectionString();


            SqlConnection connection = new SqlConnection(connetionString);
            try
            {
                connection.Open();
                if (connection != null && connection.State == ConnectionState.Open)
                {
                    string tableColumnsQuery = @"select * from dbo.projects where end_date is null;";
                    SqlCommand sqlCommand = new SqlCommand(tableColumnsQuery, connection);




                    SqlDataReader reader = sqlCommand.ExecuteReader();
                    try
                    {
                        while (reader.Read())
                        {
                            ProjectModel pm = new ProjectModel();
                            Dictionary<string, DataModel> list = new Dictionary<string, DataModel>();

                            int parent_id = (int)reader["Id"];
                            string project_name = (string)reader["Name"];
                            nodes.Add(new JsTreeModel() { id = parent_id.ToString(), parent = "#", text = project_name });
                            pm.id = parent_id;
                            pm.name = project_name;

                            SqlConnection connection1 = new SqlConnection(connetionString);
                            try
                            {
                                connection1.Open();
                                if (connection1 != null && connection1.State == ConnectionState.Open)
                                {
                                    SqlCommand sqlCommand1 = new SqlCommand(tableColumnsQuery, connection1);
                                    tableColumnsQuery = @"select * from dbo.project_mappings where Project_Id =@pro_id and end_date is null;";
                                    sqlCommand1 = new SqlCommand(tableColumnsQuery, connection1);
                                    sqlCommand1.Parameters.Add("@pro_id", SqlDbType.Int);
                                    sqlCommand1.Parameters["@pro_id"].Value = parent_id;
                                    SqlDataReader childReader = sqlCommand1.ExecuteReader();
                                    try
                                    {
                                        DataModel dm = new DataModel();
                                        while (childReader.Read())
                                        {

                                            int child_id = (int)childReader["Id"];
                                            string mapping_name = (string)childReader["mapping_name"];
                                            nodes.Add(new JsTreeModel() { id = child_id.ToString() + "_c", parent = parent_id.ToString(), text = mapping_name });

                                            dm.Id = child_id;
                                            dm.name = mapping_name;

                                            pm.mappings.Add(child_id, dm);

                                        }


                                    }
                                    catch (Exception e)
                                    {
                                        return (nodes);
                                    }
                                    finally
                                    {
                                        // Always call Close when done reading.
                                        childReader.Close();

                                        connection1.Close();
                                    }
                                }
                            }
                            catch (Exception e)
                            {
                                return (nodes);

                            }
                            finally
                            {
                                // Always call Close when done reading.


                                connection1.Close();
                            }

                            _SolutionModel.projects.Add(parent_id, pm);
                        }
                    }
                    catch (Exception e)
                    {
                        return (nodes);
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
                return (nodes);
            }
            finally
            {
                // Always call Close when done reading.
                //reader.Close();
                connection.Close();
            }

            return (nodes);
        }
    }
}
