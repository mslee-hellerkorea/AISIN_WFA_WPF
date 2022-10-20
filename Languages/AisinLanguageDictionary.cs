using System.Collections.Generic;
using System.Resources;
using System.Collections;

namespace AISIN_WFA.Languages
{
    class AisinLanguageDictionary
    {
        public Dictionary<string, string> MessageDict { get; set; } = new Dictionary<string, string>();

        public AisinLanguageDictionary()
        {
            InitializeOvenDictionaryToDefault();
        }

        public void InitializeOvenDictionaryToDefault()
        {
#if false // fjn -- cannot use Visual Studio solution folder layout
            string path = @"..\..\..\Languages\";
#else
            string path = "";
#endif
            ResXResourceReader rsxr = new ResXResourceReader(path + "Aisin.resx");
            IDictionaryEnumerator id = rsxr.GetEnumerator();
            while (id.MoveNext())
                MessageDict[id.Key.ToString()] = id.Value.ToString();
            rsxr.Close();
        }
    }
}
