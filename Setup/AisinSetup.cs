using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AISIN_WFA.Setup
{
    public class AisinSetup : INotifyPropertyChanged
    {
#pragma warning disable CS0067
        public event PropertyChangedEventHandler PropertyChanged;
#pragma warning restore CS0067
        public GeneralSetup GeneralSetup { get; set; }
        public OmronPlcSetup OmronPlcSetup { get; set; }
        public PlcSetup PlcSetup { get; set; }

        public LaneRailSetup LaneRailSetup { get; set; }
        public BarcodeSetup BarcodeSetup { get; set; }

        public AisinSetup()
        {
            GeneralSetup = new GeneralSetup();
            OmronPlcSetup = new OmronPlcSetup();
            PlcSetup = new PlcSetup();
            LaneRailSetup = new LaneRailSetup();
            BarcodeSetup = new BarcodeSetup();
        }
    }
}
