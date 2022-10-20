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
    /// Interaction logic for BottomStatePanel.xaml
    /// </summary>
    public partial class BottomStatePanel : UserControl
    {
        public BottomStatePanel()
        {
            InitializeComponent();
            BindingModels();
        }

        private void BindingModels()
        {
            try
            {
                Indicator_HC2.DataContext = AisinCollections.Instance.OvenState;
            }
            catch (Exception ex)
            {
                HLog.log("ERROR", string.Format("BindingModels() {0}", ex.Message));
                throw;
            }
        }
    }
}
