using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AISIN_WFA.Setup
{
    public class OmronPlcSetup : INotifyPropertyChanged
    {
#pragma warning disable CS0067
        public event PropertyChangedEventHandler PropertyChanged;
#pragma warning restore CS0067

        public string UpstreamPlcTag { get; set; } = "RE1inAMCV1";
        public string DownstreamPlcTag { get; set; } = "RE1outSTCV1";
        public string TagIP { get; set; } = "192.168.241.9";
    }
}
