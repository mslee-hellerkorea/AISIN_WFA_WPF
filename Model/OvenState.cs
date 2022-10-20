using System.ComponentModel;

namespace AISIN_WFA.Model
{
    public class OvenState : INotifyPropertyChanged
    {
        public bool IsHc2Alive { get; set; } = false;
        public string LighTower { get; set; } = string.Empty;
        public string RecipeName { get; set; } = string.Empty;
        public ObservableDictionary<int, float> RailWidthSP { get; set; } = new ObservableDictionary<int, float>();
        public ObservableDictionary<int, float> RailWidthPV { get; set; } = new ObservableDictionary<int, float>();
        public ObservableDictionary<int, float> BeltSpeedSP { get; set; } = new ObservableDictionary<int, float>();
        public ObservableDictionary<int, float> BeltSpeedPV { get; set; } = new ObservableDictionary<int, float>();
        public ObservableDictionary<int, float> ProcessedCount { get; set; } = new ObservableDictionary<int, float>();
        public ObservableDictionary<int, float> InOvenCount { get; set; } = new ObservableDictionary<int, float>();
        public ObservableDictionary<int, float> TopZoneTemperatureSP { get; set; } = new ObservableDictionary<int, float>();
        public ObservableDictionary<int, float> BottomZoneTemperatureSP { get; set; } = new ObservableDictionary<int, float>();
        public ObservableDictionary<int, float> TopZoneTemperaturePV { get; set; } = new ObservableDictionary<int, float>();
        public ObservableDictionary<int, float> BottomZoneTemperaturePV { get; set; } = new ObservableDictionary<int, float>();
        public ObservableDictionary<int, float> ZoneTemperatureSP { get; set; } = new ObservableDictionary<int, float>();
        public ObservableDictionary<int, float> OxgenPPM { get; set; } = new ObservableDictionary<int, float>();

#pragma warning disable CS0067
        public event PropertyChangedEventHandler PropertyChanged;
#pragma warning restore CS0067
    }
}
