using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static AISIN_WFA.Util.AisinEnums;

namespace AISIN_WFA.Setup
{
    public class PlcSetup : INotifyPropertyChanged
    {
#pragma warning disable CS0067
        public event PropertyChangedEventHandler PropertyChanged;
#pragma warning restore CS0067
        public ePlcType PlcType { get; set; } = ePlcType.MITSUBISHI;
        public int Lane1UpstreamStationNumber { get; set; } = 7;
        public int Lane2UpstreamStationNumber { get; set; } = 7;
        public int Lane1DownstreamStationNumber { get; set; } = 7;
        public int Lane2DownstreamStationNumber { get; set; } = 7;

        public string MxAddrBarcodeLane1 { get; set; } = "D0";
        public string MxAddrBarcodeLane2 { get; set; } = "D100";
        public string MxAddrBoardAvaiableLane1 { get; set; } = "D21";
        public string MxAddrBoardAvaiableLane2 { get; set; } = "D121";
        public string MxAddrRailWidthLane1 { get; set; } = "D270";
        public string MxAddrRailWidthLane2 { get; set; } = "D370";
    }
}
