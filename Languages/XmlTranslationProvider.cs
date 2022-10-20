using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;
using System.IO;

namespace AISIN_WFA.Languages
{
    class XmlTranslationProvider
    {
        AisinLanguageDictionary dictAisin;

        private const string LanguageFilePath = @"Languages\Language Files\";
        private const string LanguageFilename = "AisinLanguages.xml";

        public XmlTranslationProvider()
        {
            dictAisin = new AisinLanguageDictionary();
        }

        public void LoadLanguageFile(string language)
        {
            if (File.Exists(LanguageFilePath + LanguageFilename))
            {
                XElement root = XElement.Load(LanguageFilePath + LanguageFilename);
                XElement s = (XElement)
                        (from el in root.Elements("Language")
                         where (string)el.Element("Name") == language
                         select el).First();
                string filename = (string)s.Element("Filename");

                //read xml file
                if (File.Exists(LanguageFilePath + filename))
                {
                    FileStream fileStream = new FileStream(LanguageFilePath + filename, FileMode.Open);
                    XDocument xDocument = XDocument.Load(fileStream);

                    foreach (XElement element in xDocument.Root.Elements())
                    {

                        if (element.Name == "Message")
                        {
                            foreach (XElement el in element.Elements())
                                dictAisin.MessageDict[el.Name.LocalName] = el.Value;
                        }
                    }
                    fileStream.Close();
                }
            }
        }

        public IEnumerable<string> Languages
        {

            get
            {
                IEnumerable<string> languages;
                if (File.Exists(LanguageFilePath + LanguageFilename))
                {
                    XElement root = XElement.Load(LanguageFilePath + LanguageFilename);
                    languages = from seg in root.Descendants("Name")
                                select (string)seg;
                }
                else
                    languages = Enumerable.Empty<string>();

                return languages;
            }

        }

        public object Translate(string key)
        {
            if (dictAisin.MessageDict.ContainsKey(key))
                return dictAisin.MessageDict[key];
            else
                return "Language key not found!";
        }

    }
}
