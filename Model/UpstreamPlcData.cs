using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AISIN_WFA.Model
{
    public class UpstreamPlcData : INotifyPropertyChanged
    {
#pragma warning disable CS0067
        public event PropertyChangedEventHandler PropertyChanged;
#pragma warning restore CS0067

        public ObservableDictionary<int, int> BarcodeIntLane1 { get; set; } = new ObservableDictionary<int, int>();
        public ObservableDictionary<int, int> BarcodeIntLane2 { get; set; } = new ObservableDictionary<int, int>();
        public string BarcodeDigitLane1 { get; set; } = string.Empty;
        public string BarcodeDigitLane2 { get; set; } = string.Empty;
        public string BarcodeStringLane1 { get; set; } = string.Empty;
        public string BarcodeStringLane2 { get; set; } = string.Empty;
        public int BoadAvailableLane1 { get; set; } = 0;
        public int BoadAvailableLane2 { get; set; } = 0;

    }
}
