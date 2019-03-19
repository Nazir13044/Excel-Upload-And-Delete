using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Windows.Forms;
using System.Windows.Threading;
using Microsoft.Office.Interop.Excel;


namespace ExcelUploader
{
    public partial class Form1 : Form
    {
        //public ExcelUploadDBEntities _excelUploadDbEntities;
        public RBSYNERGYTESTEntities _RbsynergytestEntities;
        //public RBSYNERGYEntities _RbsynergyEntities;
        public Form1()
        {
            //_excelUploadDbEntities=new ExcelUploadDBEntities();
            _RbsynergytestEntities = new RBSYNERGYTESTEntities();
            //_RbsynergyEntities=new RBSYNERGYEntities();


        
            InitializeComponent();
        }

        public void ReadExcel(string filepath, string fileExt)
        {
         
      
            var xlApp = new Microsoft.Office.Interop.Excel.Application();
            Workbook xWorkbook = xlApp.Workbooks.Open(filepath);
            _Worksheet xSheet = xWorkbook.Sheets[1];
            Range xRange = xSheet.UsedRange;
            int countRow = xRange.Rows.Count;
            progressBar.Maximum = countRow - 1;


            for (int i = 2; i <= countRow; i++)
            {

                string number = (xRange.Cells[i, 1].Value2) == null ? "" : (xRange.Cells[i, 1].Value2).ToString();
                string model = (xRange.Cells[i, 2].Value2) == null ? "" : (xRange.Cells[i, 2].Value2).ToString();
                string imei = (xRange.Cells[i, 3].Value2) == null ? "" : (xRange.Cells[i, 3].Value2).ToString();
                string color = (xRange.Cells[i, 4].Value2) == null ? "" : (xRange.Cells[i, 4].Value2).ToString();

                //_RbsynergytestEntities.Database.Connection.Open();
                tblDealerDistributionDetail check = _RbsynergytestEntities.tblDealerDistributionDetails.FirstOrDefault(x => x.BarCode == imei);

                if (check != null)
                {
                    
                    var up = new tblDealerDetail
                    {
                                         
                        DealerCode = check.DealerCode,
                        DONumber=check.DONumber,
                        DistributionDate = check.DistributionDate,
                        Model = model,                      
                        IMEI = imei,                                           
                        IsSoldOut = check.IsSoldOut
                    };
                 
                    _RbsynergytestEntities.tblDealerDetails.Add(up);
                    _RbsynergytestEntities.SaveChanges();

                    //var s = new List<tblDealerDistributionDetail>();
                    //var s = new ArrayList();
                    //s.Add(check);

                    //_RbsynergytestEntities.Entry(typeof (tblDealerDistributionDetail)).State = System.Data.Entity.EntityState.Deleted;
                    _RbsynergytestEntities.tblDealerDistributionDetails.Remove(check);
                    _RbsynergytestEntities.SaveChanges();

                    //progressBar.Value = progressBar.Value + 1;
                    //progressBar.Dispatcher.Invoke(() => progressBar.Value = progressBar.Value, DispatcherPriority.Background);
                }
            }
        }



        private void btnBrowse_Click(object sender, EventArgs e)
        {
            var file = new OpenFileDialog(); //open dialog to choose file 
            if (file.ShowDialog() == DialogResult.OK) //if there is a file choosen by the user  
            {
                string filePath = file.FileName;
                string fileExt = Path.GetExtension(filePath);
                if (fileExt.CompareTo(".xls") == 0 || fileExt.CompareTo(".xlsx") == 0)
                {
                    try
                    {
                        textBox.Text = filePath;
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show(ex.Message);
                    }
                }
                else
                {
                    MessageBox.Show("Please choose .xls or .xlsx file only.", "Warning", MessageBoxButtons.OK, MessageBoxIcon.Error); //custom messageBox to show error  
                    textBox.Text = "";
                }
            }
        }

        private void btnUpload_Click(object sender, EventArgs e)
        {
            string filePath = textBox.Text;
            if (!string.IsNullOrWhiteSpace(filePath))
            {
                string fileExt = Path.GetExtension(filePath); //get the file extension
                ReadExcel(filePath, fileExt); //read excel file
                progressBar.Value = 0;
                textBox.Text = string.Empty;
             
            }
            else
            {
                   MessageBox.Show("Please select a file to upload", "Warning", MessageBoxButtons.OK, MessageBoxIcon.Error); //custom messageBox to show error  
            }
        }
    }
}
