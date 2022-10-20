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
    /// Interaction logic for SetupPanel.xaml
    /// </summary>
    public partial class SetupPanel : UserControl
    {
        public SetupPanel()
        {
            InitializeComponent();
        }

        private void button_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                Button btn = (Button)sender;
                switch (btn.Tag)
                {
                    case "Save":
                        AisinCollections.Instance.SetupControl.SaveSetupToFile();
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
