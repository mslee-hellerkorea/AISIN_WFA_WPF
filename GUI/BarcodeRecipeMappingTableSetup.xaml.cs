using AISIN_WFA.Model;
using AISIN_WFA.Util;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace AISIN_WFA.GUI
{
    /// <summary>
    /// Interaction logic for BarcodeRecipeMappingTableSetup.xaml
    /// </summary>
    public partial class BarcodeRecipeMappingTableSetup : UserControl
    {
        public BarcodeRecipeMappingTableSetup()
        {
            InitializeComponent();
            BindingModels();
        }

        private void BindingModels()
        {
            try
            {
                MappingTable.ItemsSource = AisinCollections.Instance.BarcodeMappingTableList;
            }
            catch (Exception ex)
            {
                HLog.log("ERROR", string.Format("BindingModels() {0}", ex.Message));
                throw;
            }
        }

        private void button_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                Button btn = (Button)sender;
                switch (btn.Tag)
                {
                    case "AddRow":
                        {
                            BarcodeMappingTable bcr = new BarcodeMappingTable();
                            AisinCollections.Instance.BarcodeMappingTableList.Add(bcr);
                        }
                        break;
                    case "DeleteRow":
                        {
                            BarcodeMappingTable selectBcr = new BarcodeMappingTable();
                            selectBcr = (BarcodeMappingTable)MappingTable.SelectedItem;
                            AisinCollections.Instance.BarcodeMappingTableList.Remove(selectBcr);
                        }
                        break;
                    case "Save":
                        {
                            AisinCollections.Instance.BarcodeControl.SaveBarcodeTableToFile();
                        }
                        break;
                    default:
                        break;
                }
            }
            catch (Exception ex)
            {
                HLog.log("ERROR", string.Format("button_Click() {0}", ex.Message));
            }
        }
    }
}
