using AISIN_WFA.Model;
using AISIN_WFA.Threads;
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

namespace AISIN_WFA
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            Title = Constant.SOFTWARE_NAME + " " + Constant.REVISION;
            HLog.StartupPath = AppDomain.CurrentDomain.BaseDirectory + "Logs\\";
            AisinManager.Instance.init();
            InitializeComponent();
        }

        private void Window_Closed(object sender, EventArgs e)
        {
            AisinCollections.Instance.OcxWrapper.SetSmema(0, 0);
            AisinCollections.Instance.OcxWrapper.SetSmema(1, 0);
            AisinManager.Instance.Shutdown();

            AisinCollections.Instance.OcxWrapper = null;

            if (AisinCollections.Instance.MxWrapperUpstreamLane1 != null)
            {
                AisinCollections.Instance.MxWrapperUpstreamLane1.Close();
                AisinCollections.Instance.MxWrapperUpstreamLane1 = null;
            }
            if (AisinCollections.Instance.MxWrapperUpstreamLane2 != null)
            {
                AisinCollections.Instance.MxWrapperUpstreamLane2.Close();
                AisinCollections.Instance.MxWrapperUpstreamLane2 = null;

            }
            if (AisinCollections.Instance.MxWrapperDownstreamLane1 != null)
            {
                AisinCollections.Instance.MxWrapperDownstreamLane1.Close();
                AisinCollections.Instance.MxWrapperDownstreamLane1 = null;
            }
            if (AisinCollections.Instance.MxWrapperDownstreamLane2 != null)
            {
                AisinCollections.Instance.MxWrapperDownstreamLane2.Close();
                AisinCollections.Instance.MxWrapperDownstreamLane2 = null;
            }

            App.Current.Shutdown();
        }
    }
}
