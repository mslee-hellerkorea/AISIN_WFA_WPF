using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;


namespace AISIN_WFA.Model
{
    public class ObservableDictionary<TKey, TValue> : Dictionary<TKey, TValue>, INotifyPropertyChanged
    {
#pragma warning disable CS0108 // Member hides inherited member; missing new keyword
        public TValue this[TKey index]
#pragma warning restore CS0108 // Member hides inherited member; missing new keyword
        {
            get
            {
                TValue result;
                if (!TryGetValue(index, out result))
                {
                    //throw new ArgumentException("Key not found");
                }
                return result;
            }

            set
            {
                base[index] = value;
                if (this.PropertyChanged != null)
                {
                    this.PropertyChanged(this, new PropertyChangedEventArgs(Binding.IndexerName));
                }

                //if (ContainsKey(key))
                //{
                //    GetKvpByTheKey(key).Value = value;
                //}
                //else
                //{
                //    Add(key, value);
                //}
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;
    }
}
