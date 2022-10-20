using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AISIN_WFA.Setup
{
    public partial class GeneralSetup : INotifyPropertyChanged
    {
#pragma warning disable CS0067
        public event PropertyChangedEventHandler PropertyChanged;
#pragma warning restore CS0067

        public string Language { get; set; } = "English";
        public int OcxUpdatePeriod { get; set; } = 1000;
        public int PlcUpdatePeriod { get; set; } = 1000;
        public string LogFilePath { get; set; } = @"C:\Heller Industries\AISIN Line Comm\Logs";
    }
}
