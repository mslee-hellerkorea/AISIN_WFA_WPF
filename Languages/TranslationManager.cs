using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Globalization;
using System.Threading;
using System.Xml.Linq;
using System.IO;

namespace AISIN_WFA.Languages
{
    class TranslationManager
    {
        private const string LanguageFilePathName = @"Languages\Language Files\HellerLanguages.xml";
        private static TranslationManager _translationManager = null;
        private static string _currentLanguage = "";

        public event EventHandler LanguageChanged;

        public static TranslationManager Instance
        {
            get
            {
                if (_translationManager == null)
                    _translationManager = new TranslationManager();
                return _translationManager;
            }
        }

        public XmlTranslationProvider XmlTranslationProvider { get; set; }

        public string ReadFileAndSetCurrentLanguage()
        {
            //Get current language from file.
            string currentLanguage;

            if (File.Exists(LanguageFilePathName))
            {
                XElement root = XElement.Load(LanguageFilePathName);
                currentLanguage = (string)
                                (from el in root.Descendants("CurrentLanguage")
                                 select el).First();

                //get and set culture
                string currentCulture;
                XElement s = (XElement)
                        (from el in root.Elements("Language")
                         where (string)el.Element("Name") == currentLanguage
                         select el).First();
                currentCulture = (string)s.Element("Culture");

                Thread.CurrentThread.CurrentCulture = new CultureInfo(currentCulture);
                Thread.CurrentThread.CurrentUICulture = new CultureInfo(currentCulture);

                _currentLanguage = currentLanguage;

                Instance.XmlTranslationProvider.LoadLanguageFile(currentLanguage);
            }
            else
                _currentLanguage = currentLanguage = "English";

            OnLanguageChanged();

            return currentLanguage;
        }

        public void UpdateFileAndSetCurrentLanguage(string currentLanguage)
        {
            if (File.Exists(LanguageFilePathName))
            {
                XElement root = XElement.Load(LanguageFilePathName);
                root.SetElementValue("CurrentLanguage", currentLanguage);

                //get and set culture
                string currentCulture;
                XElement s = (XElement)
                        (from el in root.Elements("Language")
                         where (string)el.Element("Name") == currentLanguage
                         select el).First();
                currentCulture = (string)s.Element("Culture");

                Thread.CurrentThread.CurrentCulture = new CultureInfo(currentCulture);
                Thread.CurrentThread.CurrentUICulture = new CultureInfo(currentCulture);

                _currentLanguage = currentLanguage;

                //save file
                root.Save(LanguageFilePathName);
            }
        }

        public string CurrentLanguage
        {
            get
            {
                return _currentLanguage;
            }
            set
            {
                if (value != null)
                {
                    UpdateFileAndSetCurrentLanguage(value.ToString());
                    Instance.XmlTranslationProvider.LoadLanguageFile(value.ToString());
                }
                else
                    _currentLanguage = "English";

                OnLanguageChanged();
            }
        }

        public IEnumerable<string> Languages
        {
            get
            {
                if (XmlTranslationProvider != null)
                    return XmlTranslationProvider.Languages;
                else
                    return Enumerable.Empty<string>();
            }
        }

        private void OnLanguageChanged()
        {
            LanguageChanged?.Invoke(this, EventArgs.Empty);
        }

        public object Translate(string key)
        {
            if (XmlTranslationProvider != null)
            {
                object translatedValue = XmlTranslationProvider.Translate(key);
                if (translatedValue != null)
                    return translatedValue;
            }
            return string.Format("{0}", key);
        }
    }
}
