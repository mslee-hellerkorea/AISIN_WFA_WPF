using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AISIN_WFA.Model
{
    public class BarcodeMappingTable : INotifyPropertyChanged
    {
#pragma warning disable CS0067
        public event PropertyChangedEventHandler PropertyChanged;
#pragma warning restore CS0067

        public string BarcodePattern { get; set; } = string.Empty;
        public string Recipe { get; set; } = string.Empty;
        public string RailWidth { get; set; } = string.Empty;
        public string BeltSpeed { get; set; } = string.Empty;

    }
}
