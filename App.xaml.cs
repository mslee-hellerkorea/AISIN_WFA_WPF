using AISIN_WFA.Model;
using AISIN_WFA.Threads;
using AISIN_WFA.Util;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;

namespace AISIN_WFA
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        private void Application_Exit(object sender, ExitEventArgs e)
        {
            HLog.log("INFO", "HellerOven is Exited !");
            Environment.Exit(Environment.ExitCode);
        }
    }
}
