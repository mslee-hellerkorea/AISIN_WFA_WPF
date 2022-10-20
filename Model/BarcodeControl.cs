using AISIN_WFA.Util;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace AISIN_WFA.Model
{
    public class BarcodeControl
    {
        private int beltCount = 0;

        private Thread waitForOvenEmptyToChangeWidthorSpeed = null;

        public string GetBarcodeString(byte[] barcodeByte)
        {
            string barcode = string.Empty;

            try
            {
                barcode = System.Text.Encoding.Default.GetString(barcodeByte);
                barcode = Encoding.ASCII.GetString(barcodeByte, 0, barcodeByte.Length);
                int offset = barcode.IndexOf("\r\n");
                if (offset < 0)
                    offset = barcode.IndexOf('\r');
                if (offset < 0)
                    offset = barcode.IndexOf('\n');
                if (offset > 0)
                    barcode = barcode.Substring(0, offset);

                barcode = barcode.Trim().Replace(" ", "");
            }
            catch (Exception e)
            {
                return string.Empty;
            }

            return barcode;
        }

        private string GetCurrentRecipeName()
        {
            string sResult = AisinCollections.Instance.OcxWrapper.GetRecipePath();
            int offset = sResult.LastIndexOf('\\');
            if (offset >= 0)
                sResult = sResult.Substring(offset + 1);
            //LogWrite("Current recipe name=" + sResult);
            return sResult;
        }

        public void CheckBarcodeRecipe(int lane, string barcodeFromUp)
        {
            string currentJob = GetCurrentRecipeName();
            bool recipeBarcodeFound = false;
            float beltSpeedChanged = -1;    // -1 means needn't change belt speed. other means the target belt speed
            int beltSpeedChangeRemark = 0;  //  0 means needn't change, 1 means change lane1, 2 means change lane2
            float railWidthChanged = -1;    // -1 means needn't change belt speed. other means the target belt speed
            int railWidthChangeRemark = 0;  //  0 means needn't change, 1 means change lane1, 2 means change lane2
            if (AisinCollections.Instance.BarcodeState.BarcodeRecipeList ==  null || 
                AisinCollections.Instance.BarcodeState.BarcodeRecipeList.Count == 0)
            {
                if (!AisinCollections.Instance.BarcodeState.BarcodeRecipeEmptyDisplayed)
                {
                    AisinCollections.Instance.BarcodeState.BarcodeRecipeEmptyDisplayed = true;
                    //LogWrite("Barcode mapping list is empty.");
                    if (MessageBox.Show("Barcode Recipe table is empty, please fill it !", "Barcode error", MessageBoxButtons.OK, MessageBoxIcon.Exclamation) == DialogResult.OK)
                        AisinCollections.Instance.BarcodeState.BarcodeRecipeEmptyDisplayed = false;
                }
            }
            else
            {
                foreach (BarcodeRecipe bar_rec in AisinCollections.Instance.BarcodeState.BarcodeRecipeList)
                {
                    float bar_rec_speed = Convert.ToSingle(bar_rec.beltSpeed);
                    float bar_rec_width = Convert.ToSingle(bar_rec.beltWidth);

                    string currentRecipe = currentJob.ToLower().Replace(".job", "");
                    string barcodeRecipe = bar_rec.recipe.ToLower().Replace(".job", "");
                    //LogWrite("current recipe: " + currentRecipe + ", barcode recipe: " + barcodeRecipe);
                    // if recipe from list matches current job
                    if (currentRecipe.Equals(barcodeRecipe))    //ver1.0.61
                    {
                        //LogWrite("Current recipe equals barcode recipe");
                        //LogWrite("barcode from mapping table: " + bar_rec.barcode + ", barcode from upstream: " + barcodeFromUp);
                        bool match = RegexLib.IsValidCurrency(barcodeFromUp, bar_rec.barcode);


                        if (match)
                        {
                            recipeBarcodeFound = true;
                            //LogWrite("Barcode=" + barcodeFromUp + " matches to current Recipe=" + currentRecipe);

                            // check lane belt speed or belt width
                            if (bar_rec_speed != -1 && bar_rec_speed != AisinCollections.Instance.OvenState.BeltSpeedSP[lane])
                            {
                                beltSpeedChanged = bar_rec_speed;
                                beltSpeedChangeRemark = 2;
                                //LogWrite(string.Format("Lane{0}, current belt speed setpoint is {1}, belt speed setpoint in the mapping table is {2}. They doesn't match !", lane, currentBeltSpeed[1].ToString(), bar_rec.beltSpeed.ToString()));
                            }
                            if (bar_rec_width != -1 && bar_rec_width != AisinCollections.Instance.OvenState.RailWidthSP[lane])
                            {
                                railWidthChanged = bar_rec_width;
                                railWidthChangeRemark = 2;
                                //LogWrite(string.Format("Lane{0}, current rail width setpoint is {1}, belt width setpoint in the mapping table is {2}. They doesn't match !", lane, currentBeltWidth[1].ToString(), bar_rec.beltWidth.ToString()));
                            }

                            // All lanes needn't change anything
                            if (beltSpeedChangeRemark == 0 && railWidthChangeRemark == 0)
                            {
                                //LogWrite("Barcode is allowed to release smema");
                                AisinCollections.Instance.OcxWrapper.SetSmema(lane, 0);
                            }
                            // need change belt speed and rail width for lane1 or lane2
                            else if (railWidthChangeRemark != 0 && beltSpeedChangeRemark != 0)
                            {
                                //LogWrite("Begin to change belt speed and rail width on lane: " + lane);
                                ChangeBeltSpeedAndWidth(lane, railWidthChanged, beltSpeedChanged);
                            }
                            // need change rail width for lane1 or lane2
                            else if (railWidthChangeRemark != 0)
                            {
                                //LogWrite("Begin to change rail width on lane: " + lane);
                                ChangeRailWidth(lane, railWidthChanged);
                            }
                            // need change belt speed for lane1 or lane2
                            else if (beltSpeedChangeRemark != 0)
                            {
                                //LogWrite("Begin to change belt speed on lane: " + lane);
                                ChangeBeltSpeed(lane, beltSpeedChanged);
                            }

                            break;
                        }
                    }

                    // may need to change recipe
                    else
                    {
                        bool match = RegexLib.IsValidCurrency(barcodeFromUp, bar_rec.barcode);

                        if (match)
                        {
                            recipeBarcodeFound = true;
                            AisinCollections.Instance.BarcodeState.NextRecipeToLoad = bar_rec.recipe;
                            //LogWrite("Barcode=" + barcodeFromUp + " matches to another Recipe=" + barcodeRecipe);

                            if (bar_rec_speed != -1)
                            {
                                beltSpeedChanged = bar_rec_speed;
                                //LogWrite("Also need to change belt speed to be " + beltSpeedChanged + " after recipe change");
                            }
                            if (bar_rec_width != -1)
                            {
                                railWidthChanged = bar_rec_width;
                                //LogWrite("Also need to change rail width to be " + railWidthChanged + " after recipe change");
                            }
                            //nextRecipeToLoad = 
                            ChangeRecipe(lane, AisinCollections.Instance.BarcodeState.NextRecipeToLoad, railWidthChanged, beltSpeedChanged);
                            break;
                        }
                    }
                }
                if (!recipeBarcodeFound)
                {
                    MessageBox.Show(string.Format("Barcode: {0} not found in the mapping table !", barcodeFromUp.Substring(0, 12)), "Barcode error", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
                }
            }
        }

        private void ChangeBeltSpeed(int lane, float targetSpeed)
        {
            int currentBoardsCount = AisinCollections.Instance.OcxWrapper.GetBoardInCount(lane);
            if (currentBoardsCount == 0)
            {
                // just change speed
                SetBeltSpeed(lane, targetSpeed);
                // relesae smema lane hold for this lane
                AisinCollections.Instance.OcxWrapper.SetSmema(lane, 0);
            }
            else
            {
                // start a monitor thread to change belt speed when board count is 0.
                if (waitForOvenEmptyToChangeWidthorSpeed == null)
                {
                    waitForOvenEmptyToChangeWidthorSpeed = new Thread(new ThreadStart(() =>
                    {
                        WaitForOvenEmptyToChangeWidthOrSpeed(lane, -1, targetSpeed);
                    }));
                    waitForOvenEmptyToChangeWidthorSpeed.SetApartmentState(ApartmentState.STA);
                    waitForOvenEmptyToChangeWidthorSpeed.Start();
                }
            }
        }

        private void ChangeRailWidth(int lane, float targetWidth)
        {
            int currentBoardsCount = AisinCollections.Instance.OcxWrapper.GetBoardInCount(lane);
            if (currentBoardsCount == 0)
            {
                // just change width
                SetRailWidth(lane, targetWidth);
                // relesae smema lane hold for this lane
                AisinCollections.Instance.OcxWrapper.SetSmema(lane, 0);
            }
            else
            {
                // start a monitor thread to change belt width when board count is 0.
                if (waitForOvenEmptyToChangeWidthorSpeed == null)
                {
                    waitForOvenEmptyToChangeWidthorSpeed = new Thread(new ThreadStart(() =>
                    {
                        WaitForOvenEmptyToChangeWidthOrSpeed(lane, targetWidth, -1);
                    }));
                    waitForOvenEmptyToChangeWidthorSpeed.SetApartmentState(ApartmentState.STA);
                    waitForOvenEmptyToChangeWidthorSpeed.Start();
                }
            }
        }

        private void ChangeBeltSpeedAndWidth(int lane, float targetWidth, float targetSpeed)
        {
            int currentBoardsCount = AisinCollections.Instance.OcxWrapper.GetBoardInCount(lane);
            if (currentBoardsCount == 0)
            {
                // just change width
                SetBeltSpeed(lane, targetSpeed);
                SetRailWidth(lane, targetWidth);
                // relesae smema lane hold for this lane
                AisinCollections.Instance.OcxWrapper.SetSmema(lane, 0);
            }
            else
            {
                // start a monitor thread to change belt width when board count is 0.
                if (waitForOvenEmptyToChangeWidthorSpeed == null)
                {
                    waitForOvenEmptyToChangeWidthorSpeed = new Thread(new ThreadStart(() =>
                    {
                        WaitForOvenEmptyToChangeWidthOrSpeed(lane, targetWidth, targetSpeed);
                    }));
                    waitForOvenEmptyToChangeWidthorSpeed.SetApartmentState(ApartmentState.STA);
                    waitForOvenEmptyToChangeWidthorSpeed.Start();
                }
            }
        }

        private void WaitForOvenEmptyToChangeWidthOrSpeed(int lane, float targetWidth, float targetSpeed)
        {
            while (true)
            {
                int boardCount = AisinCollections.Instance.OcxWrapper.GetBoardInCount(lane);
                if (boardCount == 0)
                {
                    if (targetWidth != -1)
                        SetRailWidth(lane, targetWidth);
                    if (targetSpeed != -1)
                        SetBeltSpeed(lane, targetSpeed);
                    AisinCollections.Instance.OcxWrapper.SetSmema(lane, 0);
                    break;
                }
                Thread.Sleep(2000);
            }
            waitForOvenEmptyToChangeWidthorSpeed = null;
        }

        private void WaitForOvenEmptyToChangeRecipe(int lane, float targetWidth, float targetSpeed, string recipeName)
        {
            while (true)
            {
                int boardCount = 0;
                for (int i = 0; i < 2; i++)
                {
                    boardCount += AisinCollections.Instance.OcxWrapper.GetBoardInCount(i);
                }
                if (boardCount == 0 && string.IsNullOrEmpty(AisinCollections.Instance.BarcodeState.NextRecipeToLoad))
                {
                    LoadRecipe(recipeName);
                    if (targetWidth != -1 || targetSpeed != -1)
                    {
                        Thread.Sleep(45 * 1000);

                        if (targetWidth != -1)
                        {
                            SetRailWidth(lane, targetWidth);
                        }
                        if (targetSpeed != -1)
                        {
                            SetBeltSpeed(lane, targetSpeed);
                        }
                    }
                    // release smema for all lanes.
                    AisinCollections.Instance.OcxWrapper.SetSmema(0, 0);
                    AisinCollections.Instance.OcxWrapper.SetSmema(1, 0);
                    break;
                }
                Thread.Sleep(2000);
            }
            AisinCollections.Instance.BarcodeState.WaitForOvenEmptyToLoadRecipe = false;
        }

        private void ChangeRecipe(int lane, string recipe, float targetWidth, float targetSpeed)
        {
            int currentBoardsCount = 0;
            // for (int i = 0; i < currentBoardsCount; i++) v1.19 MSL
            for (int i = 0; i < 2; i++) // Max Lane 4
            {
                currentBoardsCount += AisinCollections.Instance.OcxWrapper.GetBoardInCount(i);
            }
            if (currentBoardsCount == 0)
            {
                // just load recipe
                LoadRecipe(recipe);

                // any of them need to be changed after recipe loaded
                if (targetWidth != -1 || targetSpeed != -1)
                {
                    Thread.Sleep(45 * 1000);
                    if (targetSpeed != -1)
                    {
                        SetBeltSpeed(lane, targetSpeed);
                    }
                    if (targetWidth != -1)
                    {
                        SetRailWidth(lane, targetWidth);
                    }
                }
                AisinCollections.Instance.OcxWrapper.SetSmema(0, 0);
                AisinCollections.Instance.OcxWrapper.SetSmema(1, 0);
            }
            else
            {
                // start a monitor thread to change recipe
                if (!AisinCollections.Instance.BarcodeState.WaitForOvenEmptyToLoadRecipe)
                {
                    Task.Factory.StartNew(() =>
                    {
                        AisinCollections.Instance.BarcodeState.WaitForOvenEmptyToLoadRecipe = true;
                        WaitForOvenEmptyToChangeRecipe(lane, targetWidth, targetSpeed, recipe);
                    });
                }
            }
        }


        public void SetBeltSpeed(int lane, float targetSpeed)
        {
            AisinCollections.Instance.OcxWrapper.SetBeltSpeed((short)(lane + beltCount - 1), targetSpeed);
            //LogWrite("Set Belt Speed on lane: " + lane + ", target Speed: " + targetSpeed.ToString("F1"));
        }

        public void SetRailWidth(int lane, float targetWidth)
        {
            AisinCollections.Instance.OcxWrapper.SetRailWidth((short)(lane + beltCount - 1), targetWidth);
            //LogWrite("Set Rail Width on lane: " + lane.ToString() + ", target width: " + targetWidth.ToString());
        }

        private void LoadRecipe(string recipeName)
        {
            recipeName = "c:\\oven\\recipe files\\" + recipeName + ".job";
            int iResult = AisinCollections.Instance.OcxWrapper.LoadRecipe(recipeName);
            //LogWrite("Loading recipe: " + recipeName + " Result=" + iResult.ToString());
            AisinCollections.Instance.BarcodeState.NextRecipeToLoad = string.Empty;

        }

        public void SaveBarcodeTableToFile()
        {
            try
            {
                string paramsJsonText = JsonConvert.SerializeObject(AisinCollections.Instance.BarcodeMappingTableList, Formatting.Indented);
                if (!Directory.Exists(AisinParms.SETUP_FILE_DIRECTORY))
                {
                    Directory.CreateDirectory(AisinParms.SETUP_FILE_DIRECTORY);
                }
                File.WriteAllText(AisinParms.SETUP_FILE_DIRECTORY + AisinParms.BARCODE_TABLE_NAME, paramsJsonText);
                HLog.log("INFO", "Barcode Table save to file...");
            }
            catch (Exception ex)
            {
                HLog.log("ERROR", string.Format("SaveBarcodeTableToFile() {0}", ex.Message));
            }
        }

        public void ReadBarcodeTableFromFile()
        {
            try
            {
                if (File.Exists(AisinParms.SETUP_FILE_DIRECTORY + AisinParms.BARCODE_TABLE_NAME))
                {
                    string paramsJsonText = File.ReadAllText(AisinParms.SETUP_FILE_DIRECTORY + AisinParms.BARCODE_TABLE_NAME);
                    try
                    {
                        AisinCollections.Instance.BarcodeMappingTableList = JsonConvert.DeserializeObject<ObservableCollection<BarcodeMappingTable>>(paramsJsonText, new JsonSerializerSettings
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
                    string paramsJsonText = JsonConvert.SerializeObject(AisinCollections.Instance.BarcodeMappingTableList, Formatting.Indented);
                    if (!Directory.Exists(AisinParms.SETUP_FILE_DIRECTORY))
                    {
                        Directory.CreateDirectory(AisinParms.SETUP_FILE_DIRECTORY);
                    }
                    File.WriteAllText(AisinParms.SETUP_FILE_DIRECTORY + AisinParms.BARCODE_TABLE_NAME, paramsJsonText);
                    HLog.log("INFO", "Barcode table created to file...");
                }
            }
            catch (Exception ex)
            {
                HLog.log("ERROR", string.Format("ReadBarcodeTableFromFile() {0}", ex.Message));
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
