using AISIN_WFA.Model;
using AISIN_WFA.Util;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace AISIN_WFA.Languages
{
    /// <summary>
    /// Interaction logic for LanguageSelect.xaml
    /// </summary>
    public partial class LanguageSelect : Window
    {
        public LanguageSelect()
        {
            InitializeComponent();
            if (System.ComponentModel.DesignerProperties.GetIsInDesignMode(this)) return;
            DataContext = new LanguageViewModel();
            cbSelectLanguage.SelectedItem = TranslationManager.Instance.CurrentLanguage;
        }

        private void cbSelectLanguage_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            try
            {
                SaveCurrentLanguage();
            }
            catch (Exception ex)
            {
                HLog.log("ERROR", String.Format("cbSelectLanguage_SelectionChanged -- Message: {0}", ex.Message));
            }
        }

        private void SaveCurrentLanguage()
        {
            try
            {
                AisinCollections.Instance.AisinSetup.GeneralSetup.Language = TranslationManager.Instance.CurrentLanguage;
                string jsonText = JsonConvert.SerializeObject(AisinCollections.Instance.AisinSetup, Formatting.Indented);
                if (!Directory.Exists(AisinParms.SETUP_FILE_DIRECTORY))
                {
                    Directory.CreateDirectory(AisinParms.SETUP_FILE_DIRECTORY);
                }
                File.WriteAllText(AisinParms.SETUP_FILE_DIRECTORY + AisinParms.SETUP_FILE_NAME, jsonText);
                HLog.log("INFO", "Oven Setup saved to file due to changed language...");
            }
            catch (Exception ex)
            {
                HLog.log("ERROR", String.Format("SaveCurrentLanguage -- Message: {0}", ex.Message));
            }
        }
    }
}
