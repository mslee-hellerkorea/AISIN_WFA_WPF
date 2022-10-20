using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AISIN_WFA.Model
{
    public sealed class AisinParameters : INotifyPropertyChanged
    {
#pragma warning disable CS0067
        public event PropertyChangedEventHandler PropertyChanged;
#pragma warning restore CS0067
        private static readonly Lazy<AisinParameters> lazy = new Lazy<AisinParameters>(() => new AisinParameters());
        public static AisinParameters Instance { get { return lazy.Value; } }
        public AisinParameters()
        {

        }

        public bool RunFlag { get; set; }
    }
}
