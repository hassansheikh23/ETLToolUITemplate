using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ETL.Models
{
    public class SolutionDataModel
    {
        public Dictionary<int, ProjectModel> projects { get; set; }
        public int selected_project { get; set; }

        public int selected_mapping { get; set; }

        //--------Data base Connection
        private string connectionString;
        private string current_mode;
        //--------Data base Connection
        public SolutionDataModel()
        {
            projects = new Dictionary<int, ProjectModel>();
            selected_project = -1;
            selected_mapping = -1;

            current_mode = "AWS";//AWS - 
            connectionString = "";
            setConnectionString();
        }
        public int ToJulianDate(DateTime date)
        {
            Random random = new Random();
            int x = random.Next(300, 900);
            int y = random.Next(0, 299);
            int t = (x * y) * (x+y)/ x;
            return Convert.ToInt32(date.ToOADate() + 2415018.5 + t);
        }
        public void setConnectionString()
        {
            if (current_mode.Equals("Development"))
            {
                connectionString = @"Data Source=desktop-ig62959\PCUSER;Initial Catalog=ETL;Integrated Security=True";
            }
            else if (current_mode.Equals("AWS"))
            {
                connectionString = @"Server=" + "etl.cugvdwvlad5i.us-west-2.rds.amazonaws.com" + ";Initial Catalog=" + "etl" +
                                    ";Persist Security Info=False;User ID = " + "hassan" + ";Password=" + "10i-0113" +
                                    ";MultipleActiveResultSets=False;Encrypt=True;TrustServerCertificate = True; Connection Timeout = 30;";
            }
        }

        public string getConnectionString()
        {
            return connectionString;
        }
    }
}
