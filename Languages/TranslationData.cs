using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.ComponentModel;

namespace AISIN_WFA.Languages
{
    class TranslationData : IWeakEventListener, INotifyPropertyChanged
    {
        private string _key;

        public TranslationData(string key)
        {
            _key = key;

            LangaugeChangedEventManager.AddListener(TranslationManager.Instance, this);

        }

        ~TranslationData()
        {
            LangaugeChangedEventManager.RemoveListener(TranslationManager.Instance, this);
        }

        public object Value
        {
            get
            {
                return TranslationManager.Instance.Translate(_key);
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        public bool ReceiveWeakEvent(Type managerType, object sender, EventArgs e)
        {
            if (managerType == typeof(LangaugeChangedEventManager))
            {
                OnLanguageChanged(sender, e);
                return true;
            }
            return false;
        }

        private void OnLanguageChanged(object sender, EventArgs e)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("Value"));
        }
    }
}
