using System;
using System.Data.Entity.Migrations;
using System.Linq;
using System.Windows;
using System.Windows.Forms;
using System.IO;
using System.Windows.Forms.VisualStyles;
using System.Windows.Threading;
using Microsoft.Office.Interop.Excel;
using Window = System.Windows.Window;
using MessageBox = System.Windows.MessageBox;

namespace BomUploader
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private CellPhoneProjectEntities _cellPhoneProjectEntities;
        private WMRP_ProductionEntities _wmrpProductionEntities;
        public MainWindow()
        {
            _cellPhoneProjectEntities = new CellPhoneProjectEntities();
            _wmrpProductionEntities = new WMRP_ProductionEntities();
            InitializeComponent();
            ProjectComboBox.Items.Add("Select");
            ProjectComboBox.SelectedIndex = 0;
            ProjectComboBox_Load();
        }

        public void ReadExcel(string filepath, string fileExt)
        {
            var parentId = "";
            var flag = 0;
            var xlApp = new Microsoft.Office.Interop.Excel.Application();
            Workbook xWorkbook = xlApp.Workbooks.Open(filepath);
            _Worksheet xSheet = xWorkbook.Sheets[1];
            Range xRange = xSheet.UsedRange;
            int countRow = xRange.Rows.Count;
            int countColumn = xRange.Columns.Count;
            progressBar1.Maximum = countRow - 1;
            // set maximum value for progressbar
            DialogResult messageBoxResult = System.Windows.Forms.MessageBox.Show("Are you sure? It may take approximately " + Math.Round(((countRow - 1) * .827)) + " seconds to upload " + (countRow - 1) + " rows of data.", "Delete Confirmation", MessageBoxButtons.YesNo);
            if (messageBoxResult == System.Windows.Forms.DialogResult.Yes)
            {
                var model = "Primo G8i";//ProjectComboBox.SelectedItem.ToString();//Model Name
                for (int i = 4; i <= countRow; i++)//i=3 means how many starting excel row we have to skip
                {
                    //string item = (xRange.Cells[i, 1].Value2).ToString();//Item
                    string ywxPartNo = (xRange.Cells[i, 2].Value2) = (xRange.Cells[i, 2].Value2).ToString();//Ywx Part No
                    string name = (xRange.Cells[i, 4].Value2) == null ? "" : (xRange.Cells[i, 4].Value2).ToString();//Name
                    if (name == "")
                    {
                        flag = flag + 1;
                        name = (xRange.Cells[i-flag, 4].Value2).ToString();
                        parentId = (xRange.Cells[i-flag, 2].Value2).ToString();
                    }
                    else
                    {
                        flag = 0;
                        parentId = "";
                    }
                    string manufacturerPartNo = (xRange.Cells[i, 5].Value2) == null ? "" : (xRange.Cells[i, 5].Value2).ToString();//Manufacturer Part No
                    string description = (xRange.Cells[i, 6].Value2) == null ? "" : (xRange.Cells[i, 6].Value2).ToString();//Description
                    string reference = (xRange.Cells[i, 7].Value2) == null ? "" : (xRange.Cells[i, 7].Value2).ToString();//Reference
                    string manufacturer = (xRange.Cells[i, 10].Value2) == null ? "" : (xRange.Cells[i, 10].Value2).ToString();//Manufacturer
                    
                    long? totalQty = (xRange.Cells[i, 9].Value2) == null ? 0 : Convert.ToInt64(xRange.Cells[i, 9].Value2);//MOQ
                    long? reelqty = (xRange.Cells[i, 8].Value2) == null ? 0 : Convert.ToInt64(xRange.Cells[i, 8].Value2);//pkg set
                    long? setqty = 0;//(xRange.Cells[i, 8].Value2) == null ? 0 : Convert.ToInt64(xRange.Cells[i, 8].Value2);//MPQ
                    if (totalQty != 0 && reelqty != 0)
                    {
                        setqty = totalQty/reelqty;
                    }
                    var bom = new BomUpload
                    {
                        Model = model,
                        YwxPartNo = ywxPartNo,
                        Name = name,
                        ManufacturerPartNo = manufacturerPartNo,
                        Description = description,
                        Reference = reference,
                        ReelQty = reelqty,
                        TotalQty = totalQty,
                        SetQty = setqty,
                        Manufacturer = manufacturer,
                        AddedDate = DateTime.Now,
                        AddedBy = 0,
                        AddedByName = "Bom Uploader Processor",
                        ParentId = parentId,
                        //MPQ=mpq,
                        //UsagePcba = usagePcba,
                        //TotalUsage = totalUsage,
                        //PkgSet = pkgset,
                        //Moq = moq

                    };

                    _cellPhoneProjectEntities.BomUploads.Add(bom);
                    _cellPhoneProjectEntities.SaveChanges();

                    progressBar1.Value = progressBar1.Value + 1;
                    progressBar1.Dispatcher.Invoke(() => progressBar1.Value = progressBar1.Value, DispatcherPriority.Background);
                }
            }
        }

        private void Browse_Click(object sender, RoutedEventArgs e)
        {
            var file = new OpenFileDialog(); //open dialog to choose file 
            if (file.ShowDialog() == System.Windows.Forms.DialogResult.OK) //if there is a file choosen by the user  
            {
                string filePath = file.FileName;
                string fileExt = Path.GetExtension(filePath);
                if (fileExt.CompareTo(".xls") == 0 || fileExt.CompareTo(".xlsx") == 0)
                {
                    try
                    {
                        FilePathBox.Text = filePath;
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show(ex.Message);
                    }
                }
                else
                {
                    System.Windows.Forms.MessageBox.Show("Please choose .xls or .xlsx file only.", "Warning", MessageBoxButtons.OK, MessageBoxIcon.Error); //custom messageBox to show error  
                    FilePathBox.Text = "";
                }
            }
        }

        private void Upload_Click(object sender, RoutedEventArgs e)
        {
            var model = "Primo G8i";//ProjectComboBox.SelectedItem == null ? "Select" : ProjectComboBox.SelectedItem.ToString();//"Primo G8i";//
            if (model != "Select")
            {
                var check = _cellPhoneProjectEntities.BomUploads.FirstOrDefault(x => x.Model == model);
                if (check == null)
                {
                    string filePath = FilePathBox.Text;
                    if (!string.IsNullOrWhiteSpace(filePath))
                    {
                        string fileExt = Path.GetExtension(filePath); //get the file extension
                        ReadExcel(filePath, fileExt); //read excel file
                        progressBar1.Value = 0;// reset progressbar value
                        FilePathBox.Text = string.Empty;
                        ProjectComboBox.SelectedIndex = 0;
                    }
                    else
                    {
                        System.Windows.Forms.MessageBox.Show("Please select a file to upload", "Warning", MessageBoxButtons.OK, MessageBoxIcon.Error); //custom messageBox to show error  
                    }
                }
                else
                {
                    System.Windows.Forms.MessageBox.Show("Bom for this model already updated", "Warning", MessageBoxButtons.OK, MessageBoxIcon.Error); //custom messageBox to show error   
                }
            }
            else
            {
                System.Windows.Forms.MessageBox.Show("Please choose a Model", "Warning", MessageBoxButtons.OK, MessageBoxIcon.Error); //custom messageBox to show error  
            }
        }

        private void ProjectComboBox_SelectionChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
        {

        }

        private void ProjectComboBox_Load()
        {
            var model = _wmrpProductionEntities.ProductModels.OrderBy(x => x.ModelName).ToList();
            foreach (var m in model)
            {
                ProjectComboBox.Items.Add(m.ModelName);
            }
        }
    }
}
