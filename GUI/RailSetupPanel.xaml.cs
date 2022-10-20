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
using static AISIN_WFA.Util.AisinEnums;

namespace AISIN_WFA.GUI
{
    /// <summary>
    /// Interaction logic for RailSetupPanel.xaml
    /// </summary>
    public partial class RailSetupPanel : UserControl
    {
        public RailSetupPanel()
        {
            InitializeComponent();
            BindingModels();
        }
        
        private void BindingModels()
        {
            try
            {
                cbFrontLane1.ItemsSource = Enum.GetValues(typeof(eLaneRail));
                cbFrontLane2.ItemsSource = Enum.GetValues(typeof(eLaneRail));
                RailSetupGrid.DataContext = AisinCollections.Instance.AisinSetup.LaneRailSetup;

            }
            catch (Exception ex)
            {
                HLog.log("ERROR", string.Format("BindingModels() {0}", ex.Message));
                throw;
            }
        }
    }
}
