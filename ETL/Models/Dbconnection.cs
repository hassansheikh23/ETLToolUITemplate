//using Microsoft.Office.Interop.Excel;
using Syncfusion.XlsIO;
using System.IO;
using Syncfusion.Drawing;
using Microsoft.AspNetCore.Hosting;
using System.Data;
using System;

namespace ETL.Models
{
    [Serializable]
    public class Dbconnection
    {
        public string filename { get; set; }
        public string msg { get; set; }
       // public string ;
        public void getColumns()
        {
            IHostingEnvironment _hostingEnvironment = null;

            //New instance of ExcelEngine is created 
            //Equivalent to launching Microsoft Excel with no workbooks open
            //Instantiate the spreadsheet creation engine
            ExcelEngine excelEngine = new ExcelEngine();

            //Instantiate the Excel application object
            IApplication application = excelEngine.Excel;

            //Assigns default application version
            application.DefaultVersion = ExcelVersion.Excel2016;

            //A existing workbook is opened.              
            string basePath = "C:\\Users\\Hassan\\source\\repos\\ETL\\ETL\\wwwroot\\lib\\1.xlsx";

            try
            {
                FileStream sampleFile = new FileStream(filename, FileMode.Open);

                IWorkbook workbook = application.Workbooks.Open(sampleFile);

                //Access first worksheet from the workbook.
                IWorksheet worksheet = workbook.Worksheets[0];


                DataTable customersTable = worksheet.ExportDataTable(worksheet.UsedRange, ExcelExportDataTableOptions.ColumnNames);
            //   customersTable.Columns.Count
                msg = "file processed";
            }
            catch
            {
                msg = "file not processed";
            }
            //Set Text in cell A3.
           

          
        }


     
    }
}
