using AISIN_WFA.Util;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AISIN_WFA.Setup
{
    public class LaneRailSetup : INotifyPropertyChanged
    {
#pragma warning disable CS0067
        public event PropertyChangedEventHandler PropertyChanged;
#pragma warning restore CS0067

        public AisinEnums.eLaneRail Lane1Rail { get; set; } = AisinEnums.eLaneRail.Rail1;
        public AisinEnums.eLaneRail Lane2Rail { get; set; } = AisinEnums.eLaneRail.Rail2;

    }
}
