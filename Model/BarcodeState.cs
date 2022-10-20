using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AISIN_WFA.Model
{
    public class BarcodeState : INotifyPropertyChanged
    {
#pragma warning disable CS0067
        public event PropertyChangedEventHandler PropertyChanged;
#pragma warning restore CS0067

        public bool BASignalLane1 { get; set; } = false;
        public bool BASignalLane2 { get; set; } = false;

        public bool BarcodeRecipeEmptyDisplayed { get; set; } = false;
        public string NextRecipeToLoad { get; set; } = string.Empty;
        public bool WaitForOvenEmptyToLoadRecipe { get; set; } = false;

        public ObservableCollection<BarcodeRecipe> BarcodeRecipeList { get; set; } = new ObservableCollection<BarcodeRecipe>();
    }
}
