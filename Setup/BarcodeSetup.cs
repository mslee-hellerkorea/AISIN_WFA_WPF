using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AISIN_WFA.Setup
{
    public class BarcodeSetup : INotifyPropertyChanged
    {
#pragma warning disable CS0067
        public event PropertyChangedEventHandler PropertyChanged;
#pragma warning restore CS0067

        public bool HoldSmemaUntilBarcode { get; set; } = true;
        public bool AutoChangeRecipeWidthSpeed { get; set; } = true;
    }
}
