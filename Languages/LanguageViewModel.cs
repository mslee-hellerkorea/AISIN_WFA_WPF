using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;
using System.ComponentModel;
using System.Globalization;
using System.Reflection;

namespace AISIN_WFA.Languages
{
    class LanguageViewModel
    {
        public ICollectionView Languages { get; private set; }

        public LanguageViewModel()
        {
            if (TranslationManager.Instance.XmlTranslationProvider == null)
                TranslationManager.Instance.XmlTranslationProvider = new XmlTranslationProvider();
            Languages = CollectionViewSource.GetDefaultView(TranslationManager.Instance.Languages);
            Languages.CurrentChanged += (object s, EventArgs e) => TranslationManager.Instance.CurrentLanguage = (string)Languages.CurrentItem;
        }

    }
}
