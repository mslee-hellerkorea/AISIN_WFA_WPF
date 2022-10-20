using AISIN_WFA.Model;
using AISIN_WFA.Util;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace AISIN_WFA.Setup
{
    public class SetupControl
    {
        public void SaveSetupToFile()
        {
            try
            {
                string paramsJsonText = JsonConvert.SerializeObject(AisinCollections.Instance.AisinSetup, Formatting.Indented);
                if (!Directory.Exists(AisinParms.SETUP_FILE_DIRECTORY))
                {
                    Directory.CreateDirectory(AisinParms.SETUP_FILE_DIRECTORY);
                }
                File.WriteAllText(AisinParms.SETUP_FILE_DIRECTORY + AisinParms.SETUP_FILE_NAME, paramsJsonText);
                HLog.log("INFO", "Setup save to file...");
            }
            catch (Exception ex)
            {
                HLog.log("ERROR", string.Format("SaveSetupToFile() {0}", ex.Message));
            }
        }

        public void ReadSetupFromFile()
        {
            try
            {
                if (File.Exists(AisinParms.SETUP_FILE_DIRECTORY + AisinParms.SETUP_FILE_NAME))
                {
                    string paramsJsonText = File.ReadAllText(AisinParms.SETUP_FILE_DIRECTORY + AisinParms.SETUP_FILE_NAME);
                    try
                    {
                        AisinCollections.Instance.AisinSetup = JsonConvert.DeserializeObject<AisinSetup>(paramsJsonText, new JsonSerializerSettings
                        {
                            NullValueHandling = NullValueHandling.Ignore,
                            MissingMemberHandling = MissingMemberHandling.Ignore,
                            Error = HandleDeserializationError
                        });
                    }
                    catch (InvalidOperationException ex)
                    {
                        HLog.log("ERROR", ex.Message);
                    }
                }
                else
                {
                    string paramsJsonText = JsonConvert.SerializeObject(AisinCollections.Instance.AisinSetup, Formatting.Indented);
                    if (!Directory.Exists(AisinParms.SETUP_FILE_DIRECTORY))
                    {
                        Directory.CreateDirectory(AisinParms.SETUP_FILE_DIRECTORY);
                    }
                    File.WriteAllText(AisinParms.SETUP_FILE_DIRECTORY + AisinParms.SETUP_FILE_NAME, paramsJsonText);
                    HLog.log("INFO", "Setup created to file...");
                }
            }
            catch (Exception ex)
            {
                HLog.log("ERROR", string.Format("ReadSetupFromFile() {0}", ex.Message));
            }
        }

        private void HandleDeserializationError(object sender, Newtonsoft.Json.Serialization.ErrorEventArgs errorArgs)
        {
            errorArgs.ErrorContext.Handled = true;
            string msg = string.Format("JsonConvert.DeserializeObject path {0} failure {1}", AisinParms.SETUP_FILE_DIRECTORY + AisinParms.BARCODE_TABLE_NAME, errorArgs.ErrorContext.Error.Message);
            MessageBox.Show(msg, "Setup.json", MessageBoxButtons.OK);
            throw new InvalidOperationException(msg);
        }
    }
}
