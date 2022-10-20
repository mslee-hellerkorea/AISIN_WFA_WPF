using AISIN_WFA.Setup;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AISIN_WFA.Model
{
    public sealed class AisinCollections : INotifyPropertyChanged
    {
#pragma warning disable CS0067
        public event PropertyChangedEventHandler PropertyChanged;
#pragma warning restore CS0067

        private static readonly Lazy<AisinCollections> lazy = new Lazy<AisinCollections>(() => new AisinCollections());
        public static AisinCollections Instance { get { return lazy.Value; } }

        public AisinCollections()
        {
            AisinSetup = new AisinSetup();
            SetupControl = new SetupControl();
            OvenState = new OvenState();
            BarcodeState = new BarcodeState();
            BarcodeControl = new BarcodeControl();
            BarcodeMappingTableList = new ObservableCollection<BarcodeMappingTable>();
            UpstreamPlcData = new UpstreamPlcData();
            DownstreamPlcData = new DownstreamPlcData();
        }

        public AisinSetup AisinSetup { get; set; }

        public SetupControl SetupControl { get; set; }

        public OvenState OvenState { get; set; }

        public OcxWrapper OcxWrapper { get; set; }

        public BarcodeState BarcodeState { get; set; }

        public BarcodeControl BarcodeControl { get; set; }

        public ObservableCollection<BarcodeMappingTable> BarcodeMappingTableList { get; set; }

        public MxWrapper MxWrapperUpstreamLane1 { get; set; }
        public MxWrapper MxWrapperUpstreamLane2 { get; set; }
        public MxWrapper MxWrapperDownstreamLane1 { get; set; }
        public MxWrapper MxWrapperDownstreamLane2 { get; set; }

        public UpstreamPlcData UpstreamPlcData { get; set; }
        public DownstreamPlcData DownstreamPlcData { get; set; }
    }
}
