using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AISIN_WFA.Model
{
    public class DownstreamPlcData : INotifyPropertyChanged
    {
#pragma warning disable CS0067
        public event PropertyChangedEventHandler PropertyChanged;
#pragma warning restore CS0067

        public float Lane1Width { get; set; } = 0;
        public float Lane2Width { get; set; } = 0;
    }
}
