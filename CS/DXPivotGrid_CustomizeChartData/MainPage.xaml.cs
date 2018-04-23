using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Windows.Controls;
using System.Xml.Serialization;
using DevExpress.Xpf.PivotGrid;

namespace DXPivotGrid_CustomizeChartData {
    public partial class MainPage : UserControl {
        string dataFileName = "DXPivotGrid_CustomizeChartData.nwind.xml";
        Dictionary<string, string> productEncodeTable;
        Dictionary<string, string> ProductEncodeTable {
            get {
                if (productEncodeTable == null)
                    productEncodeTable = new Dictionary<string, string>();
                return productEncodeTable;
            }
        }
        Dictionary<string, string> categoryEncodeTable;
        Dictionary<string, string> CategoryEncodeTable {
            get {
                if (categoryEncodeTable == null)
                    categoryEncodeTable = new Dictionary<string, string>();
                return categoryEncodeTable;
            }
        }
        void CreateEncodeTables() {
            int customerCounter = 1;
            foreach (object value in fieldCustomer.GetUniqueValues()) {
                ProductEncodeTable.Add(value.ToString(), "Customer" + customerCounter++);
            }

            int countryCounter = 1;
            foreach (object value in fieldCountry.GetUniqueValues()) {
                CategoryEncodeTable.Add(value.ToString(), "Country" + countryCounter++);
            }
        }
        public MainPage() {
            InitializeComponent();

            // Parses an XML file and creates a collection of data items.
            Assembly assembly = Assembly.GetExecutingAssembly();
            Stream stream = assembly.GetManifestResourceStream(dataFileName);
            XmlSerializer s = new XmlSerializer(typeof(OrderData));
            object dataSource = s.Deserialize(stream);

            // Binds a pivot grid to this collection.
            pivotGridControl1.DataSource = dataSource;

            pivotGridControl1.ChartProvideRowFieldValuesAsType = typeof(string);
            pivotGridControl1.ChartProvideColumnFieldValuesAsType = typeof(string);
            pivotGridControl1.ChartProvideCellValuesAsType = typeof(int);
            
            CreateEncodeTables();

            chartControl1.Diagram.SeriesDataMember = "Series";
            chartControl1.Diagram.SeriesTemplate.ArgumentDataMember = "Arguments";
            chartControl1.Diagram.SeriesTemplate.ValueDataMember = "Values";
            chartControl1.UpdateData();

            fieldCountry.CollapseAll();
            fieldCountry.ExpandValue("Austria");
        }
        #region #CustomChartDataSourceData
        private void pivotGridControl1_CustomChartDataSourceData(object sender, 
        PivotCustomChartDataSourceDataEventArgs e) {
            if(e.ItemType == PivotChartItemType.RowItem) {
                if(e.FieldValueInfo.Field == fieldCountry) {
                    e.Value = CategoryEncodeTable[e.FieldValueInfo.Value.ToString()];
                }
                else if (e.FieldValueInfo.Field == fieldCustomer) {
                    string product =  
                        ProductEncodeTable[e.FieldValueInfo.Value.ToString()];
                    string category =
                        CategoryEncodeTable[
                            e.FieldValueInfo.GetHigherLevelFieldValue(fieldCountry).ToString()];
                    e.Value = product + '[' + category + ']';
                }
            }
            if(e.ItemType == PivotChartItemType.ColumnItem) {
                if(e.FieldValueInfo.ValueType == FieldValueType.GrandTotal) {
                    e.Value = "Total Freight";
                }
            }
            if(e.ItemType == PivotChartItemType.CellItem) {
                e.Value = Math.Round(Convert.ToDecimal(e.CellInfo.Value), 0);
            }
        }
        #endregion #CustomChartDataSourceData
    }
}
